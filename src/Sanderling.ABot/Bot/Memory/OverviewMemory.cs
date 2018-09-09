using System.Collections.Generic;
using System.Linq;
using Bib3;
using BotEngine.Interface;
using Sanderling.Parse;

namespace Sanderling.ABot.Bot.Memory
{
	public class OverviewMemory
	{
		private static readonly IEnumerable<ShipManeuverTypeEnum> setManeuverReset =
			new[] {ShipManeuverTypeEnum.Warp, ShipManeuverTypeEnum.Docked, ShipManeuverTypeEnum.Jump};

		private readonly IDictionary<long, HashSet<EWarTypeEnum>> setEWarTypeFromOverviewEntryId =
			new Dictionary<long, HashSet<EWarTypeEnum>>();

		public IEnumerable<EWarTypeEnum> SetEWarTypeFromOverviewEntry(IOverviewEntry entry)
		{
			return setEWarTypeFromOverviewEntryId?.TryGetValueOrDefault(entry?.Id ?? -1);
		}

		public void Aggregate(FromProcessMeasurement<IMemoryMeasurement> memoryMeasurementAtTime)
		{
			var memoryMeasurement = memoryMeasurementAtTime?.Value;

			var overviewWindow = memoryMeasurement?.WindowOverview?.FirstOrDefault();

			foreach (var overviewEntry in (overviewWindow?.ListView?.Entry?.WhereNotDefault()).EmptyIfNull())
			{
				var setEWarType = setEWarTypeFromOverviewEntryId.TryGetValueOrDefault(overviewEntry.Id);

				foreach (var ewarType in overviewEntry.EWarType.EmptyIfNull())
				{
					if (null == setEWarType)
						setEWarType = new HashSet<EWarTypeEnum>();

					setEWarType.Add(ewarType);
				}

				if (null != setEWarType)
					setEWarTypeFromOverviewEntryId[overviewEntry.Id] = setEWarType;
			}

			if (setManeuverReset.Contains(memoryMeasurement?.ShipUi?.Indication?.ManeuverType ??
			                              ShipManeuverTypeEnum.None))
			{
				var setOverviewEntryVisibleId = overviewWindow?.ListView?.Entry?.Select(entry => entry.Id)?.ToArray();

				foreach (var entryToRemoveId in setEWarTypeFromOverviewEntryId.Keys
					.Where(entryId => !(setOverviewEntryVisibleId?.Contains(entryId) ?? false)).ToArray())
					setEWarTypeFromOverviewEntryId.Remove(entryToRemoveId);
			}
		}
	}
}