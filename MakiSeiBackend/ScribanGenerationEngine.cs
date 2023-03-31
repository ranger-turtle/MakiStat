using Scriban;
using Scriban.Runtime;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MakiSeiBackend.ScribanEngine
{
	/// <summary>
	/// Exception meant to be raised when duplicate Singleton object was going to occur.
	/// </summary>
	public class DuplicateObjectException : System.Exception { public DuplicateObjectException() : base("Tried to make duplicate object meant to be Singleton.") { } }

	/// <summary>
	/// Generation engine working on Scriban-based templates.
	/// Scriban repository: https://github.com/scriban/scriban
	/// </summary>
	internal class ScribanGenerationEngine : ITemplateEngine
	{
		internal static ScribanGenerationEngine Instance { get; private set; }
		internal static TemplateContext TemplateContextInstance { get; private set; }

		private readonly string _mainPath;
		private readonly string _globalPath;
		internal ILogger Logger { get; }
		internal string LangCode { get; private set; }
		internal Stack<string> TemplatePathStack { get; }
		internal string[] LangCodes { get; private set; }

		private WebsiteGenerationErrorException websiteGenerationError;

		private bool ErrorOccured => websiteGenerationError != null;

		internal void ReportError(string message) => websiteGenerationError = new WebsiteGenerationErrorException(message, TemplatePathStack);

		public ScribanGenerationEngine(SiteGenerator siteGenerator)
		{
			if (Instance == null)
				Instance = this;
			else
				throw new DuplicateObjectException();

			TemplateContextInstance = new TemplateContext() { TemplateLoader = new TemplateLoader() };
			_mainPath = siteGenerator.MainPath;
			_globalPath = siteGenerator.GlobalPath;
			Logger = siteGenerator.Logger;
			TemplatePathStack = siteGenerator.TemplateStack;
		}

		//FIXME repair path stack-related error leaving some positions after book page processing
		/// <summary>
		/// It generates single page for the website.
		/// </summary>
		/// <param name="skeletonHtml">Skeleton HTNL code</param>
		/// <param name="htmlPagePath">Path to the template HTML representing output page</param>
		/// <param name="globalData">Data globally used across all templates</param>
		/// <param name="languageCode">Code of the processed language</param>
		/// <param name="langCodes">All of the codes meant to be processed</param>
		/// <returns></returns>
		public string GeneratePage(string skeletonHtml, string htmlPagePath, Dictionary<string, object> globalData, string languageCode, string[] langCodes)
		{
			LangCodes = langCodes;
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
				{ "lang_dir_path", SiteGenerator.GenerateLanguageDirPath(languageCode) },
				{ "main_path", _mainPath },
				{ "global_path", _globalPath }
			};
			globalScriptObject.Import(typeof(MakiScriptObject));

			Template template = Template.Parse(skeletonHtml);

			TemplateContextInstance.PushGlobal(globalScriptObject);
			Trace.WriteLine($"Processed page: {htmlPagePath}");
			Trace.WriteLine("Push");
			TemplatePathStack.Push(htmlPagePath);
			string result = template.Render(TemplateContextInstance);
			TemplatePathStack.Pop();
			_ = TemplateContextInstance.PopGlobal();
			Trace.WriteLine("Pop");
			if (ErrorOccured)
				throw websiteGenerationError;

			TemplateContextInstance.Reset();
			return result;
		}
	}
}
