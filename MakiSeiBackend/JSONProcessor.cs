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
		/// <summary>
		/// Reads data from JSON file.
		/// </summary>
		/// <param name="jsonPath">Path to the JSON file.</param>
		/// <returns>Dictionary containing data read from JSON file.</returns>
		public static Dictionary<string, object>? ReadJSONModelFromJSONFile(string jsonPath)
		{
			if (!File.Exists(jsonPath))
				throw new FileNotFoundException($"HTML page data \"{jsonPath}\" not found.");
			string json = File.ReadAllText(jsonPath);
			JsonSerializerOptions? jsonSerializeOptions = new() { IncludeFields = true };
			jsonSerializeOptions.Converters.Add(new DictionaryStringObjectJsonConverter());
			return JsonSerializer.Deserialize<Dictionary<string, object>>(json, jsonSerializeOptions);
		}
		/// <summary>
		/// Reads data from JSON file with language-specific data.
		/// </summary>
		/// <param name="jsonPath">Path to the JSON file whose name does not contain part of extension indicating the language version.</param>
		/// <param name="langCode">Language code which should be contained in actual input JSON file as a part of compound extension.</param>
		/// <returns></returns>
		public static Dictionary<string, object>? ReadLangJSONModelFromJSONFile(string jsonPath, string langCode)
		{
			string jsonNoExtFileName = Path.GetFileNameWithoutExtension(jsonPath);
			string? directory = Path.GetDirectoryName(jsonPath);
			string finalJsonPath = $"{directory}/{jsonNoExtFileName}.{langCode}.json";
			return ReadJSONModelFromJSONFile(finalJsonPath);
		}
	}
}
