using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MakiSeiBackend
{
	public class WebsiteGenerationErrorException : Exception
	{
		public Stack<string> TemplateStack { get; private set; }

		public WebsiteGenerationErrorException(string message, Stack<string> templateStack) : base(message)
		{
			TemplateStack = templateStack;
		}
	}

	//TODO Support JSON syntax errors
	//TODO Add modification checking to avoid re-rendering unchanged pages
	//BONUS add choosing to render part of the website
	public class SiteGenerator
	{
		public ILogger Logger { get; private set; }
		public string MainPath { get; private set; } = "_main";
		public string GlobalPath { get; private set; } = "_global";
		public Stack<string> TemplateStack { get; private set; } = new Stack<string>();

		//BONUS try using Dependency Injection
		private readonly ITemplateEngine templateEngine;

		public SiteGenerator() : this(new FileLogger()) { }
		public SiteGenerator(ILogger logger)
		{
			Logger = logger;
			templateEngine = new ScribanEngine.ScribanGenerationEngine(this);
		}

		private string ExtractLangCode(string jsonFilePath)
		{
			string filename = Path.GetFileName(jsonFilePath);
			string[] fragments = filename.Split('.');
			//if there are two fragments, return lang code
			return fragments[^2];
		}
		internal static string GenerateLanguageDirPath(string languageCode) => languageCode != "default" ? "/" + languageCode : null;

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

			string[] langCodes = jsonLanguageFilePaths.Select(lc => ExtractLangCode(lc)).ToArray();
			using (Logger)
			{
				foreach (string jsonFilePath in jsonLanguageFilePaths)
				{
					string currentLangCode = ExtractLangCode(jsonFilePath);
					Dictionary<string, object> globalData = JsonProcessor.ReadJSONModelFromJSONFile(jsonFilePath);

					foreach (string path in filesinMainFolder)
					{
						pageNumber++;
						float progressPercent = (pageNumber / maxPageNumber) * 100;
						progressReporter.ReportProgress(Convert.ToInt32(progressPercent), path);

						string ext = Path.GetExtension(path);
						string pageFileName = Path.GetFileName(path);
						string relativeFilePath = Path.GetRelativePath(MainPath, path);
						if (ext is ".html" or ".sbn-html")
						{
							if (!pageFileName.StartsWith('_')) //It is not a partial
							{
								try
								{
									string[] availableLangCodes = langCodes.Where(lc =>
										File.Exists($"{MainPath}/{Path.GetDirectoryName(relativeFilePath)}/{Path.GetFileNameWithoutExtension(pageFileName)}.{lc}.json"))
										.ToArray();

									string langFolder = currentLangCode == "default" ? string.Empty : $"/{currentLangCode}";
									string rootDir = $"{outputDirectory}{langFolder}";
									string destDir = $"{rootDir}/{Path.GetDirectoryName(relativeFilePath)}";
									string fileDest = $"{destDir}/{Path.GetFileNameWithoutExtension(relativeFilePath)}.html";
									TemplateStack.Push(fileDest);
									string generatedPage = templateEngine.GeneratePage(skeletonHtml, path, globalData, currentLangCode, availableLangCodes);
									//I had to do this complicated job, since this method cannot catch exceptions coming from
									//static MakiScriptObject methods, even when the type is the same

									_ = Directory.CreateDirectory(destDir);
									//[Above] This assigns ".html" extension even when "sbn-html" is loaded
									File.WriteAllText(fileDest, generatedPage);
								}
								catch (FileNotFoundException ex)
								{
									Logger.Warning(TemplateStack, ex.Message);
								}
								finally
								{
									TemplateStack.Pop();
								}
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
}
