using System.Collections.Generic;

namespace MakiSeiBackend
{
	internal interface ITemplateEngine
	{
		/// <summary>
		/// Generates 
		/// </summary>
		/// <param name="skeletonHtml">Skeleton HTML</param>
		/// <param name="htmlPagePath">Path to HTML file representing page</param>
		/// <returns>Generated page</returns>
		string GeneratePage(string skeletonHtml, string htmlPagePath, Dictionary<string, object> globalData, string languageCode, string[] languageCodes);
	}
}
