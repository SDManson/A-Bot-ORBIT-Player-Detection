using System.Linq;
using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;
using IMemoryMeasurement = Sanderling.Parse.IMemoryMeasurement;

namespace Sanderling.ABot.Parse
{
	public static class ParseExtension
	{
		/// <summary>
		///     Hobgoblin I ( <color=0 xFF00FF00>Idle</color> )
		/// </summary>
		private const string StatusStringFromDroneEntryTextRegexPattern = @"\((.*)\)";

		public static int? CountFromDroneGroupCaption(this string groupCaption)
		{
			return groupCaption?.RegexMatchIfSuccess(@"\((\d+)\)")?.Groups[1]?.Value?.TryParseInt();
		}

		public static string StatusStringFromDroneEntryText(this string droneEntryText)
		{
			return droneEntryText?.RegexMatchIfSuccess(StatusStringFromDroneEntryTextRegexPattern)?.Groups[1]?.Value
				?.RemoveXmlTag()?.Trim();
		}

		public static bool ManeuverStartPossible(this IMemoryMeasurement memoryMeasurement)
		{
			return !(memoryMeasurement?.IsDocked ?? false) &&
			       !new[] {ShipManeuverTypeEnum.Warp, ShipManeuverTypeEnum.Jump, ShipManeuverTypeEnum.Docked}.Contains(
				       memoryMeasurement?.ShipUi?.Indication?.ManeuverType ?? ShipManeuverTypeEnum.None);
		}

		public static long Width(this RectInt rect)
		{
			return rect.Side0Length();
		}

		public static long Height(this RectInt rect)
		{
			return rect.Side1Length();
		}

		public static bool IsScrollable(this IScroll scroll)
		{
			return scroll?.ScrollHandle?.Region.Height() < scroll?.ScrollHandleBound?.Region.Height() - 4;
		}

		public static bool IsNeutralOrEnemy(this IChatParticipantEntry participantEntry)
		{
			return !(participantEntry?.FlagIcon?.Any(flagIcon =>
				         new[] {"good standing", "excellent standing", "Pilot is in your (fleet|corporation|alliance)"}
					         .Any(goodStandingText =>
						         flagIcon?.HintText?.RegexMatchSuccessIgnoreCase(goodStandingText) ?? false)) ?? false);
		}
	}
}