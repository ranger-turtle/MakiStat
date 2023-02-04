using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MakiSeiBackend
{
	public class FileLogger : ILogger, IDisposable
	{
		private FileStream fileStream;
		private StreamWriter streamWriter;

		public void Open(string path = "website.log")
		{
			fileStream = new FileStream(path, FileMode.Append, FileAccess.Write);
			streamWriter = new StreamWriter(fileStream);
		}

		public string GenerateLogEntry(string messageType, string pageFilePath, string message)
		{
			DateTime dateTime = DateTime.Now;
			Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");
			string dateTimeStr = dateTime.ToString();
			return $"[{dateTimeStr}] {messageType} during processing {pageFilePath}: {message}";
		}


		private void WriteToFile(string messageType, string pageFilePath, string message)
		{
			streamWriter.WriteLine(GenerateLogEntry(messageType, pageFilePath, message));
		}

		public void Info(string pageFilePath, string message)
		{
			WriteToFile("Info", pageFilePath, message);
		}

		public void Warning(string pageFilePath, string message)
		{
			WriteToFile("Warning", pageFilePath, message);
		}

		public void Error(string pageFilePath, string message)
		{
			WriteToFile("Error", pageFilePath, message);
		}

		protected virtual void Dispose(bool disposing)
		{
			streamWriter.Close();
			fileStream.Close();
		}

		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
