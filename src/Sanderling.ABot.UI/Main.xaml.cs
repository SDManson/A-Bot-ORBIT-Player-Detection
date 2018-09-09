using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Bib3.FCL;
using BotEngine.Interface;
using BotEngine.UI;
using Sanderling.Interface.MemoryStruct;
using Sanderling.UI;

namespace Sanderling.ABot.UI
{
	public partial class Main : UserControl
	{
		public Main()
		{
			InitializeComponent();
		}

		public bool IsBotMotionEnabled => ToggleButtonMotionEnable?.ButtonRecz?.IsChecked ?? false;
		public static event Action<IMemoryMeasurement> SimulateMeasurement;

		public void BotMotionDisable()
		{
			ToggleButtonMotionEnable?.LeftButtonDown();
		}

		public void BotMotionEnable()
		{
			ToggleButtonMotionEnable?.RightButtonDown();
		}

		public void ConfigFromModelToView(ExeConfig config)
		{
			Interface.LicenseView?.LicenseClientConfigViewModel?.PropagateFromClrMemberToDependencyProperty(
				config?.LicenseClient?.CompletedWithDefault());
		}

		public ExeConfig ConfigFromViewToModel()
		{
			return new ExeConfig
			{
				LicenseClient = Interface.LicenseView?.LicenseClientConfigViewModel
					?.PropagateFromDependencyPropertyToClrMember()
			};
		}

		public void Present(
			SimpleInterfaceServerDispatcher interfaceServerDispatcher,
			FromProcessMeasurement<IMemoryMeasurement> measurement,
			Bot.Bot bot)
		{
			Interface?.Present(interfaceServerDispatcher, measurement);

			InterfaceHeader?.SetStatus(Interface.InterfaceStatusEnum());

			BotStepResultTextBox.Text = bot?.StepLastResult?.RenderBotStepToUIText();
		}

		private void BotStepResultCopyToClipboardButton_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetText(BotStepResultTextBox.Text);
		}

		private void SimulateMeasurement_Drop(object sender, DragEventArgs e)
		{
			Bib3.FCL.GBS.Extension.CatchNaacMessageBoxException(() =>
			{
				var file = Glob.LaadeFrüühestInhaltDataiAusDropFileDrop(e);

				try
				{
					var memoryMeasurementJson = Encoding.UTF8.GetString(file.Value);

					var graph = Bib3.RefNezDiferenz.Extension.ListeWurzelDeserialisiireVonJson(memoryMeasurementJson)
						?.FirstOrDefault();

					if (null == graph)
						throw new ArgumentException("failed to read graph.");

					var measurement =
						(graph as FromProcessMeasurement<IMemoryMeasurement>)?.Value ??
						graph as IMemoryMeasurement;

					if (null == measurement)
						throw new ArgumentException("unexpected type:" + graph?.GetType());

					SimulateMeasurement?.Invoke(measurement);
				}
				catch (Exception exc)
				{
					throw new Exception("Loaded from file " + file.Key, exc);
				}
			});
		}
	}
}