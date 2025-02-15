using System.Collections.Generic;

namespace MakiStatBackend;

	public interface ITemplateEngine
	{
		/// <summary>
		/// Generates 
		/// </summary>
		/// <param name="htmlPagePath">Path to HTML file representing page</param>
		/// <param name="skeletonHtml">Skeleton HTML</param>
		/// <returns>Generated page</returns>
		string GeneratePage(string htmlPagePath, string skeletonHtml, Dictionary<string, object> globalData, string languageCode, string[] languageCodes);
	}
