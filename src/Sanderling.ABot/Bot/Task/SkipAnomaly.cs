using System.Collections.Generic;
using System.Linq;
using BotEngine.Common;
using Sanderling.Motor;
using Sanderling.Parse;
using IListEntry = Sanderling.Interface.MemoryStruct.IListEntry;

namespace Sanderling.ABot.Bot.Task
{
	public class SkipAnomaly : IBotTask
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

				var probeScannerWindow = memoryMeasurement?.WindowProbeScanner?.FirstOrDefault();
				var scanActuallyAnomaly =
					probeScannerWindow?.ScanResultView?.Entry?.FirstOrDefault(ActuallyAnomaly);

				if (null != scanActuallyAnomaly)
					yield return scanActuallyAnomaly.ClickMenuEntryByRegexPattern(bot, "Ignore Result");
				else
					yield break;
			}
		}

		public IEnumerable<MotionParam> Effects => null;

		public static bool ActuallyAnomaly(IListEntry scanResult)
		{
			return scanResult?.CellValueFromColumnHeader("Distance")?.RegexMatchSuccessIgnoreCase("km") ?? false;
		}
	}
}