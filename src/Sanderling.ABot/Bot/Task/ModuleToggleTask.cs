using System.Collections.Generic;
using WindowsInput.Native;
using BotEngine.Motor;
using Sanderling.Accumulation;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot.Task
{
	public class ModuleToggleTask : IBotTask
	{
		public Bot bot;

		public IShipUiModule module;

		public IEnumerable<IBotTask> Component => null;

		public IEnumerable<MotionParam> Effects
		{
			get
			{
				var toggleKey = module?.TooltipLast?.Value?.ToggleKey;

				if (0 < toggleKey?.Length)
					yield return toggleKey?.KeyboardPressCombined();

				yield return module?.MouseClick(MouseButtonIdEnum.Left);
			}
		}

		public IBotTask ReloadAnomaly()
		{
			var ReloadAnomalyFactory = new BotTask {Component = null, Effects = ReloadAnomalyFunction()};
			return ReloadAnomalyFactory;
		}


		public IEnumerable<MotionParam> ReloadAnomalyFunction()
		{
			var APPS = VirtualKeyCode.APPS;

			yield return APPS.KeyboardPress();
			yield return APPS.KeyboardPress();
		}
	}
}