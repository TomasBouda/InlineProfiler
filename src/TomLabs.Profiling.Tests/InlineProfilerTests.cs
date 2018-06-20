using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TomLabs.Profiling.Tests
{
	[TestClass]
	public class InlineProfilerTests
	{
		[TestMethod]
		public void TestMethod()
		{
			// Setup write output
			InlineProfiler.WriteTo(x => Debug.WriteLine(x));
			// And another one if you want, with custom formating
			InlineProfiler.WriteTo((type, label, elapsed) => Console.WriteLine($"|{type.ToString().ToUpper()}| Label:{label} - {elapsed}ms"));

			InlineProfiler.Probe("Some label");

			// Some long running code
			Thread.Sleep(100);

			// Get current timestamp
			InlineProfiler.Probe();

			// Or write an absolute time spent in this section
			using (InlineProfiler.ProbeSection("Slow section"))
			{
				// Some very long running code :)
				Thread.Sleep(1000);
			}

			InlineProfiler.Probe();
		}
	}
}
