using System.Collections.Generic;
using System.Linq;
using BotEngine.Common;
using Sanderling.ABot.Parse;
using Sanderling.Motor;
using Sanderling.Parse;
using IListEntry = Sanderling.Interface.MemoryStruct.IListEntry;

namespace Sanderling.ABot.Bot.Task
{
	public class AnomalyEnter : IBotTask
	{
		public const string NoSuitableAnomalyFoundDiagnosticMessage =
			"no suitable anomaly found. waiting for anomaly to appear.";

		public Bot bot;

		public IEnumerable<IBotTask> Component
		{
			get
			{
				var memoryMeasurementAtTime = bot?.MemoryMeasurementAtTime;
				var memoryMeasurementAccu = bot?.MemoryMeasurementAccu;

				var memoryMeasurement = memoryMeasurementAtTime?.Value;

				if (!memoryMeasurement.ManeuverStartPossible())
					yield break;

				var probeScannerWindow = memoryMeasurement?.WindowProbeScanner?.FirstOrDefault();

				var scanResultCombatSite =
					probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(AnomalySuitableGeneral);

				if (null == scanResultCombatSite)
					yield return new DiagnosticTask
					{
						MessageText = NoSuitableAnomalyFoundDiagnosticMessage
					};

				if (null != scanResultCombatSite)
				{
					var menuResult = memoryMeasurement?.Menu?.ToList();
					if (null == menuResult)
					{
						yield return scanResultCombatSite.ClickMenuEntryByRegexPattern(bot, "", "");
					}
					else
					{
						menuResult = memoryMeasurement?.Menu?.ToList();

						var menuResultWarp = (menuResult?[0].Entry).ToArray();
						var menuResultSelectWarpMenu = menuResultWarp?[1];
						if (menuResult.Count < 2)
						{
							yield return menuResultSelectWarpMenu.ClickMenuEntryByRegexPattern(bot, "");
						}
						else
						{
							var menuSpecificDistance = (menuResult[1]?.Entry).ToArray();
							bot?.SetSkipAnomaly(false);
							bot?.SetOwnAnomaly(false);

							yield return menuSpecificDistance[3].ClickMenuEntryByRegexPattern(bot, "within 30 km");
						}
					}
				}
			}
		}

		public IEnumerable<MotionParam> Effects => null;

		public static bool AnomalySuitableGeneral(IListEntry scanResult)
		{
			return scanResult?.CellValueFromColumnHeader("Group")?.RegexMatchSuccessIgnoreCase("combat") ?? false;
		}
	}
}