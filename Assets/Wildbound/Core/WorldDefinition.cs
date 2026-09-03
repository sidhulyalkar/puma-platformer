using System;
using System.Collections.Generic;

namespace Wildbound.Core
{
    public enum Surface { Stone, Moss, Spring, Moving, Moonbridge, Balance, RootGate, Trailbridge, Vine, Bark }
    public enum PickupKind { Mote, Memory }

    public sealed class Platform
    {
        public Box Home, Bounds;
        public Surface Surface;
        public float Travel, Phase;
        public V2 Delta;
        public bool Enabled = true;
        public int LightSource = -1, DiscoverySource = -1;
        public Platform(Box box, Surface surface, float travel = 0, float phase = 0)
        { Home = Bounds = box; Surface = surface; Travel = travel; Phase = phase; }
        public void Update(float time)
        {
            Box next = Home.Offset(new V2((float)Math.Sin(time * .85f + Phase) * Travel, 0));
            Delta = next.Center - Bounds.Center; Bounds = next;
        }
    }
    public sealed class Pickup
    {
        public V2 Position; public PickupKind Kind; public bool Collected;
        public Pickup(float x, float y, PickupKind kind = PickupKind.Mote) { Position = new V2(x, y); Kind = kind; }
    }
    public sealed class Sign
    {
        public V2 Position; public string Heading, Text;
        public Sign(float x, float y, string heading, string text) { Position = new V2(x, y); Heading = heading; Text = text; }
    }
    public sealed partial class WorldDefinition
    {
        public string Name, Subtitle, Memory;
        public int Biome;
        public float CameraMaxX = 73, CameraMaxY = 20;
        public Box MapBounds = new Box(-6, -3, 88, 29);
        public Moontrial Trial;
        public V2 Spawn = new V2(2, 1), Exit = new V2(75, 1);
        public readonly List<Platform> Platforms = new List<Platform>();
        public readonly List<Pickup> Pickups = new List<Pickup>();
        public readonly List<Box> Hazards = new List<Box>();
        public readonly List<V2> Checkpoints = new List<V2>();
        public readonly List<Sign> Signs = new List<Sign>();
        public readonly List<Enemy> Enemies = new List<Enemy>();
        public readonly List<WildPlace> Places = new List<WildPlace>();
        public readonly List<Moonbloom> Blooms = new List<Moonbloom>();
        public readonly List<WindField> WindFields = new List<WindField>();
        public readonly List<ScentMark> ScentMarks = new List<ScentMark>();
        public readonly List<EncounterPack> Encounters = new List<EncounterPack>();
        public void Add(float x, float y, float w, float h = 1, Surface surface = Surface.Moss, float travel = 0)
        { Platforms.Add(new Platform(new Box(x, y, w, h), surface, travel)); }

        public static WorldDefinition Create(int biome)
        {
            if (biome < 0 || biome > 3) throw new ArgumentOutOfRangeException("biome");
            var w = new WorldDefinition { Biome = biome };
            w.Name = new[] { "THE AMBER CANOPY", "THE LANTERN GROTTO", "THE SKY GARDEN", "THE CINDER RAVINE" }[biome];
            w.Subtitle = new[] { "Quiet paws beneath an amber moon.", "Wake the flowers. Follow their light.", "Hunt among the stars.", "Warm stone. Quiet fire. Climb the living bark." }[biome];
            w.Memory = new[] { "The forest remembers every small beginning.", "Even the quietest places are full of life.", "Home is a trail you can choose again.", "Warmth is a trail you carry forward." }[biome];
            w.Checkpoints.Add(new V2(23, 1)); w.Checkpoints.Add(new V2(60, 1));
            w.Add(-6, -3, 1, 38, Surface.Stone); w.Add(81, -3, 1, 38, Surface.Stone);
            w.Enemies.Add(new Enemy(EnemyKind.ClawPost, 5, 1));
            w.Enemies.Add(new Enemy(EnemyKind.MossHare, 7.6f, 1, 1.1f));
            if (biome == 0) BuildCanopy(w);
            else if (biome == 1) BuildGrotto(w);
            else if (biome == 2) BuildSky(w);
            else BuildCinder(w);
            w.Signs.Add(new Sign(3, 1, "QUIET PAWS. A WIDE WORLD.", "A / D or arrows to move. SPACE / gamepad A jumps; hold for height. Pale edges mark the places your paws can land."));
            w.Signs.Add(new Sign(5, 1, "A HUNTER'S HANDS", "J / RB to claw the scratch post. Hold Q / LT to stalk prey and bring nearby scent tracks into focus."));
            w.Signs.Add(new Sign(23, 1, "A PLACE TO RETURN TO", "This shelter remembers your trail. Wild places open golden paths home. Every discovery is optional."));
            w.Signs.Add(new Sign(74, 1, "A DOOR TO SOMEWHERE", biome >= 3 ? "E / Y at the arch finishes the journey. Your wild places and waystones remain yours." : "E / Y at the arch discovers the next world. You can return through the map."));
            return w;
        }
    }
}
