namespace MakiStatBackend;

	/// <summary>
	/// Interface for the components communicating with UI.
	/// </summary>
	public interface IWebsiteGenerationProgressReporter
	{
		/// <summary>
		/// Reports progress to UI.
		/// </summary>
		/// <param name="progress">Progress level measured in percents.</param>
		/// <param name="pagePath">Page path to be displayed in UI.</param>
		void ReportProgress(int progress, string pagePath);
	}
