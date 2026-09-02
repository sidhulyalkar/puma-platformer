using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Wildbound.Unity;

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
            game.Begin();
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
    }
}
