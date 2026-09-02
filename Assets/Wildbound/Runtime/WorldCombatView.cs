using System.Collections.Generic;
using UnityEngine;
using Wildbound.Core;

namespace Wildbound.Unity
{
    public sealed partial class WorldView
    {
        private sealed class EnemyArt
        {
            public Transform Root, LeftWing, RightWing, Warning, AimLine;
            public SpriteRenderer Glow, Scent;
            public SpriteRenderer[] Parts;
            public Color[] Colors;
            public readonly List<SpriteRenderer> Hearts = new List<SpriteRenderer>();
        }
        private sealed class BloomArt { public Transform Root; public SpriteRenderer Halo, Core; }
        private sealed class BridgeArt { public int Index; public SpriteRenderer[] Parts; public Color[] Colors; }
        private readonly List<EnemyArt> wildlife = new List<EnemyArt>();
        private readonly List<BloomArt> blooms = new List<BloomArt>();
        private readonly List<BridgeArt> bridges = new List<BridgeArt>();
        private readonly List<SpriteRenderer> bolts = new List<SpriteRenderer>();
        private readonly List<SpriteRenderer> clawLines = new List<SpriteRenderer>();
        private Sprite ring;
        private SpriteRenderer pawLight;

        private void BuildCombatArt()
        {
            wildlife.Clear(); blooms.Clear(); bridges.Clear(); bolts.Clear(); clawLines.Clear();
            if (ring == null) ring = MakeSprite(3);
            pawLight = Shape("Puma moonlight", disc, Vector2.zero, Vector2.one * 3.2f, new Color(1, .75f, .4f, .06f), 3);
            foreach (var enemy in game.Session.World.Enemies)
            {
                var art = new EnemyArt();
                art.Root = new GameObject(enemy.Name).transform; art.Root.SetParent(scenery);
                var root = art.Root;
                Color fur = Hex("d5bd85"), dark = Hex("1c2937"), cyan = Hex("b9f8e0");
                if (enemy.Kind == EnemyKind.ClawPost)
                {
                    Shape("wood post", square, new Vector2(0, .7f), new Vector2(.4f, 1.4f), Hex("997958"), 16, root);
                    Shape("post cap", disc, new Vector2(0, 1.45f), new Vector2(.5f, .18f), fur, 17, root);
                    for (int i = 0; i < 3; i++) Shape("old claw marks", square, new Vector2((i - 1) * .09f, .9f), new Vector2(.025f, .35f), light, 18, root).transform.localRotation = Quaternion.Euler(0, 0, -25);
                }
                else if (enemy.Kind == EnemyKind.MossHare)
                {
                    Shape("hare body", disc, new Vector2(-.04f, .25f), new Vector2(.68f, .46f), fur, 16, root);
                    Shape("hare haunch", disc, new Vector2(-.22f, .2f), new Vector2(.4f, .4f), Hex("a6ac85"), 17, root);
                    Shape("hare head", disc, new Vector2(.22f, .44f), Vector2.one * .32f, fur, 18, root);
                    for (int i = 0; i < 2; i++) Shape("long ear", disc, new Vector2(.12f + i * .16f, .72f), new Vector2(.12f, .45f), fur, 17, root);
                    Shape("hare eye", disc, new Vector2(.31f, .48f), Vector2.one * .06f, dark, 19, root);
                    Shape("hare tail", disc, new Vector2(-.4f, .34f), Vector2.one * .17f, light, 18, root);
                }
                else if (enemy.Kind == EnemyKind.Thornling)
                {
                    Shape("thornling body", disc, new Vector2(0, .32f), new Vector2(.88f, .61f), Hex("af777b"), 16, root);
                    for (int i = -2; i <= 2; i++) Shape("leaf spine", triangle, new Vector2(i * .17f, .63f), new Vector2(.25f, .28f), Hex("edb99e"), 17, root);
                    Shape("thornling eye", disc, new Vector2(.23f, .4f), Vector2.one * .08f, light, 18, root);
                }
                else if (enemy.Kind == EnemyKind.Bristleback)
                {
                    Shape("boar haunch", disc, new Vector2(0, .4f), new Vector2(1.22f, .77f), Hex("666e91"), 16, root);
                    for (int i = -2; i <= 2; i++) Shape("armor plate", triangle, new Vector2(i * .22f, .72f), new Vector2(.4f, .35f), Hex("9aabd0"), 17, root);
                    Shape("armored snout", disc, new Vector2(.5f, .37f), new Vector2(.38f, .44f), Hex("acb6c9"), 18, root);
                    Shape("forward tusk", triangle, new Vector2(.73f, .24f), new Vector2(.16f, .34f), light, 19, root).transform.localRotation = Quaternion.Euler(0, 0, -70);
                    Shape("boar eye", disc, new Vector2(.37f, .54f), Vector2.one * .085f, Hex("ffe0a3"), 19, root);
                }
                else if (enemy.Kind == EnemyKind.ReedSpitter)
                {
                    Shape("reed stem", square, new Vector2(0, .5f), new Vector2(.15f, 1), Hex("5d8b88"), 16, root);
                    Shape("spitter bulb", disc, new Vector2(.1f, .8f), new Vector2(.66f, .55f), Hex("89aaa3"), 17, root);
                    Shape("reed muzzle", disc, new Vector2(.42f, .77f), new Vector2(.4f, .23f), Hex("bfcdc0"), 18, root);
                    Shape("muzzle hole", disc, new Vector2(.59f, .77f), new Vector2(.07f, .17f), dark, 19, root);
                    for (int i = -1; i <= 1; i += 2) Shape("reed leaf", disc, new Vector2(i * .24f, .27f), new Vector2(.52f, .15f), moss, 17, root).transform.localRotation = Quaternion.Euler(0, 0, i * 32);
                    Shape("spitter eye", disc, new Vector2(.2f, .94f), Vector2.one * .08f, light, 19, root);
                }
                else
                {
                    art.LeftWing = Shape("left moth wing", disc, new Vector2(-.4f, .43f), new Vector2(.75f, .85f), Hex("77bcc1"), 16, root).transform;
                    art.RightWing = Shape("right moth wing", disc, new Vector2(.4f, .43f), new Vector2(.75f, .85f), Hex("77bcc1"), 16, root).transform;
                    Shape("lantern body", disc, new Vector2(0, .33f), new Vector2(.36f, .65f), cyan, 18, root);
                    Shape("lantern wick", disc, new Vector2(0, .25f), Vector2.one * .14f, Color.white, 19, root);
                }
                art.Parts = root.GetComponentsInChildren<SpriteRenderer>(); art.Colors = new Color[art.Parts.Length];
                for (int i = 0; i < art.Parts.Length; i++) art.Colors[i] = art.Parts[i].color;
                art.Glow = Shape("creature glow", disc, Vector2.zero, Vector2.one * 2, new Color(cyan.r, cyan.g, cyan.b, .07f), 4);
                art.Scent = Shape("scent ring", ring, Vector2.zero, Vector2.one * 1.5f, cyan, 28);
                art.Warning = Shape("attack tell", triangle, Vector2.zero, new Vector2(.22f, .3f), Hex("ffce85"), 29).transform;
                art.AimLine = Shape("locked attack path", square, Vector2.zero, new Vector2(1, .045f), new Color(1, .72f, .42f, .35f), 14).transform;
                for (int i = 0; i < enemy.MaxHealth; i++) art.Hearts.Add(Shape("enemy health", disc, Vector2.zero, Vector2.one * .075f, light, 29));
                wildlife.Add(art);
            }
            foreach (var bloom in game.Session.World.Blooms)
            {
                var art = new BloomArt(); art.Root = new GameObject("Moonbloom").transform; art.Root.SetParent(scenery); art.Root.position = Point(bloom.Position);
                Shape("bloom stalk", square, new Vector2(0, -.3f), new Vector2(.05f, .6f), moss, 11, art.Root);
                for (int i = 0; i < 5; i++)
                {
                    float angle = i * 72 * Mathf.Deg2Rad;
                    Shape("moon petal", disc, new Vector2(Mathf.Cos(angle) * .22f, Mathf.Sin(angle) * .22f), Vector2.one * .28f, Hex("a1d4ed"), 13, art.Root);
                }
                art.Core = Shape("moonbloom core", disc, Vector2.zero, Vector2.one * .24f, light, 14, art.Root);
                art.Halo = Shape("moonbloom light", disc, Vector2.zero, Vector2.one * 2, new Color(.5f, .8f, 1, .09f), 4, art.Root);
                blooms.Add(art);
            }
            for (int i = 0; i < platforms.Count; i++) if (game.Session.World.Platforms[i].Surface == Surface.Moonbridge)
            {
                var parts = platforms[i].GetComponentsInChildren<SpriteRenderer>(); var colors = new Color[parts.Length];
                for (int j = 0; j < parts.Length; j++) colors[j] = parts[j].color;
                bridges.Add(new BridgeArt { Index = i, Parts = parts, Colors = colors });
            }
            for (int i = 0; i < 24; i++) bolts.Add(Shape("glowing reed seed", disc, Vector2.zero, Vector2.one * .24f, Hex("ffd99d"), 24));
            for (int i = 0; i < 36; i++) clawLines.Add(Shape("claw arc", square, Vector2.zero, new Vector2(.25f, .03f), light, 30));
        }

