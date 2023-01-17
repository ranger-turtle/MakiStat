using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MakiSeiBackend
{
	public class LanguageJsonNotFoundException : Exception {
		public LanguageJsonNotFoundException(string message) : base(message) { }
	}
	public class JsonProcessor
	{
#nullable enable
		public static Dictionary<string, object>? ReadJSONModelFromJSONFile(string jsonPath)
		{
			string json = File.ReadAllText(jsonPath);
			JsonSerializerOptions? jsonSerializeOptions = new JsonSerializerOptions() { IncludeFields = true };
			jsonSerializeOptions.Converters.Add(new DictionaryStringObjectJsonConverter());
			return JsonSerializer.Deserialize<Dictionary<string, object>>(json, jsonSerializeOptions);
		}
		public static Dictionary<string, object>? ReadLangJSONModelFromJSONFile(string jsonPath, string langCode)
		{
			string jsonNoExtFileName = Path.GetFileNameWithoutExtension(jsonPath);
			string? directory = Path.GetDirectoryName(jsonPath);
			string finalJsonPath = langCode == "default" ? $"{directory}/{jsonNoExtFileName}.json" : $"{directory}/{jsonNoExtFileName}.{langCode}.json";
			if (!File.Exists(finalJsonPath) && File.Exists(jsonPath))
				throw new LanguageJsonNotFoundException($"HTML page data with language code \"{langCode}\" not found.");
			return ReadJSONModelFromJSONFile(finalJsonPath);
		}
	}
}
