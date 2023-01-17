//#define RELEASE

using MakiSeiBackend;
using System;
using System.Windows;

namespace MakiSei
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private SiteGenerator siteGenerator = new();

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
#if RELEASE
			try
			{
#endif
				siteGenerator.GenerateSite(PathTextBox.Text);
				_ = MessageBox.Show("Website generated!", "Info", MessageBoxButton.OK, icon: MessageBoxImage.Information);
#if RELEASE
			}
			catch (Exception ex)
			{
				_ = MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, icon: MessageBoxImage.Error);
			}
#endif
		}
	}
}
