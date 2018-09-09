using System.Collections.Generic;
using System.Linq;
using Sanderling.Accumulation;

namespace Sanderling.ABot.Bot.Task
{
	public static class ModuleTaskExtension
	{
		public static bool? IsActive(
			this IShipUiModule module,
			Bot bot)
		{
			if (bot?.MouseClickLastAgeStepCountFromUIElement(module) <= 1)
				return null;

			if (bot?.ToggleLastAgeStepCountFromModule(module) <= 1)
				return null;

			return module?.RampActive;
		}

		public static IBotTask EnsureIsActive(
			this Bot bot,
			IShipUiModule module)
		{
			if (module?.IsActive(bot) ?? true)
				return null;

			return new ModuleToggleTask {bot = bot, module = module};
		}

		public static IBotTask DeactiveModule(
			this Bot bot,
			IShipUiModule module)
		{
			if (module?.IsActive(bot) == false || module?.RampActive == false)
				return null;
			return new ModuleToggleTask {bot = bot, module = module};
		}


		public static IBotTask EnsureIsActive(
			this Bot bot,
			IEnumerable<IShipUiModule> setModule)
		{
			return new BotTask {Component = setModule?.Select(module => bot?.EnsureIsActive(module))};
		}

		public static IBotTask DeactivateModule(
			this Bot bot,
			IEnumerable<IShipUiModule> setModule)
		{
			return new BotTask {Component = setModule?.Select(module => bot?.DeactiveModule(module))};
		}
	}
}