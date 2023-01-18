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

			ScriptObject globalScriptObject = new()
			{
				{ "global", globalData },
				{ "uni_page", JsonProcessor.ReadJSONModelFromJSONFile($"{htmlPageDirectory}/{htmlPageNameWithoutExt}.json") },
				{ "page", JsonProcessor.ReadLangJSONModelFromJSONFile($"{htmlPageDirectory}/{htmlPageNameWithoutExt}.json", languageCode) },
				{ "page_file", htmlPagePath },
				{ "lang_code", languageCode },
				{ "main_path", _mainPath },
				{ "global_path", _globalPath }
			};
			globalScriptObject.Import(typeof(MakiScriptObject));

			Template template = Template.Parse(skeletonHtml);

			TemplateContextInstance.PushGlobal(globalScriptObject);
			Trace.WriteLine("Push");
			string result = template.Render(TemplateContextInstance);
			TemplateContextInstance.PopGlobal();
			Trace.WriteLine("Pop");

			return result;
		}
	}
}
