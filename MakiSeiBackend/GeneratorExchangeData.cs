using System.Collections.Generic;

namespace MakiSeiBackend
{
	internal class GeneratorExchangeData
	{
		public ILogger Logger { get; private set; }
		public string MainPath { get; private set; } = "_main";
		public string GlobalPath { get; private set; } = "_global";
		public Stack<string> TemplateStack { get; private set; }
	}
}
