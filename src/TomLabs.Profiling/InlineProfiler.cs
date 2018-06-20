using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TomLabs.Profiling
{
	/// <summary>
	/// Lightweigth class used for profiling code performace using timestamps.
	/// </summary>
	public class InlineProfiler : IDisposable
	{
		public enum ProfilerType : byte
		{
			/// <summary>
			/// Measuring inline time
			/// </summary>
			Probe,
			/// <summary>
			/// Measuring wrapped section
			/// </summary>
			Section
		}

		/// <summary>
		/// Profiler's Label
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Gets how much time was spend after instantiating <see cref="InlineProfiler"/>
		/// </summary>
		public long ElapsedMilliseconds => InstanceStopwatch.ElapsedMilliseconds;

		protected Stopwatch InstanceStopwatch { get; set; }
		protected long WatchStartTime { get; set; }
		protected long SectionTime => InstanceStopwatch.ElapsedMilliseconds - WatchStartTime;

		private static Stopwatch SWatch { get; set; }
		private static int _probeOrder = 1;
		private static int _sectionOrder = 1;

		private static List<Action<string>> Sinks { get; set; } = new List<Action<string>>();
		private static List<Action<ProfilerType, string, long>> StructuredSinks { get; set; } = new List<Action<ProfilerType, string, long>>();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="watch"></param>
		/// <param name="label"></param>
		protected InlineProfiler(Stopwatch watch, string label = null)
		{
			Label = GetLabel(label, false);
			InstanceStopwatch = watch ?? Stopwatch.StartNew();
			WatchStartTime = InstanceStopwatch.ElapsedMilliseconds;
			_sectionOrder++;
		}
		public void Dispose()
		{
			WriteLog(ProfilerType.Section, Label, SectionTime);
		}

		/// <summary>
		/// Creates <see cref="InlineProfiler"/> instance that writes message with current timestamp in milliseconds after beeing disposed.
		/// Note: Use with using statement
		/// </summary>
		/// <example>
		/// using(InlineProfiler.ProbeSection("test"))
		/// {
		///		Thread.Sleep(1000);
		/// }
		/// </example>
		/// <param name="label"></param>
		/// <returns></returns>
		public static InlineProfiler ProbeSection(string label = null)
		{
			return new InlineProfiler(SWatch, label);
		}

		/// <summary>
		/// Creates probe message with current timestamp in milliseconds
		/// </summary>
		/// <param name="label"></param>
		/// <param name="reset"></param>
		public static void Probe(string label = null, bool reset = false)
		{
			if (reset)
			{
				Reset();
			}

			SWatch = SWatch ?? Stopwatch.StartNew();
			WriteLog(ProfilerType.Probe, label, SWatch.ElapsedMilliseconds);
			_probeOrder++;
		}

		/// <summary>
		/// Resets profiler's <see cref="Stopwatch"/>
		/// </summary>
		public static void Reset()
		{
			if (SWatch != null)
			{
				SWatch.Stop();
				SWatch.Reset();
			}
		}

		/// <summary>
		/// Sets write function that will be used for logging messages
		/// </summary>
		/// <param name="writeFunction">Function receiving <see cref="string"/> message parameter</param>
		public static void WriteTo(Action<string> writeFunction)
		{
			Sinks.Add(writeFunction);
		}

		/// <summary>
		/// Sets write function that will be used for logging messages
		/// </summary>
		/// <param name="writeFunction">Function receiving <see cref="ProfilerType"/>, <see cref="Label"/> and <see cref="ElapsedMilliseconds"/> parameters</param>
		public static void WriteTo(Action<ProfilerType, string, long> writeFunction)
		{
			StructuredSinks.Add(writeFunction);
		}

		protected static void WriteLog(ProfilerType type, string label, long elapsed)
		{
			WriteLog($"{type.ToString().ToUpper()} {GetLabel(label)}: {elapsed}ms");

			WriteStructuredLog(type, GetLabel(label), elapsed);
		}

		protected static void WriteLog(string message)
		{
			foreach (var sink in Sinks)
			{
				sink?.Invoke(message);
			}
		}

		protected static void WriteStructuredLog(ProfilerType type, string label, long elapsed)
		{
			foreach (var sink in StructuredSinks)
			{
				sink?.Invoke(type, label, elapsed);
			}
		}

		private static string GetLabel(string label = null, bool probe = true)
		{
			return label ?? (probe ? _probeOrder : _sectionOrder).ToString();
		}
	}
}
