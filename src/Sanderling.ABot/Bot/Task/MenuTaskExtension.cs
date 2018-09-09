using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot.Task
{
	public static class MenuTaskExtension
	{
		public static MenuPathTask ClickMenuEntryByRegexPattern(this IUIElement rootUIElement,
			Bot bot, string menuEntryRegexPattern)
		{
			return ClickMenuEntryByRegexPattern(rootUIElement, bot, menuEntryRegexPattern, "");
		}

		public static MenuPathTask ClickMenuEntryByRegexPattern(this IUIElement rootUIElement,
			Bot bot,
			string menuEntryRegexPattern, string menuNextStep)
		{
			if (null == rootUIElement)
				return null;

			return new MenuPathTask
			{
				Bot = bot,
				RootUIElement = rootUIElement,
				ListMenuListPriorityEntryRegexPattern = new[] {new[] {menuEntryRegexPattern}, new[] {menuNextStep}}
			};
		}
	}
}