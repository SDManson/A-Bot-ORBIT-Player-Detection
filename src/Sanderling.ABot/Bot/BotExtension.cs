using System.Collections.Generic;
using System.Linq;
using Bib3;
using BotEngine.Common;
using Sanderling.ABot.Bot.Task;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;
using Sanderling.Parse;
using IOverviewEntry = Sanderling.Parse.IOverviewEntry;
using IShipUiModule = Sanderling.Accumulation.IShipUiModule;

namespace Sanderling.ABot.Bot
{
	public static class BotExtension
	{
		private static readonly EWarTypeEnum[][] listEWarPriorityGroup =
		{
			new[] {EWarTypeEnum.ECM},
			new[] {EWarTypeEnum.Web},
			new[] {EWarTypeEnum.WarpDisrupt, EWarTypeEnum.WarpScramble}
		};

		public static int AttackPriorityIndexForOverviewEntryEWar(IEnumerable<EWarTypeEnum> setEWar)
		{
			var setEWarRendered = setEWar?.ToArray();

			return
				listEWarPriorityGroup.FirstIndexOrNull(priorityGroup => priorityGroup.ContainsAny(setEWarRendered)) ??
				listEWarPriorityGroup.Length + (0 < setEWarRendered?.Length ? 0 : 1);
		}

		public static int AttackPriorityIndex(
			this Bot bot,
			IOverviewEntry entry)
		{
			return AttackPriorityIndexForOverviewEntryEWar(bot?.OverviewMemory?.SetEWarTypeFromOverviewEntry(entry));
		}

		public static bool ShouldBeIncludedInStepOutput(this IBotTask task)
		{
			return (task?.ContainsEffect() ?? false) || task is DiagnosticTask;
		}

		public static bool LastContainsEffect(this IEnumerable<IBotTask> listTask)
		{
			return listTask?.LastOrDefault()?.ContainsEffect() ?? false;
		}

		public static IEnumerable<MotionParam> ApplicableEffects(this IBotTask task)
		{
			return task?.Effects?.WhereNotDefault();
		}

		public static bool ContainsEffect(this IBotTask task)
		{
			return 0 < task?.ApplicableEffects()?.Count();
		}

		public static IEnumerable<IBotTask[]> TakeSubsequenceWhileUnwantedInferenceRuledOut(
			this IEnumerable<IBotTask[]> listTaskPath)
		{
			return listTaskPath
				?.EnumerateSubsequencesStartingWithFirstElement()
				?.OrderBy(subsequenceTaskPath => 1 == subsequenceTaskPath?.Count(LastContainsEffect))
				?.LastOrDefault();
		}

		public static IUIElementText TitleElementText(this IModuleButtonTooltip tooltip)
		{
			var tooltipHorizontalCenter = tooltip?.RegionCenter()?.A;

			var setLabelIntersectingHorizontalCenter =
				tooltip?.LabelText
					?.Where(label =>
						label?.Region.Min0 < tooltipHorizontalCenter && tooltipHorizontalCenter < label?.Region.Max0);

			return
				setLabelIntersectingHorizontalCenter
					?.OrderByCenterVerticalDown()?.FirstOrDefault();
		}

		public static bool ShouldBeActivePermanent(this IShipUiModule module, Bot bot)
		{
			return new[]
				{
					module?.TooltipLast?.Value?.IsHardener,
					bot?.ConfigSerialAndStruct.Value?.ModuleActivePermanentSetTitlePattern
						?.Any(activePermanentTitlePattern =>
							module?.TooltipLast?.Value?.TitleElementText()?.Text
								?.RegexMatchSuccessIgnoreCase(activePermanentTitlePattern) ?? false)
				}
				.Any(sufficientCondition => sufficientCondition ?? false);
		}
	}
}