namespace MakiSeiBackend
{
	public interface IWebsiteGenerationProgressReporter
	{
		void ReportProgress(int progress, string pagePath);
	}
}
