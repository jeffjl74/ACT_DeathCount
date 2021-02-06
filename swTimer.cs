using System;
using System.Diagnostics;

// Stopwatch based class to consolodate high/low/average performance time measurements
class SwTimer : Stopwatch
{
    long lowTicks = Int64.MaxValue;
    long highTicks = 0;
    double avgTicks = 0;
    long measurements = 0;

    public SwTimer()
    {
    }

    public void Clear()
    {
        lowTicks = Int64.MaxValue;
        highTicks = 0;
        avgTicks = 0;
        measurements = 0;
    }

    new public void Stop()
    {
        base.Stop();

        long elapsed = ElapsedTicks;
        if (elapsed > highTicks)
            highTicks = elapsed;
        if (elapsed < lowTicks)
            lowTicks = elapsed;
        if (measurements == 0)
            avgTicks = elapsed;
        else
            avgTicks = ((avgTicks * measurements) + elapsed) / (measurements + 1);
        measurements++;
    }
}