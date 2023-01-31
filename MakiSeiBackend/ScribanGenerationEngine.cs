using Scriban;
using Scriban.Runtime;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MakiSeiBackend
{
	public class ScribanGenerationEngine : ITemplateEngine
	{
		public static TemplateContext TemplateContextInstance { get; private set; }

		private string _mainPath;
		private string _globalPath;
		internal static string LangCode { get; private set; }

		public ScribanGenerationEngine(SiteGenerator siteGenerator)
		{
			TemplateContextInstance = new TemplateContext() { TemplateLoader = new TemplateLoader() };
			_mainPath = siteGenerator.MainPath;
			_globalPath = siteGenerator.GlobalPath;
		}

		public string GeneratePage(string skeletonHtml, string htmlPagePath, Dictionary<string, object> globalData, string languageCode)
		{
			LangCode = languageCode;

			string htmlPageDirectory = Path.GetDirectoryName(htmlPagePath);
			string htmlPageNameWithoutExt = Path.GetFileNameWithoutExtension(htmlPagePath);
			string potentialUniversalModelPath = $"{htmlPageDirectory}/{htmlPageNameWithoutExt}.json";

			ScriptObject globalScriptObject = new()
			{
				{ "global", globalData },
				{ "uni_page", File.Exists(potentialUniversalModelPath) ? JsonProcessor.ReadJSONModelFromJSONFile(potentialUniversalModelPath) : null },
				{ "page", JsonProcessor.ReadLangJSONModelFromJSONFile($"{htmlPageDirectory}/{htmlPageNameWithoutExt}.json", languageCode) },
				{ "current_page", htmlPageNameWithoutExt },
				{ "page_file", htmlPagePath },
				{ "lang_code", languageCode },
				{ "lang_dir_path", languageCode != "default" ? "/" + languageCode : null },
				{ "main_path", _mainPath },
				{ "global_path", _globalPath }
			};
			globalScriptObject.Import(typeof(MakiScriptObject));

			Template template = Template.Parse(skeletonHtml);

			TemplateContextInstance.PushGlobal(globalScriptObject);
			Trace.WriteLine($"Processed page: {htmlPagePath}");
			Trace.WriteLine("Push");
			string result = template.Render(TemplateContextInstance);
			TemplateContextInstance.PopGlobal();
			Trace.WriteLine("Pop");

			TemplateContextInstance.Reset();
			return result;
		}
	}
}
