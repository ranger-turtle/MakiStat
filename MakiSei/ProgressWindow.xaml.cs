using MakiSeiBackend;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MakiSei
{
	/// <summary>
	/// Interaction logic for ProgressWindow.xaml
	/// </summary>
	public partial class ProgressWindow : Window, IWebsiteGenerationProgressReporter
	{
		public SiteGenerator SiteGenerator { get; private set; }

		private readonly BackgroundWorker backgroundWorker;
		private readonly string skeletonPath;
		private string currentPath;

		public ProgressWindow(SiteGenerator siteGenerator, string skeletonPath)
		{
			InitializeComponent();
			SiteGenerator = siteGenerator;
			WindowStyle = WindowStyle.None;
			this.skeletonPath = skeletonPath;
			backgroundWorker = new();
			backgroundWorker.WorkerReportsProgress = true;
			backgroundWorker.DoWork += Worker_DoWork;
			backgroundWorker.ProgressChanged += Worker_ProgressChanged;
			backgroundWorker.RunWorkerCompleted += Worker_Exit;

			backgroundWorker.RunWorkerAsync();
		}
		private void Worker_DoWork(object sender, DoWorkEventArgs e)
		{
			SiteGenerator.GenerateSite(skeletonPath, this);
		}

		public void ReportProgress(int progress, string pagePath)
		{
			backgroundWorker.ReportProgress(progress);
			currentPath = pagePath;
		}

		private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			genStatus.Value = e.ProgressPercentage;
			pagePath.Content = $"Generated page: {currentPath}";
		}

		private void Worker_Exit(object sender, RunWorkerCompletedEventArgs e)
		{
			if (e.Error == null)
			{
				DialogResult = true;
			}
			else
			{
				string message = $@"Error during rendering page: {SiteGenerator.ProcessedPagePath}

{e.Error?.Message}
{e.Error?.InnerException?.Message}";
				_ = MessageBox.Show(message, "Error", MessageBoxButton.OK, icon: MessageBoxImage.Error);
				DialogResult = false;
			}
		}
	}
}
