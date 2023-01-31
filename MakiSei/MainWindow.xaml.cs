#define RELEASE

using MakiSeiBackend;
using Microsoft.Win32;
using System;
using System.Windows;

namespace MakiSei
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		private void GenerateBtn_Click(object sender, RoutedEventArgs e)
		{
			ProgressWindow progressWindow = new(PathTextBox.Text)
			{
				Owner = this
			};
			bool? result = progressWindow.ShowDialog();
			if (result == true)
			{
				_ = MessageBox.Show("Website generated!", "Info", MessageBoxButton.OK, icon: MessageBoxImage.Information);
			}
		}

		private void SearchBtn_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog fileDialog = new();
			fileDialog.Filter = "Scriban HTML Files (*.html, *.sbn-html)|*.html;*.sbn-html";

			bool? result = fileDialog.ShowDialog();

			if (result == true)
			{
				PathTextBox.Text = fileDialog.FileName;
			}
		}
	}
}
