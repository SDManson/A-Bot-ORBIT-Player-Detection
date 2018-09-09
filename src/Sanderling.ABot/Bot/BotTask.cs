using System.Collections.Generic;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot
{
	public class BotTask : IBotTask
	{
		public IEnumerable<IBotTask> Component { set; get; }

		public IEnumerable<MotionParam> Effects { set; get; }
	}
}