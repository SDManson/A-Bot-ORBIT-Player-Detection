using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Sanderling.ABot.Exe
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		public string TitleComputed =>
			"A-Bot v" + (TryFindResource("AppVersionId") ?? "");

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			ProcessInput();
		}

		public void ProcessInput()
		{
			if (App.SetKeyBotMotionDisable?.Any(setKey => setKey?.All(key => Keyboard.IsKeyDown(key)) ?? false) ??
			    false)
				Main?.BotMotionDisable();
		}
	}
}