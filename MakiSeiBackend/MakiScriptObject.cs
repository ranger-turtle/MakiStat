using Scriban;
using Scriban.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace MakiSeiBackend
{
	class MakiScriptObject : ScriptObject
	{
		private static ScriptObject generateScriptObject()
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
			string result = template.Render(templateContext);
			_ = templateContext.PopGlobal();
			Trace.WriteLine("Pop");
			return result;
		}

		public static string LoadPage(string templatePath, string sectionName = "main")
		{
			CheckExtension(ref templatePath, ".html");

			string pageTemplate = File.ReadAllText(templatePath);

			ScriptObject scriptObject = generateScriptObject();
			scriptObject["section"] = sectionName;

			return ParseAndRenderTemplate(pageTemplate, scriptObject);
		}

		public static string LoadPartialFile(string templatePath, string modelPath, bool multilingual = true)
		{
			CheckExtension(ref templatePath, ".html");
			CheckExtension(ref modelPath, ".json");

			string pageTemplate = File.ReadAllText(templatePath);

			string langCode = ScribanGenerationEngine.LangCode;
			Dictionary<string, object> model = multilingual ? JsonProcessor.ReadLangJSONModelFromJSONFile(modelPath, langCode) : JsonProcessor.ReadJSONModelFromJSONFile(modelPath);

			ScriptObject scriptObject = generateScriptObject();
			scriptObject["model"] = model;

			string partialDataPath = Path.GetDirectoryName(templatePath) + '/' + Path.GetFileNameWithoutExtension(templatePath) + ".json";
			if (File.Exists(partialDataPath))
			{
				Dictionary<string, object> partialData = JsonProcessor.ReadLangJSONModelFromJSONFile(partialDataPath, langCode);
				scriptObject["partial"] = partialData;
			}

			return ParseAndRenderTemplate(pageTemplate, scriptObject);//new TemplateContext() { TemplateLoader = new TemplateLoader() });
		}

		public static string LoadPartial(string templatePath, Dictionary<string, object> model)
		{
			CheckExtension(ref templatePath, ".html");

			string pageTemplate = File.ReadAllText(templatePath);

			string langCode = ScribanGenerationEngine.LangCode;

			ScriptObject scriptObject = generateScriptObject();
			scriptObject["model"] = model;

			string partialDataPath = Path.GetDirectoryName(templatePath) + '/' + Path.GetFileNameWithoutExtension(templatePath) + ".json";
			if (File.Exists(partialDataPath))
			{
				Dictionary<string, object> partialData = JsonProcessor.ReadLangJSONModelFromJSONFile(partialDataPath, langCode);
				scriptObject["partial"] = partialData;
			}

			return ParseAndRenderTemplate(pageTemplate, scriptObject);
		}

		public static object LoadModel(string modelPath, bool multilingual = true)
		{
			CheckExtension(ref modelPath, ".json");

			string langCode = ScribanGenerationEngine.LangCode;
			Dictionary<string, object> model = multilingual ? JsonProcessor.ReadLangJSONModelFromJSONFile(modelPath, langCode) : JsonProcessor.ReadJSONModelFromJSONFile(modelPath);

			return model;
		}
	}
}
