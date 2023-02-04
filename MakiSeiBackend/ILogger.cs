using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MakiSeiBackend
{
	public interface ILogger
	{
		void Open(string path = "website.log");
		void Info(string pageFilePath, string message);
		void Warning(string pageFilePath, string message);
		void Error(string pageFilePath, string message);
		void Dispose();
	}
}
