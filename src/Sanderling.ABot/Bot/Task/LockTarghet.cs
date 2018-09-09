using System.Collections.Generic;
using WindowsInput.Native;
using BotEngine.Motor;
using Sanderling.Motor;
using Sanderling.Parse;

namespace Sanderling.ABot.Bot.Task
{
	public class LockTarghet : IBotTask
	{
		public IOverviewEntry Target;
		public IEnumerable<IBotTask> Component => null;

		public IEnumerable<MotionParam> Effects
		{
			get
			{
				var ctrlKey = VirtualKeyCode.CONTROL;

				yield return ctrlKey.KeyDown();
				yield return Target.MouseClick(MouseButtonIdEnum.Left);
				yield return ctrlKey.KeyUp();
			}
		}
	}
}