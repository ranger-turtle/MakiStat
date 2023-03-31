using Scriban;
using Scriban.Functions;
using Scriban.Parsing;
using Scriban.Runtime;
using Scriban.Syntax;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MakiSeiBackend.ScribanEngine
{

	/// <summary>
	/// Template loader needed to make builtin scriban "include" function work.
	/// </summary>
	public class TemplateLoader : ITemplateLoader
	{
		public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
		{
			if (templateName.EndsWith(".sbn"))
				context.CachedTemplates.Remove(templateName);
			return templateName;
		}

		//TODO find out how to get error from Eval method
		public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
		{
			// Template path was produced by the `GetPath` method above in case the Template has 
			// not been loaded yet
			try
			{
				string fileContent = File.ReadAllText(templatePath);
				if (templatePath.EndsWith(".sbn"))
					return ObjectFunctions.Eval(context, callerSpan, fileContent)?.ToString() ?? string.Empty;
				else
					return fileContent;
			}
			catch (FileNotFoundException ex)
			{
				ScribanGenerationEngine.Instance.ReportError(ex.Message);
				return string.Empty;
			}
		}

		public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
		{
			throw new NotImplementedException();
		}
	}
}