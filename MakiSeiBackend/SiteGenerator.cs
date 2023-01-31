using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace MakiSeiBackend
{
	using JSONObject = Dictionary<string, object>;
	//TODO Support JSON syntax errors
	//TODO support lacking JSON language files
	//TODO Add modification checking to avoid re-rendering unchanged pages
	public class SiteGenerator
	{
		private readonly IWebsiteGenerationProgressReporter progressReporter;

		public string MainPath { get; private set; } = "_main";
		public string GlobalPath { get; private set; } = "_global";

		public string ProcessedPagePath { get; private set; }

		//BONUS try using Dependency Injection
		private readonly ITemplateEngine templateEngine;

		public SiteGenerator(IWebsiteGenerationProgressReporter progressReporter)
		{
			this.progressReporter = progressReporter;
			templateEngine = new ScribanGenerationEngine(this);
		}

		private string ExtractLangCode(string jsonFilePath)
		{
			string filename = Path.GetFileName(jsonFilePath);
			string[] fragments = filename.Split('.');
			//if there are two fragments, return lang code
			return fragments[^2];
		}

		private void ReportProgress(float progressPercent, string processedPagePath)
		{
			progressReporter.ReportProgress(Convert.ToInt32(progressPercent), processedPagePath);
		}

		public void GenerateSite(string skeletonPath)
		{
			if (skeletonPath == null || skeletonPath == string.Empty)
				throw new FileNotFoundException($"You did not enter the path of the skeleton.{Environment.NewLine}Please give the path to the existing skeleton of the page.");

			Environment.CurrentDirectory = Path.GetDirectoryName(skeletonPath);
			string[] filesinMainFolder = Directory.GetFiles(MainPath, "*.*", new EnumerationOptions() { RecurseSubdirectories = true});
			string outputDirectory = "output";
			Directory.CreateDirectory(outputDirectory);

			string skeletonFileName = Path.GetFileNameWithoutExtension(skeletonPath);
			string skeletonHtml = File.ReadAllText($"{skeletonFileName}.html");

			string[] jsonLanguageFilePaths = Directory.GetFiles(Environment.CurrentDirectory, $"{skeletonFileName}*.json", SearchOption.TopDirectoryOnly);

			foreach (string jsonFilePath in jsonLanguageFilePaths)
			{
				string langCode = ExtractLangCode(jsonFilePath);
				JSONObject globalData = JsonProcessor.ReadJSONModelFromJSONFile(jsonFilePath);

				float maxPageNumber = filesinMainFolder.Length;
				float pageNumber = 0;
				foreach (string path in filesinMainFolder)
				{
					ProcessedPagePath = path;
					pageNumber++;
					ReportProgress((pageNumber / maxPageNumber) * 100, path);

					string ext = Path.GetExtension(path);
					string pageFileName = Path.GetFileName(path);
					string relativeFilePath = Path.GetRelativePath(MainPath, path);
					if (ext is ".html" or ".sbn-html")
					{
						if (!pageFileName.StartsWith('_')) //It is not a partial
						{
							string generatedPage = templateEngine.GeneratePage(skeletonHtml, path, globalData, langCode);

							string langFolder = langCode == "default" ? string.Empty : $"/{langCode}";
							string rootDir = $"{outputDirectory}{langFolder}";
							string destDir = $"{rootDir}/{Path.GetDirectoryName(relativeFilePath)}";
							_ = Directory.CreateDirectory(destDir);
							string fileDest = $"{destDir}/{Path.GetFileNameWithoutExtension(relativeFilePath)}.html";
							//[Above] This assigns ".html" extension even when "sbn-html" is loaded
							File.WriteAllText(fileDest, generatedPage);
						}
					}
					else if (ext is not ".json" and not ".sbn")
					{
						//copy file to equivalent folder
						string destDir = Path.GetDirectoryName(relativeFilePath);
						Directory.CreateDirectory(destDir);
						File.Copy($"{MainPath}/{relativeFilePath}", $"{outputDirectory}/{relativeFilePath}", true);
					}
				}
			}
		}
	}
}