        private void UpdateCombatArt()
        {
            var session = game.Session; var player = session.Player; float time = session.Time;
            pawLight.transform.position = Point(player.Position + new V2(0, .6f));
            bool protectedNow = session.Combat.Invulnerable > 0 || player.Dodging;
            pawLight.color = protectedNow ? new Color(.6f, .87f, 1, .17f) : new Color(1, .75f, .4f, .06f);
            for (int i = 0; i < wildlife.Count; i++)
            {
                var art = wildlife[i]; var e = session.World.Enemies[i];
                art.Root.gameObject.SetActive(e.Alive);
                art.Root.position = Point(e.Position);
                float squash = e.Phase == EnemyPhase.Tell && e.Kind != EnemyKind.LanternMoth ? .8f : 1;
                art.Root.localScale = new Vector3(e.Facing, squash, 1);
                for (int j = 0; j < art.Parts.Length; j++) art.Parts[j].color = e.HitFlash > 0 ? Color.Lerp(art.Colors[j], Color.white, .65f) : art.Colors[j];
                Vector3 above = Point(e.Position) + Vector3.up * (e.Height + .35f);
                bool tell = e.Alive && e.Phase == EnemyPhase.Tell;
                art.Warning.gameObject.SetActive(tell);
                art.Warning.position = above;
                art.Warning.localScale = new Vector3(.22f, .25f + .1f * Mathf.Sin(e.PhaseTime * 13), 1);
                art.Glow.enabled = e.Alive && (e.Kind == EnemyKind.LanternMoth || tell || e.Phase == EnemyPhase.Stunned);
                art.Glow.transform.position = Point(e.Bounds.Center);
                art.Glow.color = tell ? new Color(1, .65f, .3f, .12f) : new Color(.5f, .9f, 1, .1f);
                art.Scent.enabled = e.Alive && !e.Harmless && player.Stalking && (e.Position - player.Position).Length < 10;
                if (e.Kind == EnemyKind.MossHare) { art.Scent.enabled = e.Alive && player.Stalking && (e.Position - player.Position).Length < 10; art.Scent.color = Hex("ffe3a4"); }
                art.Scent.transform.position = Point(e.Bounds.Center);
                art.Scent.transform.localScale = Vector3.one * (1.3f + .1f * Mathf.Sin(time * 3));
                bool aim = tell && (e.Kind == EnemyKind.LanternMoth || e.Kind == EnemyKind.ReedSpitter || e.Kind == EnemyKind.Bristleback);
                art.AimLine.gameObject.SetActive(aim);
                if (aim)
                {
                    V2 end = e.Kind == EnemyKind.Bristleback ? e.Bounds.Center + new V2(e.Facing * 6, 0) : e.LockedTarget;
                    V2 delta = end - e.Bounds.Center;
                    art.AimLine.position = Point(e.Bounds.Center + delta * .5f);
                    art.AimLine.localScale = new Vector3(delta.Length, .045f, 1);
                    art.AimLine.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(delta.Y, delta.X) * Mathf.Rad2Deg);
                }
                for (int j = 0; j < art.Hearts.Count; j++)
                {
                    art.Hearts[j].enabled = e.Alive && j < e.Health && (e.Health < e.MaxHealth || tell || player.Stalking);
                    art.Hearts[j].transform.position = above + new Vector3((j - (e.MaxHealth - 1) / 2f) * .13f, .3f, 0);
                }
                if (art.LeftWing != null)
                {
                    float spread = e.Phase == EnemyPhase.Active ? .3f : .75f + Mathf.Sin(time * 16) * .15f;
                    art.LeftWing.localScale = art.RightWing.localScale = new Vector3(spread, .85f, 1);
                }
            }
            for (int i = 0; i < blooms.Count; i++)
            {
                var state = session.World.Blooms[i]; var art = blooms[i];
                float burst = Mathf.Clamp01(state.GlowTime / 2);
                art.Halo.transform.localScale = Vector3.one * (2 + burst * 6);
                art.Halo.color = new Color(.48f, .8f, 1, .08f + burst * .11f);
                art.Core.color = state.Awakened ? Color.white : light;
            }
            foreach (var bridge in bridges)
            {
                bool enabled = session.World.Platforms[bridge.Index].Enabled;
                for (int i = 0; i < bridge.Parts.Length; i++)
                { Color c = bridge.Colors[i]; c.a = enabled ? 1 : .09f; bridge.Parts[i].color = c; }
            }
            for (int i = 0; i < bolts.Count; i++)
            {
                bolts[i].enabled = i < session.Projectiles.Count;
                if (bolts[i].enabled) bolts[i].transform.position = Point(session.Projectiles[i].Position);
            }
            DrawClaws();
        }

