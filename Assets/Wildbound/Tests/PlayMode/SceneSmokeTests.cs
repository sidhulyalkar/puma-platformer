using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Wildbound.Unity;
using Wildbound.Core;

namespace Wildbound.Tests
{
    public sealed class SceneSmokeTests
    {
        [UnityTest]
        public IEnumerator RuntimeCreatesOnePumaAndPausesCleanly()
        {
            yield return null;
            var game = Object.FindFirstObjectByType<WildboundGame>();
            // The normal player boot hook may not run in every test-runner scene setup.
            if (game == null) game = new GameObject("Wildbound smoke test").AddComponent<WildboundGame>();
            yield return null;
            Assert.That(Camera.main, Is.Not.Null);
            Assert.That(Resources.Load<Shader>("WildboundFlat"), Is.Not.Null);
            int pumas = 0;
            foreach (var item in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)) if (item.name == "Puma") pumas++;
            Assert.That(pumas, Is.EqualTo(1));
            Assert.That(Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Length, Is.GreaterThan(100));
            if (game.Playing) game.Resume(); else game.Begin();
            yield return new WaitForFixedUpdate();
            Assert.That(game.Playing, Is.True);
            game.TogglePause();
            float time = game.Session.Time;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            Assert.That(game.Session.Time, Is.EqualTo(time));
            Assert.That(game.Session.Player.Charging, Is.False);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator HostMenusPauseAndCancelResetWithoutReplacingTheJourney()
        {
            yield return null;
            var game = Object.FindFirstObjectByType<WildboundGame>();
            if (game == null) game = new GameObject("Wildbound menu smoke test").AddComponent<WildboundGame>();
            if (game.Playing) game.Resume(); else game.Begin();
            var session = game.Session;
            game.TogglePause();
            game.ToggleControls();
            Assert.That(game.ShowControls, Is.True);
            game.ToggleMap();
            Assert.That(game.ShowControls, Is.False);
            Assert.That(game.ShowMap, Is.True);
            float time = session.Time;
            yield return new WaitForFixedUpdate();
            Assert.That(session.Time, Is.EqualTo(time));
            game.TogglePause();
            Assert.That(game.Flow.Screen, Is.EqualTo(JourneyScreen.Pause));
            game.NewJourney();
            Assert.That(game.ShowResetConfirmation, Is.True);
            game.TogglePause();
            game.Resume();
            game.TogglePause();
            game.NewJourney();
            Assert.That(game.ShowResetConfirmation, Is.True);
            Assert.That(game.Session, Is.SameAs(session));
            game.TogglePause();
            Assert.That(game.Flow.Screen, Is.EqualTo(JourneyScreen.Pause));
            Assert.That(session.Paused, Is.True);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
