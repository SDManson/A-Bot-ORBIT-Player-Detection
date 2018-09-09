using System.Collections.Generic;
using WindowsInput.Native;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Task
{
	public class ReloadAnomalies : IBotTask
	{
		public const string NoSuitableAnomalyFoundDiagnosticMessage =
			"no suitable anomaly found. waiting for anomaly to appear.";

		public IEnumerable<IBotTask> Component => null;

		public IEnumerable<MotionParam> Effects
		{
			get
			{
				var APPS = VirtualKeyCode.VK_P;

				yield return APPS.KeyboardPress();
				yield return APPS.KeyboardPress();
			}
		}
	}
}