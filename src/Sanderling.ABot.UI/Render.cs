using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;
using Sanderling.ABot.Bot;
using Sanderling.ABot.Bot.Task;

namespace Sanderling.ABot.UI
{
	public static class Render
	{
		private static IEnumerable<string> RenderBotStepToUITextComponentException(this Exception exception)
		{
			return null == exception
				? null
				: new[]
				{
					"---- Exception ----",
					exception.ToString()
				};
		}

		public static string RenderBotLeafTaskTypeToString(IBotTask leafTask)
		{
			return new[]
			{
				leafTask.ContainsEffect() ? "Effect" : null,
				leafTask is DiagnosticTask ? "Diagnostic: \"" + (leafTask as DiagnosticTask)?.MessageText + "\"" : null
			}.WhereNotDefault().FirstOrDefault();
		}

		public static string RenderTaskPathToUIText(IBotTask[] taskPath)
		{
			return taskPath.IsNullOrEmpty()
				? null
				: RenderBotLeafTaskTypeToString(taskPath?.LastOrDefault()) +
				  "(" + string.Join("->", taskPath.Select(taskPathNode => taskPathNode?.GetType()?.Name)) + ")";
		}

		public static string RenderBotStepToUIText(this BotStepResult stepResult)
		{
			return string.Join(Environment.NewLine, new[]
			{
				stepResult?.Exception?.RenderBotStepToUITextComponentException(),
				new[] {""},
				stepResult?.OutputListTaskPath?.Select(RenderTaskPathToUIText)
			}.ConcatNullable());
		}

		public static string TimeAgeMilliToUIText(this long? ageMilli)
		{
			return !ageMilli.HasValue
				? null
				: ageMilli / 1000 + " s ago at " +
				  (DateTime.Now - TimeSpan.FromMilliseconds(ageMilli.Value)).ToLongTimeString();
		}

		public static string RenderBotStepToUIText(this PropertyGenTimespanInt64<BotStepResult> stepResultAtTimeMilli)
		{
			return null == stepResultAtTimeMilli
				? null
				: stepResultAtTimeMilli?.Value?.OutputListTaskPath?.Count() + " leaves " +
				  TimeAgeMilliToUIText(Bot.Bot.GetTimeMilli() - stepResultAtTimeMilli?.Begin) +
				  Environment.NewLine +
				  RenderBotStepToUIText(stepResultAtTimeMilli.Value);
		}
	}
}