        private void DrawClaws()
        {
            var c = game.Session.Combat; var p = game.Session.Player;
            for (int i = 0; i < clawLines.Count; i++)
            {
                var line = clawLines[i]; line.enabled = c.Active;
                if (!c.Active) continue;
                int row = i / 12, segment = i % 12;
                float angle = Mathf.Lerp(-65, 85, segment / 11f);
                if (c.Move == ClawMove.DownRake) angle = Mathf.Lerp(195, 345, segment / 11f);
                else if (c.Move == ClawMove.RisingRake) angle = Mathf.Lerp(15, 165, segment / 11f);
                float radius = (c.Timing.Reach + (c.Empowered ? .45f : 0)) * (.7f + row * .07f), radians = angle * Mathf.Deg2Rad;
                Vector3 center = Point(p.Position) + Vector3.up * (c.Move == ClawMove.DownRake ? .3f : .55f);
                line.transform.position = center + new Vector3(Mathf.Cos(radians) * radius * c.Facing, Mathf.Sin(radians) * radius, 0);
                line.transform.localRotation = Quaternion.Euler(0, 0, c.Facing > 0 ? angle + 90 : 90 - angle);
                line.transform.localScale = new Vector3(radius * .25f, .025f + row * .007f, 1);
                line.color = c.Empowered ? Hex("b1f7ff") : Hex("fff0c8");
            }
        }

