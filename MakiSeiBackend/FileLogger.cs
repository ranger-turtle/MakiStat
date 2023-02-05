using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace MakiSeiBackend
{
	public class FileLogger : ILogger
	{
		private FileStream fileStream;
		private StreamWriter streamWriter;

		public void Open(string path = "website.log")
		{
			fileStream = new FileStream(path, FileMode.Append, FileAccess.Write);
			streamWriter = new StreamWriter(fileStream);
		}

		public string GenerateLogEntry(string messageType, Stack<string> templatePathStack, string message)
		{
			DateTime dateTime = DateTime.Now;
			Thread.CurrentThread.CurrentCulture = new CultureInfo("pl-PL");
			string dateTimeStr = dateTime.ToString();
			string lastPath = templatePathStack.Pop();
			string pathStackForMessage = lastPath;
			foreach (string path in templatePathStack)
				pathStackForMessage = $"{path}->{pathStackForMessage}";
			templatePathStack.Push(lastPath);
			return $"[{dateTimeStr}] {messageType} during processing {pathStackForMessage}: {message}";
		}


		private void WriteToFile(string messageType, Stack<string> templatePathStack, string message)
		{
			streamWriter.WriteLine(GenerateLogEntry(messageType, templatePathStack, message));
		}

		public void Info(Stack<string> templatePathStack, string message)
		{
			WriteToFile("Info", templatePathStack, message);
		}

		public void Warning(Stack<string> templatePathStack, string message)
		{
			WriteToFile("Warning", templatePathStack, message);
		}

		public void Error(Stack<string> templatePathStack, string message)
		{
			WriteToFile("Error", templatePathStack, message);
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
