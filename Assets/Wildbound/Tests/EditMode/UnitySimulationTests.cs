using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Wildbound.Core;

namespace Wildbound.Tests
{
    public sealed class UnitySimulationTests
    {
        public static IEnumerable<string> Cases { get { return SimulationCases.All.Keys; } }
        [TestCaseSource(nameof(Cases))]
        public void Regression(string name) { SimulationCases.All[name](); }

        [Test]
        public void UnityJsonRoundTripKeepsPracticeAndExistingProgress()
        {
            var saved = new JourneySave { Biome = 1, FurthestBiome = 2, Practiced = 63, Discoveries = 17, Waystones = 5,
                Collected = new[] { 7, 3, 1 }, Checkpoints = new[] { 1, 0, 1 }, Completed = true };
            var flow = new JourneyFlow(JsonUtility.FromJson<JourneySave>(JsonUtility.ToJson(saved)));
            var restored = flow.Session.Save;
            Assert.That(restored.Practiced, Is.EqualTo(63));
            Assert.That(restored.Discoveries, Is.EqualTo(17));
            Assert.That(restored.Waystones, Is.EqualTo(5));
            Assert.That(restored.Collected, Is.EqualTo(saved.Collected));
            Assert.That(restored.Checkpoints, Is.EqualTo(saved.Checkpoints));
            Assert.That(restored.Completed, Is.True);
            Assert.That(flow.Screen, Is.EqualTo(JourneyScreen.Title));
            Assert.That(flow.Session.Paused, Is.True);
        }

        [Test]
        public void UnityJsonLegacyJourneyDefaultsPracticeWithoutLosingProgress()
        {
            const string json = "{\"Version\":1,\"Biome\":1,\"FurthestBiome\":2,\"Collected\":[7,3,1],\"Checkpoints\":[1,0,1],\"Discoveries\":17,\"Waystones\":5}";
            var restored = new JourneyFlow(JsonUtility.FromJson<JourneySave>(json)).Session.Save;
            Assert.That(restored.Practiced, Is.Zero);
            Assert.That(restored.Biome, Is.EqualTo(1));
            Assert.That(restored.FurthestBiome, Is.EqualTo(2));
            Assert.That(restored.Collected, Is.EqualTo(new[] { 7, 3, 1 }));
            Assert.That(restored.Checkpoints, Is.EqualTo(new[] { 1, 0, 1 }));
            Assert.That(restored.Discoveries, Is.EqualTo(17));
            Assert.That(restored.Waystones, Is.EqualTo(5));
        }
    }
}