        private void ApplyCombatPose()
        {
            var c = game.Session.Combat;
            if (c.Busy && paws.Count > 3)
            {
                paws[3].localPosition = new Vector3(c.Active ? .9f : .28f, c.Move == ClawMove.RisingRake ? 1.1f : .65f, 0);
                paws[3].localRotation = Quaternion.Euler(0, 0, c.Active ? -75 : 35);
                head.localRotation = Quaternion.Euler(0, 0, c.Active ? 8 : -10);
            }
        }

        private void CombatFeedback(GameEvent e)
        {
            if ((e & (GameEvent.Hit | GameEvent.Block | GameEvent.Bloom | GameEvent.Moonbell | GameEvent.Breach)) != 0)
                Emit((e & GameEvent.Bloom) != 0 ? 22 : 12, game.Session.Combat.LastImpact, (e & GameEvent.Block) != 0 ? Hex("ffc78e") : light);
            if ((e & (GameEvent.Hit | GameEvent.DashClaw)) != 0) shake = .07f;
            if ((e & GameEvent.Hurt) != 0) { shake = .14f; Emit(14, game.Session.Player.Position, Hex("e6a2a2")); }
            if ((e & GameEvent.Defeat) != 0) Emit(20, game.Session.Combat.LastImpact, moss);
            if ((e & (GameEvent.Balance | GameEvent.Waystone)) != 0) Emit(18, game.Session.Player.Position, light);
        }
    }
}
