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
			return fragments[^2];
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
							string generatedPage = templateEngine.GeneratePage(skeletonHtml, path, globalData, langCode);

							string langFolder = langCode == "default" ? string.Empty : $"/{langCode}";
							string rootDir = $"{outputDirectory}{langFolder}";
							string destDir = $"{rootDir}/{Path.GetDirectoryName(relativeFilePath)}";
							_ = Directory.CreateDirectory(destDir);
							string fileDest = $"{rootDir}/{relativeFilePath}";
							File.WriteAllText(fileDest, generatedPage);
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
