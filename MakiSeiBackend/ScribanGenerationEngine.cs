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
		internal string CurrentPageTemplateFilePath { get; private set; }
		internal string RelativeCurrentOutputPageFilePath { get; private set; }
		internal Stack<string> TemplatePathStack { get; init; }
		internal string[] LangCodes { get; private set; }
		internal ModificationChecker ModificationChecker { get; private set; }

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
			ModificationChecker = siteGenerator.ModificationChecker;
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
		public string GeneratePage(string htmlPagePath, string skeletonHtml, Dictionary<string, object> globalData, string languageCode, string[] langCodes)
		{
			websiteGenerationError = null;

			LangCodes = langCodes;
			LangCode = languageCode;
			CurrentPageTemplateFilePath = htmlPagePath;

			string htmlPageDirectory = Path.GetDirectoryName(htmlPagePath);
			string htmlPageNameWithoutExt = Path.GetFileNameWithoutExtension(htmlPagePath);
			string potentialUniversalModelPath = $"{htmlPageDirectory}/{htmlPageNameWithoutExt}.json";
			RelativeCurrentOutputPageFilePath = Path.Combine(SiteGenerator.GenerateLanguageDirPath(languageCode) ?? string.Empty, Path.GetRelativePath(_mainPath, htmlPagePath));
			if (RelativeCurrentOutputPageFilePath.StartsWith('/'))
				RelativeCurrentOutputPageFilePath = RelativeCurrentOutputPageFilePath[1..];
			// Range used for removing leading slash
			string languageModelPathWithoutExt = Path.Combine(htmlPageDirectory, htmlPageNameWithoutExt);
			string languageModelPath = Path.Combine(htmlPageDirectory, $"{htmlPageNameWithoutExt}.{languageCode}.json");

			ScriptObject globalScriptObject = new()
			{
				{ "global", globalData },
				{ "uni_page", File.Exists(potentialUniversalModelPath) ? JsonProcessor.ReadJSONModelFromJSONFile(potentialUniversalModelPath) : null },
				{ "page", JsonProcessor.ReadLangJSONModelFromJSONFile(languageModelPathWithoutExt, languageCode) },
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
			string result = template.Render(TemplateContextInstance);
			_ = TemplateContextInstance.PopGlobal();
			Trace.WriteLine("Pop");
			
			if (File.Exists(potentialUniversalModelPath))
			{
				string universalModelContent = File.ReadAllText(potentialUniversalModelPath);
				ModificationChecker.AddResourceToModificationChecking(RelativeCurrentOutputPageFilePath, potentialUniversalModelPath, universalModelContent);
			}
			string languagePageModelContent = File.ReadAllText(languageModelPath);
			ModificationChecker.AddResourceToModificationChecking(RelativeCurrentOutputPageFilePath, languageModelPath, languagePageModelContent);

			if (ErrorOccured)
				throw websiteGenerationError;

			TemplateContextInstance.Reset();
			return result;
		}
	}
}
