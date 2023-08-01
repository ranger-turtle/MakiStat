using MakiSeiBackend;

namespace MakiSeiTest
{
	public class SiteGeneratorTesting
	{
		private class MockProgressReporter : IWebsiteGenerationProgressReporter
		{
			public void ReportProgress(int progress, string pagePath)
			{
			}
		}

		public const string testFileRoot = "../../..";

		[Fact]
		public void GenerateSite_GeneralTest_SuccessfulTwoSimplePages()
		{
			SiteGenerator siteGenerator = new(new MockTemplateEngine());

			string projectRoot = Path.Combine(testFileRoot, "SiteGeneratorTests", "Success");
			siteGenerator.GenerateSite(Path.Combine(projectRoot, "_skeleton.html"), new MockProgressReporter());

			List<string> expectedPages = new() { $"Expected\\page.html", $"Expected\\pl\\page.html" };
			List<string> generatedPages = new() { $"output\\page.html", $"output\\pl\\page.html" };

			for (int i = 0; i < 2; i++)
			{
				string expectedPage = File.ReadAllText(expectedPages[i]);
				string generatedPage = File.ReadAllText(generatedPages[i]);
				Assert.True(expectedPage == generatedPage, $"Page at path {generatedPages[i]} is not generated correctly.");
			}
		}
	}
}