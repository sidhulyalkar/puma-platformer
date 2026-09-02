using System.Collections.Generic;
using UnityEngine;
using Wildbound.Core;

namespace Wildbound.Unity
{
    /// <summary>Original lightweight cut-paper art. No downloaded art or editor-generated assets required.</summary>
    public sealed partial class WorldView : MonoBehaviour
    {
        private WildboundGame game;
        private Camera cameraView;
        private Transform scenery, far, near, cat;
        private Sprite square, disc, triangle;
        private Material flat;
        private readonly List<Transform> platforms = new List<Transform>();
        private readonly List<Transform> pickups = new List<Transform>();
        private readonly List<SpriteRenderer> checkpoints = new List<SpriteRenderer>();
        private readonly List<Transform> paws = new List<Transform>();
        private readonly List<Transform> tail = new List<Transform>();
        private readonly List<Spark> sparks = new List<Spark>();
        private Transform body, head;
        private float impact, shake, trailClock;
        private Vector3 cameraVelocity;
        private Color sky, distant, rock, moss, light;
        private System.Random random = new System.Random(407);
        private sealed class Spark { public SpriteRenderer Renderer; public Vector3 Velocity; public float Life, MaxLife; }

        public void Initialize(WildboundGame owner)
        {
            game = owner;
            flat = new Material(Resources.Load<Shader>("WildboundFlat"));
            square = MakeSprite(0); disc = MakeSprite(1); triangle = MakeSprite(2);
            cameraView = Camera.main;
            if (cameraView == null) { var obj = new GameObject("World Camera"); obj.tag = "MainCamera"; cameraView = obj.AddComponent<Camera>(); }
            cameraView.orthographic = true; cameraView.orthographicSize = 7.8f;
            cameraView.nearClipPlane = .1f; cameraView.farClipPlane = 100;
            if (FindFirstObjectByType<AudioListener>() == null) cameraView.gameObject.AddComponent<AudioListener>();
            Rebuild();
        }
        private static Color Hex(string value) { Color c; ColorUtility.TryParseHtmlString("#" + value, out c); return c; }
        private void Palette()
        {
            int b = game.Session.Save.Biome;
            sky = Hex(new[] { "0b1527", "0f1225", "101c35" }[b]);
            distant = Hex(new[] { "172d3c", "242743", "2b3e58" }[b]);
            rock = Hex(new[] { "223840", "30324e", "34475d" }[b]);
            moss = Hex(new[] { "80b8b0", "77c9ba", "a0bfd2" }[b]);
            light = Hex(new[] { "ffe6ae", "b4f7e0", "fff0c9" }[b]);
            cameraView.backgroundColor = sky;
        }
        public void Rebuild()
        {
            if (scenery != null) { scenery.gameObject.SetActive(false); Destroy(scenery.gameObject); }
            platforms.Clear(); pickups.Clear(); checkpoints.Clear(); paws.Clear(); tail.Clear(); sparks.Clear();
            random = new System.Random(407 + game.Session.Save.Biome);
            scenery = new GameObject("World art").transform; scenery.SetParent(transform);
            Palette(); Background();
            foreach (var p in game.Session.World.Platforms) BuildPlatform(p);
            foreach (var h in game.Session.World.Hazards)
            {
                for (float x = h.X; x < h.Right; x += .35f)
                    Shape("bramble", triangle, new Vector2(x + .18f, h.Y + .24f), new Vector2(.45f, .5f), Hex("e69d9c"), 12);
            }
            foreach (var p in game.Session.World.Pickups)
            {
                var root = new GameObject(p.Kind == PickupKind.Memory ? "Hidden memory" : "Light mote").transform;
                root.SetParent(scenery); root.position = Point(p.Position);
                if (p.Kind == PickupKind.Memory)
                {
                    Shape("halo", disc, Vector2.zero, new Vector2(1.2f, 1.2f), new Color(.8f, .95f, .9f, .1f), 13, root);
                    Shape("memory", square, Vector2.zero, new Vector2(.5f, .5f), light, 14, root).transform.localRotation = Quaternion.Euler(0, 0, 45);
                    Shape("heart", square, Vector2.zero, new Vector2(.2f, .2f), sky, 15, root).transform.localRotation = Quaternion.Euler(0, 0, 45);
                }
                else
                {
                    Shape("glow", disc, Vector2.zero, new Vector2(.65f, .65f), new Color(1, .83f, .52f, .12f), 13, root);
                    Shape("seed", disc, Vector2.zero, new Vector2(.22f, .34f), Hex("ffe6a5"), 14, root);
                }
                pickups.Add(root);
            }
            foreach (var p in game.Session.World.Checkpoints)
            {
                Shape("trail stone", disc, new Vector2(p.X, p.Y + .45f), new Vector2(.8f, .9f), rock, 11);
                var mark = Shape("trail rune", square, new Vector2(p.X, p.Y + .56f), new Vector2(.17f, .28f), light, 12);
                mark.transform.localRotation = Quaternion.Euler(0, 0, 45); checkpoints.Add(mark);
                Shape("beacon stem", square, new Vector2(p.X, p.Y + 1.3f), new Vector2(.04f, .5f), moss, 11);
                Shape("beacon bud", disc, new Vector2(p.X, p.Y + 1.6f), new Vector2(.22f, .22f), light, 12);
            }
            BuildCombatArt();
            BuildTrialArt();
            BuildExplorationArt();
            if (!game.Session.InTrial) BuildPortal(game.Session.World.Exit);
            BuildPuma();
            for (int i = 0; i < 72; i++)
            {
                var r = Shape("spark", disc, Vector2.zero, Vector2.one * .1f, light, 25); r.enabled = false;
                sparks.Add(new Spark { Renderer = r });
            }
            SnapCamera();
        }
        private void Background()
        {
            far = new GameObject("Distant silhouettes").transform; far.SetParent(scenery);
            near = new GameObject("Near silhouettes").transform; near.SetParent(scenery);
            int biome = game.Session.Save.Biome;
            Shape("moon glow", disc, new Vector2(12, 10), new Vector2(7, 7), new Color(light.r, light.g, light.b, .04f), -45, far);
            Shape("moon", disc, new Vector2(12, 10), new Vector2(3, 3), new Color(light.r, light.g, light.b, .55f), -44, far);
            for (int i = 0; i < 24; i++)
            {
                float x = i * 8 - 55, h = Range(7, 20);
                Shape("ridge", triangle, new Vector2(x, h / 2 - 5), new Vector2(Range(13, 22), h), distant, -40, far);
                if (biome == 1)
                {
                    Shape("cave tooth", triangle, new Vector2(x, 19), new Vector2(Range(4, 8), Range(6, 12)), rock, -32, near).transform.localRotation = Quaternion.Euler(0, 0, 180);
                    Shape("lantern stem", square, new Vector2(x + 2, 9), new Vector2(.04f, 3), distant, -31, near);
                    Shape("hanging light", disc, new Vector2(x + 2, 7.5f), new Vector2(.3f, .5f), moss, -30, near);
                }
                else if (biome == 0)
                {
                    Shape("ancient trunk", square, new Vector2(x, 8), new Vector2(.65f, 23), distant, -31, near);
                    for (int j = 0; j < 3; j++)
                        Shape("canopy", disc, new Vector2(x + Range(-2, 2), 14 + j * 2), new Vector2(7, 4), distant, -30, near);
                    Shape("root", triangle, new Vector2(x, 0), new Vector2(3, 8), distant, -31, near);
                }
                else
                {
                    Shape("floating island", triangle, new Vector2(x, 8), new Vector2(4, 6), distant, -30, near).transform.localRotation = Quaternion.Euler(0, 0, 180);
                    Shape("island meadow", disc, new Vector2(x, 11), new Vector2(4, .4f), moss * new Color(.7f, .7f, .7f, 1), -29, near);
                }
            }
            for (int i = 0; i < 90; i++) Shape("firefly", disc, new Vector2(Range(-20, 115), Range(-1, 23)), Vector2.one * Range(.025f, .08f), new Color(light.r, light.g, light.b, .4f), -25, near);
        }
        private void BuildPlatform(Platform p)
        {
            var root = new GameObject(p.Surface.ToString()).transform; root.SetParent(scenery); root.position = Point(p.Bounds.Center);
            platforms.Add(root);
            float w = p.Bounds.W, h = p.Bounds.H;
            Shape("earth", square, Vector2.zero, new Vector2(w, h), rock, 5, root);
            Shape("grassy lip", square, new Vector2(0, h / 2 - .07f), new Vector2(w, .14f), moss, 7, root);
            Shape("edge shadow", square, new Vector2(0, h / 2 - .22f), new Vector2(w, .1f), rock * new Color(.7f, .7f, .7f, 1), 6, root);
            if (p.Surface == Surface.Moonbridge || p.Surface == Surface.Trailbridge)
            {
                Color edge = p.Surface == Surface.Trailbridge ? Hex("ffd88a") : light;
                Shape("moon edge", square, new Vector2(0, h / 2), new Vector2(w, .06f), edge, 9, root);
                for (int i = 0; i < w; i++) Shape("moon rune", disc, new Vector2(i - w / 2 + .5f, 0), Vector2.one * .1f, edge, 10, root);
            }
            else if (p.Surface == Surface.RootGate)
            {
                for (float y = -h / 2; y < h / 2; y += .55f)
                {
                    Shape("braided root", square, new Vector2(0, y), new Vector2(w * 1.3f, .13f), Hex("dbac86"), 9, root).transform.localRotation = Quaternion.Euler(0, 0, 28);
                    Shape("root seam", square, new Vector2(0, y + .2f), new Vector2(w * .75f, .045f), light, 10, root);
                }
            }
            else if (p.Surface == Surface.Spring)
            {
                for (int i = -2; i <= 2; i++)
                    Shape("spring petal", disc, new Vector2(i * .26f, h / 2 + .12f + Mathf.Abs(i) * .07f), new Vector2(.6f, .24f), Hex("f1a5b6"), 10, root);
                Shape("spring center", disc, new Vector2(0, h / 2 + .23f), new Vector2(.4f, .19f), Hex("fff0c2"), 11, root);
            }
            else if (h < 10)
            {
                for (float x = -w / 2 + .25f; x < w / 2; x += Range(.5f, 1.4f))
                {
                    float grass = Range(.12f, .32f);
                    Shape("grass", triangle, new Vector2(x, h / 2 + grass / 2), new Vector2(.18f, grass), moss, 8, root);
                    if (Range(0, 1) < .24f)
                    {
                        Shape("stem", square, new Vector2(x, h / 2 + .22f), new Vector2(.025f, .4f), moss, 8, root);
                        Shape("wildflower", disc, new Vector2(x, h / 2 + .44f), Vector2.one * .13f, light, 9, root);
                    }
                }
                for (int i = 0; i < w / 1.5f; i++) Shape("stone fleck", square, new Vector2(Range(-w / 2 + .2f, w / 2 - .2f), Range(-h / 2 + .1f, h / 2 - .25f)), new Vector2(.18f, .05f), distant, 6, root);
            }
            if (p.Surface == Surface.Moving || p.Surface == Surface.Balance)
                Shape("floating heart", disc, new Vector2(0, -h / 2 - .2f), new Vector2(.3f, .3f), light, 7, root);
        }
        private void BuildPortal(V2 p)
        {
            Shape("arch light", disc, new Vector2(p.X, p.Y + 1.9f), new Vector2(3, 4.2f), new Color(light.r, light.g, light.b, .11f), 8);
            for (int i = 0; i <= 12; i++)
            {
                float angle = Mathf.PI * i / 12;
                Vector2 pos = new Vector2(p.X + Mathf.Cos(angle) * 1.5f, p.Y + 2.4f + Mathf.Sin(angle) * 1.5f);
                var stone = Shape("arch stone", square, pos, new Vector2(.58f, .53f), moss, 10);
                stone.transform.localRotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
            }
            Shape("arch left", square, new Vector2(p.X - 1.5f, p.Y + 1.15f), new Vector2(.52f, 2.3f), moss, 10);
            Shape("arch right", square, new Vector2(p.X + 1.5f, p.Y + 1.15f), new Vector2(.52f, 2.3f), moss, 10);
            Shape("arch rune", square, new Vector2(p.X, p.Y + 2.1f), new Vector2(.35f, .35f), light, 11).transform.localRotation = Quaternion.Euler(0, 0, 45);
        }
        private void BuildPuma()
        {
            cat = new GameObject("Puma").transform; cat.SetParent(scenery);
            Color gold = Hex("edb878"), shaded = Hex("b87d53"), cream = Hex("ffe5b2"), ink = Hex("303647");
            for (int i = 0; i < 4; i++)
            {
                var paw = Shape("paw", disc, new Vector2(i < 2 ? -.35f : .37f, .24f), new Vector2(.21f, .43f), i % 2 == 0 ? shaded : gold, 19 + i % 2, cat).transform;
                paws.Add(paw);
            }
            for (int i = 0; i < 7; i++) tail.Add(Shape("tail", disc, new Vector2(-.65f - i * .13f, .64f), new Vector2(.25f, .17f), i == 6 ? shaded : gold, 19, cat).transform);
            body = Shape("body", disc, new Vector2(-.04f, .6f), new Vector2(1.22f, .57f), gold, 21, cat).transform;
            Shape("belly", disc, new Vector2(.02f, .41f), new Vector2(.75f, .18f), cream, 22, cat);
            head = new GameObject("head").transform; head.SetParent(cat); head.localPosition = new Vector3(.54f, .79f, 0);
            Shape("face", disc, Vector2.zero, new Vector2(.55f, .53f), gold, 23, head);
            for (int i = -1; i <= 1; i += 2)
            {
                Shape("ear", triangle, new Vector2(i * .17f, .23f), new Vector2(.22f, .29f), shaded, 22, head);
                Shape("ear inset", triangle, new Vector2(i * .17f, .22f), new Vector2(.1f, .14f), Hex("e4aa92"), 23, head);
            }
            Shape("muzzle", disc, new Vector2(.2f, -.1f), new Vector2(.3f, .23f), cream, 24, head);
            Shape("nose", disc, new Vector2(.32f, -.055f), new Vector2(.1f, .075f), ink, 25, head);
            Shape("eye", disc, new Vector2(.13f, .065f), new Vector2(.10f, .13f), ink, 25, head);
            Shape("eye glint", disc, new Vector2(.15f, .09f), new Vector2(.033f, .039f), Color.white, 26, head);
        }
        public void SnapCamera()
        {
            if (cameraView == null) return;
            var p = game.Session.Player.Position;
            cameraView.transform.position = new Vector3(Mathf.Clamp(p.X + 3, 8, game.Session.World.CameraMaxX), Mathf.Clamp(p.Y + 4.1f, 5.7f, game.Session.World.CameraMaxY), -10);
            cameraVelocity = Vector3.zero;
        }
        public void React(GameEvent e)
        {
            if ((e & (GameEvent.Land | GameEvent.Spring | GameEvent.Pounce | GameEvent.WallKick | GameEvent.Stomp)) != 0)
            { impact = .22f; Emit(9, game.Session.Player.Position, moss); }
            if ((e & GameEvent.Pounce) != 0) shake = .12f;
            CombatFeedback(e);
            if ((e & (GameEvent.Collect | GameEvent.Secret | GameEvent.Discovery)) != 0) Emit(12, game.Session.Player.Position + new V2(0, .8f), light);
        }
        private void LateUpdate()
        {
            if (game == null || cat == null) return;
            var session = game.Session; var p = session.Player;
            float dt = Time.deltaTime, t = session.Time;
            for (int i = 0; i < platforms.Count; i++)
            {
                var platform = session.World.Platforms[i];
                platforms[i].position = Point(platform.Bounds.Center);
                if (platform.Surface == Surface.RootGate) platforms[i].gameObject.SetActive(platform.Enabled);
            }
            for (int i = 0; i < pickups.Count; i++)
            {
                var pickup = session.World.Pickups[i]; pickups[i].gameObject.SetActive(!pickup.Collected);
                pickups[i].position = Point(pickup.Position) + Vector3.up * (game.ReducedMotion ? 0 : Mathf.Sin(t * 2.3f + i) * .12f);
            }
            UpdateCombatArt();
            UpdateTrialArt();
            UpdateExplorationArt();
            for (int i = 0; i < checkpoints.Count; i++) checkpoints[i].color = session.Save.Checkpoints[session.Save.Biome] == i ? Hex("fff6d8") : distant;
            cat.position = Point(p.Position);
            float crouch = p.LowProfile ? .42f : p.Stalking ? .22f : p.Charging ? p.Charge * .28f : 0;
            cat.localScale = new Vector3(p.Facing * (1 + crouch * .3f), 1 - crouch, 1);
            cat.localRotation = Quaternion.Euler(0, 0, p.RollTime > 0 ? p.Facing * (1 - p.RollTime / p.Tuning.RollSeconds) * -360 : 0);
            float gait = p.Grounded ? Mathf.Sin(t * Mathf.Abs(p.Velocity.X) * 2.1f) : 0;
            body.localRotation = Quaternion.Euler(0, 0, p.Grounded ? gait * 2 : Mathf.Clamp(p.Velocity.Y, -18, 18) * .7f);
            head.localRotation = Quaternion.Euler(0, 0, p.Charging ? -12 : Mathf.Sin(t * 2) * 3);
            for (int i = 0; i < paws.Count; i++)
            {
                float stride = Mathf.Sin(t * Mathf.Abs(p.Velocity.X) * 2.1f + i * Mathf.PI * .7f);
                paws[i].localPosition = new Vector3((i < 2 ? -.35f : .37f) + (p.Grounded ? stride * .12f : (i < 2 ? -.16f : .18f)), p.Grounded ? .23f + Mathf.Max(0, stride) * .08f : .35f, 0);
                paws[i].localRotation = Quaternion.Euler(0, 0, p.Grounded ? stride * 28 : (i < 2 ? -50 : 50));
            }
            for (int i = 0; i < tail.Count; i++) tail[i].localPosition = new Vector3(-.57f - i * .13f, .65f + Mathf.Sin(t * 3 - i * .45f) * .13f + i * .035f, 0);
            ApplyCombatPose();
            if (!session.Paused)
            {
                impact = Mathf.Max(0, impact - dt); shake = Mathf.Max(0, shake - dt);
                if (p.PounceTime > 0 && (trailClock -= dt) <= 0) { trailClock = .025f; Emit(2, p.Position + new V2(-p.Facing * .5f, .55f), light); }
                foreach (var s in sparks)
                {
                    if (s.Life <= 0) continue; s.Life -= dt;
                    s.Renderer.enabled = s.Life > 0; s.Renderer.transform.position += s.Velocity * dt; s.Velocity.y -= dt * 5;
                    Color c = s.Renderer.color; c.a = Mathf.Max(0, s.Life / s.MaxLife); s.Renderer.color = c;
                }
            }
            var target = new Vector3(Mathf.Clamp(p.Position.X + p.Facing * 2.8f, 8, session.World.CameraMaxX), Mathf.Clamp(p.Position.Y + 4.1f, 5.7f, session.World.CameraMaxY), -10);
            cameraView.transform.position = Vector3.SmoothDamp(cameraView.transform.position, target, ref cameraVelocity, game.ReducedMotion ? .1f : .23f, Mathf.Infinity, dt);
            if (!game.ReducedMotion && shake > 0 && !session.Paused) cameraView.transform.position += new Vector3(Mathf.Sin(t * 111) * shake * .35f, 0, 0);
            far.localPosition = new Vector3(cameraView.transform.position.x * .75f, cameraView.transform.position.y * .4f, 0);
            near.localPosition = new Vector3(cameraView.transform.position.x * .4f, cameraView.transform.position.y * .12f, 0);
        }
        private void Emit(int count, V2 point, Color color)
        {
            if (game.ReducedMotion) count = Mathf.Min(3, count);
            foreach (var s in sparks)
            {
                if (s.Life > 0) continue;
                s.Life = s.MaxLife = Range(.22f, .55f); s.Velocity = new Vector3(Range(-3, 3), Range(1, 4), 0);
                s.Renderer.enabled = true; s.Renderer.color = color; s.Renderer.transform.position = Point(point);
                s.Renderer.transform.localScale = Vector3.one * Range(.05f, .13f);
                if (--count <= 0) break;
            }
        }
        private SpriteRenderer Shape(string name, Sprite sprite, Vector2 position, Vector2 size, Color color, int order, Transform parent = null)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent != null ? parent : scenery, false);
            obj.transform.localPosition = new Vector3(position.x, position.y, 0); obj.transform.localScale = new Vector3(size.x, size.y, 1);
            var renderer = obj.AddComponent<SpriteRenderer>(); renderer.sharedMaterial = flat; renderer.sprite = sprite; renderer.color = color; renderer.sortingOrder = order; return renderer;
        }
        private static Sprite MakeSprite(int shape)
        {
            const int size = 64; var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
            {
                float px = (x + .5f) / size * 2 - 1, py = (y + .5f) / size * 2 - 1;
                float radius = px * px + py * py;
                bool fill = shape == 0 || (shape == 1 ? radius < .97f : shape == 3 ? radius < .97f && radius > .78f : Mathf.Abs(px) < (1 - py) * .5f);
                pixels[y * size + x] = fill ? Color.white : Color.clear;
            }
            texture.SetPixels(pixels); texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
        }
        private float Range(float a, float b) { return Mathf.Lerp(a, b, (float)random.NextDouble()); }
        private static Vector3 Point(V2 p) { return new Vector3(p.X, p.Y, 0); }
        private void OnDestroy()
        {
            foreach (var s in new[] { square, disc, triangle, ring }) if (s != null) { Destroy(s.texture); Destroy(s); }
            if (flat != null) Destroy(flat);
        }
    }
}
