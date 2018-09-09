using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Bib3;
using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;
using Glob = Bib3.FCL.Glob;
using MessageBox = System.Windows.MessageBox;

namespace Sanderling.ABot.Exe
{
	public partial class App : Application
	{
		public App()
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

			SensorServerDispatcher.CyclicExchangeStart();

			UI.Main.SimulateMeasurement += MainWindow_SimulateMeasurement;

			TimerConstruct();
		}

		private static string AssemblyDirectoryPath =>
			Glob.ZuProcessSelbsctMainModuleDirectoryPfaadBerecne().EnsureEndsWith(@"\");

		public static long GetTimeStopwatch()
		{
			return Bib3.Glob.StopwatchZaitMiliSictInt();
		}

		private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var matchFullName =
				AppDomain.CurrentDomain.GetAssemblies()
					?.FirstOrDefault(candidate => string.Equals(candidate.GetName().FullName, args?.Name));

			if (null != matchFullName)
				return matchFullName;

			var matchName =
				AppDomain.CurrentDomain.GetAssemblies()
					?.FirstOrDefault(candidate => string.Equals(candidate.GetName().Name, args?.Name));

			return matchName;
		}

		private void TimerConstruct()
		{
			var timer = new DispatcherTimer(TimeSpan.FromSeconds(1.0 / 10), DispatcherPriority.Normal, Timer_Tick,
				Dispatcher);

			timer.Start();
		}

		private void Timer_Tick(object sender, object e)
		{
			Window?.ProcessInput();

			InterfaceExchange();

			UIPresent();

			var motionEnable = MainControl?.IsBotMotionEnabled ?? false;

			Task.Run(() => BotProgress(motionEnable));
		}

		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			try
			{
				var filePath =
					AssemblyDirectoryPath.PathToFilesysChild(
						DateTime.Now.SictwaiseKalenderString(".", 0) + " Exception");

				filePath.WriteToFileAndCreateDirectoryIfNotExisting(Encoding.UTF8.GetBytes(e.Exception.SictString()));

				var message = "exception written to file: " + filePath;

				MessageBox.Show(message, message, MessageBoxButton.OK, MessageBoxImage.Exclamation);
			}
			catch (Exception PersistException)
			{
				Bib3.FCL.GBS.Extension.MessageBoxException(PersistException);
			}

			Bib3.FCL.GBS.Extension.MessageBoxException(e.Exception);

			e.Handled = true;
		}

		private void MainWindow_SimulateMeasurement(IMemoryMeasurement measurement)
		{
			var time = GetTimeStopwatch();

			MemoryMeasurementLast = new FromProcessMeasurement<IMemoryMeasurement>(measurement, time, time);
		}
	}
}