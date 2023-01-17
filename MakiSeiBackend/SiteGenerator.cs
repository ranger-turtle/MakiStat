using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace MakiSeiBackend
{
	using JSONObject = Dictionary<string, object>;
	public class SiteGenerator
	{
		public string MainPath { get; private set; } = "_main";
		public string GlobalPath { get; private set; } = "_global";

		//BONUS try using Dependency Injection
		private readonly ITemplateEngine templateEngine;

		public SiteGenerator()
		{
			templateEngine = new ScribanGenerationEngine(this);
		}

		private string ExtractLangCode(string jsonFilePath)
		{
			string filename = Path.GetFileName(jsonFilePath);
			string[] fragments = filename.Split('.');
			//if there are two fragments, return lang code
			return fragments.Length > 2 ? fragments[1] : "default";
		}

		public void GenerateSite(string skeletonPath)
		{
			Environment.CurrentDirectory = Path.GetDirectoryName(skeletonPath);
			string[] filesinMainFolder = Directory.GetFiles(MainPath, "*.*", new EnumerationOptions() { RecurseSubdirectories = true});
			string outputDirectory = "output";
			Directory.CreateDirectory(outputDirectory);

			string skeletonHtml = File.ReadAllText("_skeleton.html");

			string[] jsonLanguageFilePaths = Directory.GetFiles(Environment.CurrentDirectory, "*.json", SearchOption.TopDirectoryOnly);

			foreach (string jsonFilePath in jsonLanguageFilePaths)
			{
				string langCode = ExtractLangCode(jsonFilePath);
				JSONObject globalData = JsonProcessor.ReadJSONModelFromJSONFile(jsonFilePath);

				foreach (string path in filesinMainFolder)
				{
					string ext = Path.GetExtension(path);
					string relativeFilePath = Path.GetRelativePath(MainPath, path);
					if (ext == ".html")
					{
						if (ext[0] != '_') //It is not a partial
						{
							try
							{
								string generatedPage = templateEngine.GeneratePage(skeletonHtml, path, globalData, langCode);

								string langFolder = langCode == "default" ? string.Empty : $"/{langCode}";
								string destDir = $"{outputDirectory}{langFolder}/{Path.GetDirectoryName(relativeFilePath)}";
								_ = Directory.CreateDirectory(destDir);
								string fileDest = $"{outputDirectory}{langFolder}/{relativeFilePath}";
								File.WriteAllText(fileDest, generatedPage);
							}
							catch (LanguageJsonNotFoundException ljnfe)
							{
								Trace.TraceWarning($"Error during processing file {path}: {ljnfe}");
							}
						}
					}
					else if (ext != ".json")
					{
						//copy file to equivalent folder
						string destDir = Path.GetDirectoryName(relativeFilePath);
						Directory.CreateDirectory(destDir);
						File.Copy($"{MainPath}/relativeFilePath", $"{outputDirectory}/{relativeFilePath}");
					}
				}
			}
		}
	}
}
