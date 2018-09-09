using System.Collections.Generic;
using BotEngine.Motor;
using Sanderling.Motor;
using Sanderling.Parse;

namespace Sanderling.ABot.Bot.Task
{
	public class EnableInfoPanelCurrentSystem : IBotTask
	{
		public IMemoryMeasurement MemoryMeasurement;

		public IEnumerable<IBotTask> Component => null;

		public IEnumerable<MotionParam> Effects
		{
			get
			{
				if (null != MemoryMeasurement?.InfoPanelCurrentSystem)
					yield break;

				yield return MemoryMeasurement?.InfoPanelButtonCurrentSystem?.MouseClick(MouseButtonIdEnum.Left);
			}
		}
	}
}