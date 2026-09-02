using System;
using System.Collections.Generic;

namespace Wildbound.Core
{
    public enum Surface { Stone, Moss, Spring, Moving, Moonbridge }
    public enum PickupKind { Mote, Memory }

    public sealed class Platform
    {
        public Box Home, Bounds;
        public Surface Surface;
        public float Travel, Phase;
        public V2 Delta;
        public bool Enabled = true;
        public int LightSource = -1;
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
    public sealed class WorldDefinition
    {
        public string Name, Subtitle, Memory;
        public int Biome;
        public V2 Spawn = new V2(2, 1), Exit = new V2(75, 1);
        public readonly List<Platform> Platforms = new List<Platform>();
        public readonly List<Pickup> Pickups = new List<Pickup>();
        public readonly List<Box> Hazards = new List<Box>();
        public readonly List<V2> Checkpoints = new List<V2>();
        public readonly List<Sign> Signs = new List<Sign>();
        public readonly List<Enemy> Enemies = new List<Enemy>();
        public readonly List<Moonbloom> Blooms = new List<Moonbloom>();
        public void Add(float x, float y, float w, float h = 1, Surface surface = Surface.Moss, float travel = 0)
        { Platforms.Add(new Platform(new Box(x, y, w, h), surface, travel)); }

        public static WorldDefinition Create(int biome)
        {
            if (biome < 0 || biome > 2) throw new ArgumentOutOfRangeException("biome");
            var w = new WorldDefinition { Biome = biome };
            w.Name = new[] { "THE AMBER CANOPY", "THE LANTERN GROTTO", "THE SKY GARDEN" }[biome];
            w.Subtitle = new[] { "Quiet paws beneath an amber moon.", "Wake the flowers. Follow their light.", "Hunt among the stars." }[biome];
            w.Memory = new[] { "The forest remembers every small beginning.", "Even the quietest places are full of life.", "Home is a trail you can choose again." }[biome];
            // Broad recovery floors alternate with traversable gaps; upper routes are optional.
            w.Add(-5, -3, 21, 4); w.Add(19, -3, 15, 4); w.Add(38, -3, 15, 4); w.Add(58, -3, 23, 4);
            w.Add(-6, -3, 1, 25, Surface.Stone); w.Add(81, -3, 1, 25, Surface.Stone);
            w.Add(9, 1, 3, 1.5f); w.Add(14, 3, 4); w.Add(21, 5, 5);
            w.Add(28, 2.6f, 4); w.Add(34, .4f, 4, .6f, Surface.Moving, 1.2f);
            w.Add(39, 1, 2, .35f, Surface.Spring);
            w.Add(43, 6, 4); w.Add(49, 9, 5); w.Add(54, 4, 4, .7f, Surface.Moving, 1.1f);
            w.Add(61, 2.8f, 5); w.Add(69, 4.5f, 4);
            // Wall-kick alcove / high memory perch. The main route stays open below it.
            w.Add(45, 1, 1, 4, Surface.Stone); w.Add(49, 1, 1, 5.5f, Surface.Stone);
            w.Add(24, 8, 4); w.Add(29, 10.5f, 4); w.Add(35, 12.5f, 4);
            w.Pickups.Add(new Pickup(37, 14, PickupKind.Memory));
            float[] xs = { 7, 11, 16, 23, 30, 36, 40, 45, 51, 56, 63, 71 };
            float[] ys = { 2.2f, 3.7f, 5.2f, 7.2f, 4.8f, 2.5f, 3.1f, 8.2f, 11.2f, 6, 5, 6.7f };
            for (int i = 0; i < xs.Length; i++) w.Pickups.Add(new Pickup(xs[i], ys[i]));
            w.Checkpoints.Add(new V2(23, 1)); w.Checkpoints.Add(new V2(60, 1));
            w.Hazards.Add(new Box(30, 1, 2, .45f)); w.Hazards.Add(new Box(66, 1, 1.8f, .45f));
            w.Enemies.Add(new Enemy(EnemyKind.ClawPost, 5, 1));
            w.Enemies.Add(new Enemy(EnemyKind.MossHare, 7.6f, 1, 1.1f));
            w.Enemies.Add(new Enemy(EnemyKind.Thornling, 26.8f, 1, 1.2f));
            w.Enemies.Add(new Enemy(EnemyKind.Bristleback, 71, 1, 2));
            if (biome > 0)
            {
                w.Enemies.Add(new Enemy(EnemyKind.LanternMoth, 35, 7.5f, 2));
                // This moth guards the second bloom within its unobstructed dazzle radius.
                w.Enemies.Add(new Enemy(EnemyKind.LanternMoth, 47.5f, 5.5f, 1));
            }
            w.Enemies.Add(new Enemy(EnemyKind.ReedSpitter, biome == 2 ? 63 : 51, biome == 2 ? 3.8f : 10));
            w.Blooms.Add(new Moonbloom(12.8f, 1.6f));
            w.Blooms.Add(new Moonbloom(47.5f, 1.6f));
            w.Platforms.Add(new Platform(new Box(18, 6.8f, 3, .35f), Surface.Moonbridge) { LightSource = 0, Enabled = false });
            w.Platforms.Add(new Platform(new Box(54, 11.8f, 4, .35f), Surface.Moonbridge) { LightSource = 1, Enabled = false });
            w.Signs.Add(new Sign(3, 1, "A SMALL CAT. A WIDE WORLD.", "A / D or arrows to roam. SPACE to jump. Hold for height; tap for a hop."));
            w.Signs.Add(new Sign(13, 1, "COIL. THEN FLY.", "Hold SHIFT to crouch and charge. Release to pounce. W / S aim up or down."));
            w.Signs.Add(new Sign(23, 1, "TAKE THE SCENIC ROUTE", "Glowing stones remember your trail. The high path hides a memory. Every discovery is optional."));
            w.Signs.Add(new Sign(39, 1, "BORROW A LITTLE SPRING", "Land on a pink flower to bounce. It restores your pounce. Charge again in the air!"));
            w.Signs.Add(new Sign(47, 1, "WAKE THE MOON", "J to claw a moonbloom. Her strike wakes a lasting light bridge and dazzles nearby moths."));
            w.Signs.Add(new Sign(5.5f, 1, "A HUNTER'S HANDS", "J to claw. Tap again as a sweep ends to chain three strikes. W + J lifts. Try the scratch post."));
            w.Signs.Add(new Sign(8, 1, "QUIET PAWS", "Hold Q to stalk and see scents. Hares flee in hops. Catch one to restore a heart and your pounce."));
            w.Signs.Add(new Sign(25, 1, "READ THE LEAP", "Thornlings curl, then leap. L rolls through; K lunges with claws. Airborne S + J rebounds on a hit."));
            w.Signs.Add(new Sign(68, 1, "LET HIM COMMIT", "Bristlebacks guard their fronts. Watch the amber tell, dodge the charge, then claw the exposed back."));
            w.Signs.Add(new Sign(74, 1, "A DOOR TO SOMEWHERE", biome == 2 ? "Press E at the arch to finish your journey. You can keep exploring afterward." : "Press E at the arch to discover the next world."));
            if (biome == 1)
            {
                w.Add(19, 9, 1, 5, Surface.Stone); w.Add(23, 10, 1, 5, Surface.Stone);
                w.Add(26, 15, 6); w.Pickups[0].Position = new V2(29, 16.5f);
                w.Add(60, 7, 3, .35f, Surface.Spring); w.Add(65, 12, 6);
                w.Signs[0] = new Sign(3, 1, "LIGHT UNDER THE LEAVES", "The grotto climbs higher. Try wall kicks and upward pounces to reach the lanterns.");
            }
            if (biome == 2)
            {
                w.Add(12, 7, 2, .35f, Surface.Spring); w.Add(17, 12, 4);
                w.Add(41, 12, 3, .6f, Surface.Moving, 2);
                w.Add(58, 14, 3, .35f, Surface.Spring); w.Add(66, 18, 6);
                w.Pickups[0].Position = new V2(68, 19.5f);
                w.Signs[0] = new Sign(3, 1, "THE WORLD OPENS UP", "Flowers, walls, and pounces can become one long leap. Find your own line through the sky.");
            }
            return w;
        }
    }
}
