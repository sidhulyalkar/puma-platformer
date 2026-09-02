using System.Collections.Generic;
using NUnit.Framework;

namespace Wildbound.Tests
{
    public sealed class UnitySimulationTests
    {
        public static IEnumerable<string> Cases { get { return SimulationCases.All.Keys; } }
        [TestCaseSource(nameof(Cases))]
        public void Regression(string name) { SimulationCases.All[name](); }
    }
}
