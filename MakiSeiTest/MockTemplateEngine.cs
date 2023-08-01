using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MakiSeiBackend;
using MakiSeiBackend.ScribanEngine;

namespace MakiSeiTest
{
	internal class MockTemplateEngine : ITemplateEngine
	{
		public string GeneratePage(string htmlPagePath, string skeletonPath, Dictionary<string, object> globalData, string languageCode, string[] languageCodes)
		{
			StringBuilder pageBuilder = new();
			pageBuilder.AppendLine($"Skeleton data: {skeletonPath}");
			pageBuilder.AppendLine($"Properties:");
			foreach (var item in globalData)
				pageBuilder.AppendLine(item.Key);
			pageBuilder.Append($"Species name from skeleton: ");
			pageBuilder.AppendLine($"{globalData["SpeciesName"]}");
			pageBuilder.Append($"Lifespan from skeleton: ");
			pageBuilder.AppendLine($"{globalData["Lifespan"]}");
			pageBuilder.Append($"Length from skeleton: ");
			pageBuilder.AppendLine($"{(decimal)globalData["Length"]:0.#}");
			pageBuilder.Append($"Current language code: ");
			pageBuilder.AppendLine(languageCode);
			pageBuilder.AppendLine($"Languages:");
			foreach (string langCode in languageCodes)
				pageBuilder.AppendLine(langCode);
			string pageContent = File.ReadAllText($"{Path.Combine("../../", "SiteGeneratorTests/Success" , htmlPagePath)}");
			pageBuilder.Append(pageContent);
			return pageBuilder.ToString();
		}
	}
}
