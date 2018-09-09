using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Bib3;
using Bib3.RateLimit;
using Bib3.Synchronization;
using BotEngine.Interface;
using Sanderling.ABot.Bot;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;
using Sanderling.UI;
using Main = Sanderling.ABot.UI.Main;

namespace Sanderling.ABot.Exe
{
	partial class App
	{
		private const int FromMotionToMeasurementDelayMilli = 300;

		private const int MemoryMeasurementDistanceMaxMilli = 3000;

		private const string BotConfigFileName = "bot.config";

		private readonly Bot.Bot bot = new Bot.Bot();
		private readonly object botLock = new object();

		private readonly IRateLimitStateInt MemoryMeasurementRequestRateLimit = new RateLimitStateIntSingle();
		private readonly Sensor sensor = new Sensor();

		private readonly SimpleInterfaceServerDispatcher SensorServerDispatcher = new SimpleInterfaceServerDispatcher
		{
			InterfaceAppDomainSetupType = typeof(InterfaceAppDomainSetup),
			InterfaceAppDomainSetupTypeLoadFromMainModule = true,
			LicenseClientConfig = ExeConfig.LicenseClientDefault
		};

		private PropertyGenTimespanInt64<KeyValuePair<Exception, StringAtPath>> BotConfigLoaded;

		private PropertyGenTimespanInt64<MotionResult[]> BotStepLastMotionResult;

		private FromProcessMeasurement<IMemoryMeasurement> MemoryMeasurementLast;

		public static IEnumerable<IEnumerable<Key>> SetKeyBotMotionDisable => new[]
		{
			new[] {Key.LeftCtrl, Key.LeftAlt},
			new[] {Key.LeftCtrl, Key.RightAlt},
			new[] {Key.RightCtrl, Key.LeftAlt},
			new[] {Key.RightCtrl, Key.RightAlt}
		};

		private MainWindow Window => MainWindow as MainWindow;

		private Main MainControl => Window?.Main;

		private InterfaceToEve InterfaceToEveControl => Window?.Main?.Interface;

		public int? EveOnlineClientProcessId => InterfaceToEveControl?.ProcessChoice?.ChoosenProcessId;

		private void InterfaceExchange()
		{
			var eveOnlineClientProcessId = EveOnlineClientProcessId;

			var measurementRequestTime = MeasurementRequestTime() ?? 0;

			if (eveOnlineClientProcessId.HasValue && measurementRequestTime <= GetTimeStopwatch())
				if (MemoryMeasurementRequestRateLimit.AttemptPass(GetTimeStopwatch(), 700))
					Task.Run(() => botLock.IfLockIsAvailableEnter(() =>
						MeasurementMemoryTake(eveOnlineClientProcessId.Value, measurementRequestTime)));
		}

		private void MeasurementMemoryTake(int processId, long measurementBeginTimeMinMilli)
		{
			var measurement = sensor.MeasurementTake(processId, measurementBeginTimeMinMilli);

			if (null == measurement)
				return;

			MemoryMeasurementLast = measurement;
		}

		private void UIPresent()
		{
			MainControl?.Present(SensorServerDispatcher, MemoryMeasurementLast, bot);
		}

		private long? MeasurementRequestTime()
		{
			var memoryMeasurementLast = MemoryMeasurementLast;

			var botStepLastMotionResult = BotStepLastMotionResult;

			if (memoryMeasurementLast?.Begin < botStepLastMotionResult?.End &&
			    0 < botStepLastMotionResult?.Value?.Length)
				return botStepLastMotionResult?.End + FromMotionToMeasurementDelayMilli;

			return memoryMeasurementLast?.Begin + MemoryMeasurementDistanceMaxMilli;
		}

		private void BotProgress(bool motionEnable)
		{
			botLock.IfLockIsAvailableEnter(() =>
			{
				var memoryMeasurementLast = MemoryMeasurementLast;

				var time = memoryMeasurementLast?.End;

				if (!time.HasValue)
					return;

				if (time <= bot?.StepLastInput?.TimeMilli)
					return;

				BotConfigLoad();

				var stepResult = bot.Step(new BotStepInput
				{
					TimeMilli = time.Value,
					FromProcessMemoryMeasurement = memoryMeasurementLast,
					StepLastMotionResult = BotStepLastMotionResult?.Value,
					ConfigSerial = BotConfigLoaded?.Value.Value
				});

				if (motionEnable)
					BotMotion(memoryMeasurementLast, stepResult?.ListMotion);
			});
		}

		private void BotMotion(
			FromProcessMeasurement<IMemoryMeasurement> memoryMeasurement,
			IEnumerable<MotionRecommendation> sequenceMotion)
		{
			var processId = memoryMeasurement?.ProcessId;

			if (!processId.HasValue || null == sequenceMotion)
				return;

			var process = Process.GetProcessById(processId.Value);

			if (null == process)
				return;

			var startTime = GetTimeStopwatch();

			var motor = new WindowMotor(process.MainWindowHandle);

			var listMotionResult = new List<MotionResult>();

			foreach (var motion in sequenceMotion.EmptyIfNull())
			{
				var motionResult =
					motor.ActSequenceMotion(motion.MotionParam.AsSequenceMotion(memoryMeasurement?.Value));

				listMotionResult.Add(new MotionResult
				{
					Id = motion.Id,
					Success = motionResult?.Success ?? false
				});
			}

			BotStepLastMotionResult =
				new PropertyGenTimespanInt64<MotionResult[]>(listMotionResult.ToArray(), startTime, GetTimeStopwatch());

			Thread.Sleep(FromMotionToMeasurementDelayMilli);
		}

		private void BotConfigLoad()
		{
			Exception exception = null;
			string configString = null;
			var configFilePath = AssemblyDirectoryPath.PathToFilesysChild(BotConfigFileName);

			try
			{
				using (var fileStream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read))
				{
					configString = new StreamReader(fileStream).ReadToEnd();
				}
			}
			catch (Exception e)
			{
				exception = e;
			}

			BotConfigLoaded = new PropertyGenTimespanInt64<KeyValuePair<Exception, StringAtPath>>(
				new KeyValuePair<Exception, StringAtPath>(
					exception,
					new StringAtPath {Path = configFilePath, String = configString}), GetTimeStopwatch());
		}
	}
}