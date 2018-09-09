using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;
using BotEngine.Interface;
using Sanderling.ABot.Bot.Memory;
using Sanderling.ABot.Bot.Task;
using Sanderling.ABot.Serialization;
using Sanderling.Accumulator;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;
using Sanderling.Parse;
using IMemoryMeasurement = Sanderling.Parse.IMemoryMeasurement;
using IShipUiModule = Sanderling.Accumulation.IShipUiModule;

namespace Sanderling.ABot.Bot
{
	public class Bot
	{
		public static readonly Func<long> GetTimeMilli = Glob.StopwatchZaitMiliSictInt;

		public readonly MemoryMeasurementAccumulator MemoryMeasurementAccu = new MemoryMeasurementAccumulator();

		private readonly IDictionary<long, int> MouseClickLastStepIndexFromUIElementId = new Dictionary<long, int>();

		public readonly OverviewMemory OverviewMemory = new OverviewMemory();

		private readonly IDictionary<IShipUiModule, int> ToggleLastStepIndexFromModule =
			new Dictionary<IShipUiModule, int>();

		private int motionId;

		private int stepIndex;
		public bool OwnAnomaly { private set; get; }
		public bool SkipAnomaly { private set; get; }

		public BotStepInput StepLastInput { private set; get; }

		public PropertyGenTimespanInt64<BotStepResult> StepLastResult { private set; get; }

		public FromProcessMeasurement<IMemoryMeasurement> MemoryMeasurementAtTime { private set; get; }

		public KeyValuePair<Deserialization, Config> ConfigSerialAndStruct { private set; get; }

		public long? MouseClickLastAgeStepCountFromUIElement(IUIElement uiElement)
		{
			if (null == uiElement)
				return null;

			var interactionLastStepIndex = MouseClickLastStepIndexFromUIElementId?.TryGetValueNullable(uiElement.Id);

			return stepIndex - interactionLastStepIndex;
		}

		public long? ToggleLastAgeStepCountFromModule(IShipUiModule module)
		{
			return stepIndex - ToggleLastStepIndexFromModule?.TryGetValueNullable(module);
		}

		public void SetOwnAnomaly(bool value)
		{
			OwnAnomaly = value;
		}

		public void SetSkipAnomaly(bool value)
		{
			SkipAnomaly = value;
		}

		private IEnumerable<IBotTask[]> StepOutputListTaskPath()
		{
			return ((IBotTask) new BotTask {Component = RootTaskListComponent()})
				?.EnumeratePathToNodeFromTreeDFirst(node => node?.Component)
				?.Where(taskPath => (taskPath?.LastOrDefault()).ShouldBeIncludedInStepOutput())
				?.TakeSubsequenceWhileUnwantedInferenceRuledOut();
		}

		private void MemorizeStepInput(BotStepInput input)
		{
			ConfigSerialAndStruct = (input?.ConfigSerial?.String).DeserializeIfDifferent(ConfigSerialAndStruct);

			MemoryMeasurementAtTime =
				input?.FromProcessMemoryMeasurement?.MapValue(measurement => measurement?.Parse());

			MemoryMeasurementAccu.Accumulate(MemoryMeasurementAtTime);

			OverviewMemory.Aggregate(MemoryMeasurementAtTime);
		}

		private void MemorizeStepResult(BotStepResult stepResult)
		{
			var setMotionMouseWaypointUIElement =
				stepResult?.ListMotion
					?.Select(motion => motion?.MotionParam)
					?.Where(motionParam => 0 < motionParam?.MouseButton?.Count())
					?.Select(motionParam => motionParam?.MouseListWaypoint)
					?.ConcatNullable()?.Select(mouseWaypoint => mouseWaypoint?.UIElement)?.WhereNotDefault();

			foreach (var mouseWaypointUIElement in setMotionMouseWaypointUIElement.EmptyIfNull())
				MouseClickLastStepIndexFromUIElementId[mouseWaypointUIElement.Id] = stepIndex;
		}

		public BotStepResult Step(BotStepInput input)
		{
			var beginTimeMilli = GetTimeMilli();

			StepLastInput = input;

			Exception exception = null;

			var listMotion = new List<MotionRecommendation>();

			IBotTask[][] outputListTaskPath = null;

			try
			{
				MemorizeStepInput(input);

				outputListTaskPath = StepOutputListTaskPath()?.ToArray();

				foreach (var moduleToggle in outputListTaskPath.ConcatNullable().OfType<ModuleToggleTask>()
					.Select(moduleToggleTask => moduleToggleTask?.module).WhereNotDefault())
					ToggleLastStepIndexFromModule[moduleToggle] = stepIndex;

				foreach (var taskPath in outputListTaskPath.EmptyIfNull())
				foreach (var effectParam in (taskPath?.LastOrDefault()?.ApplicableEffects()).EmptyIfNull())
					listMotion.Add(new MotionRecommendation
					{
						Id = motionId++,
						MotionParam = effectParam
					});
			}
			catch (Exception e)
			{
				exception = e;
			}

			var stepResult = new BotStepResult
			{
				Exception = exception,
				ListMotion = listMotion?.ToArrayIfNotEmpty(),
				OutputListTaskPath = outputListTaskPath
			};

			MemorizeStepResult(stepResult);

			StepLastResult = new PropertyGenTimespanInt64<BotStepResult>(stepResult, beginTimeMilli, GetTimeMilli());

			++stepIndex;

			return stepResult;
		}

		private IEnumerable<IBotTask> RootTaskListComponent()
		{
			return StepLastInput?.RootTaskListComponentOverride ??
			       RootTaskListComponentDefault();
		}

		private IEnumerable<IBotTask> RootTaskListComponentDefault()
		{
			yield return new BotTask {Component = EnumerateConfigDiagnostics()};

			yield return new EnableInfoPanelCurrentSystem {MemoryMeasurement = MemoryMeasurementAtTime?.Value};

			var saveShipTask = new SaveShipTask {Bot = this};

			yield return saveShipTask;

			yield return this.EnsureIsActive(
				MemoryMeasurementAccu?.ShipUiModule?.Where(module => module.ShouldBeActivePermanent(this)));

			var moduleUnknown =
				MemoryMeasurementAccu?.ShipUiModule?.FirstOrDefault(module => null == module?.TooltipLast?.Value);

			yield return new BotTask {Effects = new[] {moduleUnknown?.MouseMove()}};

			if (!saveShipTask.AllowRoam)
				yield break;

			var combatTask = new CombatTask {bot = this};

			yield return combatTask;

			if (!saveShipTask.AllowAnomalyEnter)
				yield break;

			yield return new UndockTask {MemoryMeasurement = MemoryMeasurementAtTime?.Value};

			if (combatTask.Completed)
				yield return new AnomalyEnter {bot = this};
		}

		private IEnumerable<IBotTask> EnumerateConfigDiagnostics()
		{
			var configDeserializeException = ConfigSerialAndStruct.Key?.Exception;

			if (null != configDeserializeException)
				yield return new DiagnosticTask
					{MessageText = "error parsing configuration: " + configDeserializeException.Message};
			else if (null == ConfigSerialAndStruct.Value)
				yield return new DiagnosticTask {MessageText = "warning: no configuration supplied."};
		}
	}
}