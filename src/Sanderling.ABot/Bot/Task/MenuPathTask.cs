using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using BotEngine.Motor;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Task
{
	public class MenuPathTask : IBotTask
	{
		public Bot Bot;

		public string[][] ListMenuListPriorityEntryRegexPattern;

		public IUIElement RootUIElement;

		public IEnumerable<IBotTask> Component => null;

		public IEnumerable<MotionParam> Effects
		{
			get
			{
				var memoryMeasurement = Bot?.MemoryMeasurementAtTime?.Value;

				var listMenu = memoryMeasurement?.Menu?.ToArray();

				var rootUIElement = RootUIElement;

				if (null == rootUIElement)
					yield break;

				IMenuEntry menuEntryToContinue = null;

				var mouseClickOnRootAge = Bot?.MouseClickLastAgeStepCountFromUIElement(RootUIElement);

				if (MenuOpenOnRootPossible() && mouseClickOnRootAge <= listMenu?.Length)
				{
					var levelCount = Math.Min(ListMenuListPriorityEntryRegexPattern?.Length ?? 0,
						listMenu?.Length ?? 0);

					for (var levelIndex = 0; levelIndex < levelCount; levelIndex++)
					{
						var listPriorityEntryRegexPattern = ListMenuListPriorityEntryRegexPattern[levelIndex];

						var menuEntry =
							listPriorityEntryRegexPattern
								?.WhereNotDefault()
								?.Select(priorityEntryRegexPattern =>
									listMenu[levelIndex]?.Entry
										?.FirstOrDefault(c =>
											c?.Text?.RegexMatchSuccessIgnoreCase(priorityEntryRegexPattern) ?? false))
								?.WhereNotDefault()?.FirstOrDefault();

						if (null == menuEntry)
							break;

						menuEntryToContinue = menuEntry;

						if (!(menuEntry?.HighlightVisible ?? false))
							break;
					}
				}

				yield return
					menuEntryToContinue?.MouseClick(MouseButtonIdEnum.Left) ??
					RootUIElement?.MouseClick(MouseButtonIdEnum.Right);
			}
		}

		private bool MenuOpenOnRootPossible()
		{
			var memoryMeasurement = Bot?.MemoryMeasurementAtTime?.Value;

			var menu = memoryMeasurement?.Menu?.FirstOrDefault();

			if (null == menu)
				return false;

			var overviewEntry = RootUIElement as IOverviewEntry;

			var regionExpected = RootUIElement;

			if (null != overviewEntry)
			{
				regionExpected = memoryMeasurement?.WindowOverview?.FirstOrDefault();

				if (!(overviewEntry.IsSelected ?? false))
					return false;

				if (!(menu?.Entry?.Any(menuEntry =>
					      menuEntry?.Text?.RegexMatchSuccessIgnoreCase(@"remove.*overview") ?? false) ?? false))
					return false;
			}

			if (regionExpected.Region.Intersection(menu.Region.WithSizeExpandedPivotAtCenter(10)).IsEmpty())
				return false;

			return true;
		}
	}
}