using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MakiSeiBackend
{
	public interface IWebsiteGenerationProgressReporter
	{
		void ReportProgress(int progress, string pagePath);
	}
}
