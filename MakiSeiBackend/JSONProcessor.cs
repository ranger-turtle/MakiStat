using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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
			if (!File.Exists(jsonPath))
				throw new FileNotFoundException($"HTML page data \"{jsonPath}\" not found.");
			string json = File.ReadAllText(jsonPath);
			JsonSerializerOptions? jsonSerializeOptions = new() { IncludeFields = true };
			jsonSerializeOptions.Converters.Add(new DictionaryStringObjectJsonConverter());
			return JsonSerializer.Deserialize<Dictionary<string, object>>(json, jsonSerializeOptions);
		}
		public static Dictionary<string, object>? ReadLangJSONModelFromJSONFile(string jsonPath, string langCode)
		{
			string jsonNoExtFileName = Path.GetFileNameWithoutExtension(jsonPath);
			string? directory = Path.GetDirectoryName(jsonPath);
			string finalJsonPath = $"{directory}/{jsonNoExtFileName}.{langCode}.json";
			return ReadJSONModelFromJSONFile(finalJsonPath);
		}
	}
}
