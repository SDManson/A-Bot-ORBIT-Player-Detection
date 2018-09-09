using System.Collections.Generic;
using WindowsInput.Native;
using BotEngine.Motor;
using Sanderling.Motor;
using Sanderling.Parse;

namespace Sanderling.ABot.Bot.Task
{
	public class OrbitTarghet : IBotTask
	{
		public IOverviewEntry target;
		public IShipUiTarget targetLocked;
		public IEnumerable<IBotTask> Component => null;

		public IEnumerable<MotionParam> Effects
		{
			get
			{
				var maiuscKEY = VirtualKeyCode.VK_W;

				yield return maiuscKEY.KeyDown();

				if (targetLocked != null)
					yield return targetLocked.MouseClick(MouseButtonIdEnum.Left);
				else
					yield return target.MouseClick(MouseButtonIdEnum.Left);

				yield return maiuscKEY.KeyUp();
			}
		}
	}
}