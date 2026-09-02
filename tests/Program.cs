using System;
using Wildbound.Tests;

internal static class Program
{
    private static int Main()
    {
        int failed = 0;
        foreach (var test in SimulationCases.All)
        {
            try { test.Value(); Console.WriteLine("PASS " + test.Key); }
            catch (Exception e) { failed++; Console.Error.WriteLine("FAIL " + test.Key + ": " + e.Message); }
        }
        Console.WriteLine((SimulationCases.All.Count - failed) + "/" + SimulationCases.All.Count + " passed");
        return failed == 0 ? 0 : 1;
    }
}
