# InlineProfiler

Lightweigth code profiling using timestamps.

Goal of this library is to provide simple solution for first and quick profiling. You can setup few Probes and immediately see where is the bottleneck.

## Install via nuget

```ps1
Install-Package TomLabs.InlineProfiler
```

## Usage

```cs
using TomLabs.Profiling;

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
```

Will result in this:

```
Trace>
--------
PROBE Some label: 0ms
PROBE 2: 104ms
SECTION Slow section: 1002ms
PROBE 3: 1106ms

Console>
--------
|PROBE| Label:Some label - 0ms
|PROBE| Label:2 - 104ms
|SECTION| Label:Slow section - 1002ms
|PROBE| Label:3 - 1106ms
```