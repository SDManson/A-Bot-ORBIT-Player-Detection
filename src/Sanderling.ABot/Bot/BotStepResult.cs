using System;

namespace Sanderling.ABot.Bot
{
	public class BotStepResult
	{
		public Exception Exception;

		public MotionRecommendation[] ListMotion;

		public IBotTask[][] OutputListTaskPath;
	}
}