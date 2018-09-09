using System.Collections.Generic;
using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.ABot.Bot
{
	public class BotStepInput
	{
		public StringAtPath ConfigSerial;

		public FromProcessMeasurement<IMemoryMeasurement> FromProcessMemoryMeasurement;

		public IEnumerable<IBotTask> RootTaskListComponentOverride;

		public MotionResult[] StepLastMotionResult;
		public long TimeMilli;
	}
}