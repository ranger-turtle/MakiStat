using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MakiSeiBackend
{
	//TODO Add function returning available language versions for processed page
	internal class MakiScriptObject : ScriptObject
	{
		private static ScriptObject GenerateScriptObject()
		{
			ScriptObject scriptObject = new();
			scriptObject.Import(typeof(MakiScriptObject));
			return scriptObject;
		}

		private static void CheckExtension(ref string filePath, string ext)
		{
			if (!filePath.EndsWith(ext, StringComparison.InvariantCulture))
				filePath += ext;
		}

		private static string ParseAndRenderTemplate(string pageTemplate, ScriptObject scriptObject)
		{
			TemplateContext templateContext = ScribanGenerationEngine.TemplateContextInstance;
			Template template = Template.Parse(pageTemplate);
			templateContext.PushGlobal(scriptObject);
			Trace.WriteLine("Push");
			string result;
			try
			{
				result = template.Render(templateContext);
			}
			catch (FileNotFoundException ex)
			{
				ScribanGenerationEngine.SiteGenerator.Logger.Warning(pageTemplate, ex.Message);
				result = string.Empty;
			}
			catch (ScriptRuntimeException ex)
			{
				ScribanGenerationEngine.SiteGenerator.ReportError(ex.Message);
				result = string.Empty;
			}
			_ = templateContext.PopGlobal();
			Trace.WriteLine("Pop");
			return result;
		}
		private static void LoadPartialDataToScriptObject(string templatePath, string langCode, ScriptObject scriptObject)
		{
			string partialDataPath = $"{Path.GetDirectoryName(templatePath)}/{Path.GetFileNameWithoutExtension(templatePath)}.json";
			Dictionary<string, object> partialData = JsonProcessor.ReadLangJSONModelFromJSONFile(partialDataPath, langCode);
			scriptObject["partial"] = partialData;
		}
		public static void LoadUniversalModelToScriptObject(string modelPath, ScriptObject scriptObject)
		{
			modelPath = Path.GetDirectoryName(modelPath) + '/' + Path.GetFileNameWithoutExtension(modelPath) + ".json";
			if (File.Exists(modelPath))
			{
				Dictionary<string, object> universalModel = JsonProcessor.ReadJSONModelFromJSONFile(modelPath);
				scriptObject["uni_model"] = universalModel;
			}
			else
				scriptObject["uni_model"] = null;
		}
		public static string LoadPage(string templatePath, string sectionName = "main")
		{
			CheckExtension(ref templatePath, ".html");

			string pageTemplate = File.ReadAllText(templatePath);

			ScriptObject scriptObject = GenerateScriptObject();
			scriptObject["section"] = sectionName;

			return ParseAndRenderTemplate(pageTemplate, scriptObject);
		}

		public static string LoadPartialFile(string templatePath, string modelPath)
		{
			CheckExtension(ref templatePath, ".html");
			CheckExtension(ref modelPath, ".json");

			string pageTemplate = File.ReadAllText(templatePath);

			string langCode = ScribanGenerationEngine.LangCode;
			Dictionary<string, object> model = JsonProcessor.ReadLangJSONModelFromJSONFile(modelPath, langCode);

			ScriptObject scriptObject = GenerateScriptObject();
			scriptObject["model"] = model;

			LoadPartialDataToScriptObject(templatePath, langCode, scriptObject);
			LoadUniversalModelToScriptObject(modelPath, scriptObject);
			return ParseAndRenderTemplate(pageTemplate, scriptObject);
		}

		public static string LoadPartial(string templatePath, object model, object uniModel = null)
		{
			CheckExtension(ref templatePath, ".html");

			string pageTemplate = File.ReadAllText(templatePath);

			string langCode = ScribanGenerationEngine.LangCode;

			ScriptObject scriptObject = GenerateScriptObject();
			scriptObject["model"] = model;
			scriptObject["uni_model"] = uniModel;

			try
			{
				LoadPartialDataToScriptObject(templatePath, langCode, scriptObject);
				return ParseAndRenderTemplate(pageTemplate, scriptObject);
			}
			catch (LanguageJsonNotFoundException ljnfe)
			{
				Trace.TraceWarning($"Error during processing file {templatePath}: {ljnfe}");
				return $"Could not load partial {templatePath}";
			}
		}

		public static object LoadModel(string modelPath, bool multilingual = true)
		{
			try
			{
				CheckExtension(ref modelPath, ".json");

				string langCode = ScribanGenerationEngine.LangCode;
				Dictionary<string, object> model = multilingual ? JsonProcessor.ReadLangJSONModelFromJSONFile(modelPath, langCode) : JsonProcessor.ReadJSONModelFromJSONFile(modelPath);

				return model;
			}
			catch (FileNotFoundException ex)
			{
				ScribanGenerationEngine.SiteGenerator.Logger.Warning(ScribanGenerationEngine.SiteGenerator.ProcessedPagePath, ex.Message);
				return null;
			}
		}
	}
}
