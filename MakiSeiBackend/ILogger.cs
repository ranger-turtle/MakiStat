using System;
using System.Collections.Generic;

namespace MakiSeiBackend
{
	public interface ILogger : IDisposable
	{
		void Open(string path = "website.log");
		void Info(Stack<string> templatePathStack, string message);
		void Warning(Stack<string> templatePathStack, string message);
		void Error(Stack<string> templatePathStack, string message);
	}
}
