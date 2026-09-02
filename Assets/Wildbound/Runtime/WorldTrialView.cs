using System.Collections.Generic;
using UnityEngine;
using Wildbound.Core;

namespace Wildbound.Unity
{
    public sealed partial class WorldView
    {
        private Transform sanctuaryArt, balanceArt, bellArt, windRibbon;
        private SpriteRenderer sanctuaryGlow, entranceRune, bellHalo;
        private readonly List<SpriteRenderer> balanceLights = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> sanctuaryLights = new List<SpriteRenderer>();
        private void BuildTrialArt()
        {
            balanceArt = bellArt = sanctuaryArt = windRibbon = null;
            balanceLights.Clear(); sanctuaryLights.Clear();
            var session = game.Session;
            var entrance = new GameObject("Moontrail entrance").transform; entrance.SetParent(scenery); entrance.position = Point(Moontrial.Entrance);
            Shape("crescent stone", disc, new Vector2(0, .52f), new Vector2(.85f, 1), rock, 10, entrance);
            entranceRune = Shape("crescent rune", ring, new Vector2(0, .63f), Vector2.one * .4f, light, 12, entrance);
            Shape("crescent shadow", disc, new Vector2(.1f, .68f), Vector2.one * .32f, rock, 13, entrance);
            Shape("entrance halo", disc, new Vector2(0, .8f), Vector2.one * 2, new Color(.55f, .8f, 1, .09f), 4, entrance);
            if (!session.InTrial) return;
            var trial = session.World.Trial;
            sanctuaryArt = new GameObject("Waystone sanctuary").transform; sanctuaryArt.SetParent(scenery); sanctuaryArt.position = Point(trial.Sanctuary);
            Shape("waystone slab", disc, new Vector2(0, .9f), new Vector2(1.2f, 1.8f), rock, 11, sanctuaryArt);
            Shape("waystone crown", ring, new Vector2(0, 1.65f), Vector2.one * .7f, light, 13, sanctuaryArt);
            sanctuaryGlow = Shape("waystone light", disc, new Vector2(0, 1), Vector2.one * 3, new Color(.5f, .8f, 1, .06f), 4, sanctuaryArt);
            for (int i = 0; i < trial.GoalCount; i++)
                sanctuaryLights.Add(Shape("waystone seal", square, new Vector2((i - (trial.GoalCount - 1) / 2f) * .28f, .75f), Vector2.one * .13f, distant, 14, sanctuaryArt));
            if (trial.Balance != null)
            {
                balanceArt = new GameObject("Balance center").transform; balanceArt.SetParent(scenery);
                Shape("safe center", ring, Vector2.zero, new Vector2(BalancePerch.CenterRadius * 2, .2f), light, 15, balanceArt);
                Shape("perch lantern", disc, new Vector2(0, -.75f), new Vector2(.24f, .4f), light, 14, balanceArt);
                Shape("perch cord", square, new Vector2(0, -.45f), new Vector2(.035f, .5f), moss, 13, balanceArt);
                for (int i = 0; i < 8; i++)
                    balanceLights.Add(Shape("balance charge", disc, new Vector2((i - 3.5f) * .14f, .28f), Vector2.one * .09f, distant, 16, balanceArt));
                windRibbon = new GameObject("Wind ribbon").transform; windRibbon.SetParent(balanceArt, false); windRibbon.localPosition = new Vector3(0, 1.9f, 0);
                Shape("wind line", square, Vector2.zero, new Vector2(1.1f, .035f), light, 15, windRibbon);
                Shape("wind arrow", triangle, new Vector2(.65f, 0), new Vector2(.2f, .2f), light, 15, windRibbon).transform.localRotation = Quaternion.Euler(0, 0, -90);
            }
            if (trial.Bell != null)
            {
                bellArt = new GameObject("Rebound moonbell").transform; bellArt.SetParent(scenery); bellArt.position = Point(trial.Bell.Position);
                bellHalo = Shape("bell ripple", ring, Vector2.zero, Vector2.one * 1.5f, new Color(.8f, .65f, 1, .2f), 12, bellArt);
                Shape("moonbell rim", disc, Vector2.zero, new Vector2(1, .25f), Hex("e4c9ff"), 14, bellArt);
                Shape("moonbell body", triangle, new Vector2(0, -.15f), new Vector2(.6f, .5f), Hex("b89add"), 13, bellArt);
                Shape("downward mark", triangle, new Vector2(0, .55f), Vector2.one * .18f, light, 15, bellArt).transform.localRotation = Quaternion.Euler(0, 0, 180);
            }
        }
        private void UpdateTrialArt()
        {
            var session = game.Session;
            entranceRune.color = session.WaystoneRestored(session.Save.Biome) ? Hex("c0ffcc") : light;
            if (!session.InTrial) return;
            var trial = session.World.Trial;
            int done = trial.FinishedGoals(session.World);
            for (int i = 0; i < sanctuaryLights.Count; i++) sanctuaryLights[i].color = i < done ? light : distant;
            sanctuaryGlow.color = new Color(.6f, .9f, 1, trial.Ready(session.World) ? .23f : .06f);
            if (balanceArt != null)
            {
                var state = trial.Balance; var platform = session.World.Platforms[state.PlatformIndex];
                balanceArt.position = new Vector3(platform.Bounds.Center.X, platform.Bounds.Top + .03f, 0);
                float charge = state.Attuned ? 1 : state.Charge / BalancePerch.SettleSeconds;
                for (int i = 0; i < balanceLights.Count; i++) balanceLights[i].color = i < charge * 8 ? light : distant;
                windRibbon.gameObject.SetActive(!state.Attuned);
                windRibbon.localScale = new Vector3(state.Wind(session.Time), 1, 1);
            }
            if (bellArt != null)
            {
                float ripple = game.ReducedMotion ? 0 : trial.Bell.Cooldown / .4f;
                bellHalo.transform.localScale = Vector3.one * (1.5f + ripple * 2);
                bellHalo.color = new Color(.8f, .65f, 1, trial.Bell.Rung ? .4f : .15f);
            }
        }
    }
}
