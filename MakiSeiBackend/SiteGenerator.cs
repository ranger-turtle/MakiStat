using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MakiSeiBackend
{
	public class WebsiteGenerationErrorException : Exception { public WebsiteGenerationErrorException(string message) : base(message) { } }

	//TODO Support JSON syntax errors
	//TODO support lacking JSON language files
	//TODO Add modification checking to avoid re-rendering unchanged pages
	//BONUS add choosing to render part of the website
	public class SiteGenerator
	{
		public ILogger Logger { get; private set; }

		public string MainPath { get; private set; } = "_main";
		public string GlobalPath { get; private set; } = "_global";

		public string ProcessedPagePath { get; private set; }

		//BONUS try using Dependency Injection
		private readonly ITemplateEngine templateEngine;

		private WebsiteGenerationErrorException websiteGenerationError;

		private bool ErrorOccured => websiteGenerationError != null;

		public SiteGenerator() : this(new FileLogger()) { }
		public SiteGenerator(ILogger logger)
		{
			Logger = logger;
			templateEngine = new ScribanGenerationEngine(this);
		}

		private string ExtractLangCode(string jsonFilePath)
		{
			string filename = Path.GetFileName(jsonFilePath);
			string[] fragments = filename.Split('.');
			//if there are two fragments, return lang code
			return fragments[^2];
		}

		public void ReportError(string message)
		{
			websiteGenerationError = new WebsiteGenerationErrorException(message);
		}

		public void GenerateSite(string skeletonPath, IWebsiteGenerationProgressReporter progressReporter)
		{
			if (skeletonPath == null || skeletonPath == string.Empty)
				throw new FileNotFoundException($"You did not enter the path of the skeleton.{Environment.NewLine}Please give the path to the existing skeleton of the page.");

			Environment.CurrentDirectory = Path.GetDirectoryName(skeletonPath);
			Logger.Open();
			string[] filesinMainFolder = Directory.GetFiles(MainPath, "*.*", new EnumerationOptions() { RecurseSubdirectories = true });
			string outputDirectory = "output";
			Directory.CreateDirectory(outputDirectory);

			string skeletonFileName = Path.GetFileNameWithoutExtension(skeletonPath);
			string skeletonHtml = File.ReadAllText($"{skeletonFileName}.html");

			string[] jsonLanguageFilePaths = Directory.GetFiles(Environment.CurrentDirectory, $"{skeletonFileName}*.json", SearchOption.TopDirectoryOnly);

			float maxPageNumber = filesinMainFolder.Length * jsonLanguageFilePaths.Length;
			float pageNumber = 0;
			try
			{
				foreach (string jsonFilePath in jsonLanguageFilePaths)
				{
					string langCode = ExtractLangCode(jsonFilePath);
					Dictionary<string, object> globalData = JsonProcessor.ReadJSONModelFromJSONFile(jsonFilePath);

					foreach (string path in filesinMainFolder)
					{
						try
						{
							pageNumber++;
							float progressPercent = (pageNumber / maxPageNumber) * 100;
							progressReporter.ReportProgress(Convert.ToInt32(progressPercent), path);

							string ext = Path.GetExtension(path);
							string pageFileName = Path.GetFileName(path);
							string relativeFilePath = Path.GetRelativePath(MainPath, path);
							if ((ext is ".html" or ".sbn-html") && !pageFileName.StartsWith('_')) //It is not a partial
							{
								string langFolder = langCode == "default" ? string.Empty : $"/{langCode}";
								string rootDir = $"{outputDirectory}{langFolder}";
								string destDir = $"{rootDir}/{Path.GetDirectoryName(relativeFilePath)}";
								string fileDest = $"{destDir}/{Path.GetFileNameWithoutExtension(relativeFilePath)}.html";
								ProcessedPagePath = fileDest;
								string generatedPage = templateEngine.GeneratePage(skeletonHtml, path, globalData, langCode);
								if (ErrorOccured)
									throw websiteGenerationError;
								//I had to do this complicated job, since this method cannot catch exceptions coming from
								//static MakiScriptObject methods, even when the type is the same

								_ = Directory.CreateDirectory(destDir);
								//[Above] This assigns ".html" extension even when "sbn-html" is loaded
								File.WriteAllText(fileDest, generatedPage);
							}
							else if (ext is not ".json" and not ".sbn")
							{
								//copy file to equivalent folder
								string destDir = Path.GetDirectoryName(relativeFilePath);
								Directory.CreateDirectory(destDir);
								File.Copy($"{MainPath}/{relativeFilePath}", $"{outputDirectory}/{relativeFilePath}", true);
							}
						}
						catch (FileNotFoundException ex)
						{
							Logger.Warning(path, ex.Message);
						}
					}
				}
			}
			finally
			{
				Logger.Dispose();
				websiteGenerationError = null;
			}
		}
	}
}
