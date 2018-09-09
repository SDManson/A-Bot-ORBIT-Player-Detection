using System.Collections.Generic;
using Sanderling.Motor;

namespace Sanderling.ABot.Bot
{
	public interface IBotTask
	{
		IEnumerable<IBotTask> Component { get; }

		/// <summary>
		///     Effects to apply to the eve online client.
		/// </summary>
		IEnumerable<MotionParam> Effects { get; }
	}
}