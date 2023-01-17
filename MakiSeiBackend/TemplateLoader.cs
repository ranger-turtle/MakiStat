using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MakiSeiBackend
{

	/// <summary>
	/// </summary>
	public class TemplateLoader : ITemplateLoader
	{
		public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
		{
			return templateName;
		}

		public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
		{
			// Template path was produced by the `GetPath` method above in case the Template has 
			// not been loaded yet
			return File.Exists(templatePath) ? File.ReadAllText(templatePath) : string.Empty;
		}

		public ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
		{
			throw new NotImplementedException();
		}
	}
}