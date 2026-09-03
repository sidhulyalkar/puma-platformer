using System;

namespace Wildbound.Core
{
    // Persistent IDs: never renumber these when reordering or moving places.
    public enum WildPlaceId { RootHollow = 0, AmberOverlook = 1, StillwaterShelf = 2, LanternRoost = 3, CloudNest = 4, StarflowerCrown = 5 }

    /// <summary>
    /// A discoverable wild place. Stories and MemoryTitle feed the vignette system (docs/STORY.md).
    /// </summary>
    public sealed class WildPlace
    {
        public readonly WildPlaceId Id;
        public readonly string Name, Story, Hint;
        public readonly V2 Position;
        public readonly V2[] Tracks;
        public bool Found;
        public int Mask { get { return 1 << (int)Id; } }
        public readonly string MemoryTitle;

        public WildPlace(WildPlaceId id, string name, string story, string hint, V2 position, params V2[] tracks)
            : this(id, name, story, hint, "", position, tracks) { }

        public WildPlace(WildPlaceId id, string name, string story, string hint, string memoryTitle, V2 position, params V2[] tracks)
        {
            Id = id;
            Name = name;
            Story = story;
            Hint = hint;
            MemoryTitle = memoryTitle ?? "";
            Position = position;
            Tracks = tracks ?? Array.Empty<V2>();
        }

        public bool Reached(PumaMotor puma)
        {
            return puma.Grounded
                && Math.Abs(puma.Position.X - Position.X) < 1.1f
                && Math.Abs(puma.Position.Y - Position.Y) < .25f;
        }

        public static bool ScentVisible(WorldDefinition world, PumaMotor puma, V2 track)
        {
            return puma.Stalking
                && (track - puma.Position).Length < 8
                && WorldCollision.ClearLine(world, puma.Bounds.Center, track + new V2(0, .18f));
        }

        public void OpenPath(WorldDefinition world)
        {
            Found = true;
            foreach (var platform in world.Platforms)
                if (platform.DiscoverySource == (int)Id) platform.Enabled = true;
        }

        public MemoryVignette ToVignette(int biome)
        {
            string title = !string.IsNullOrEmpty(MemoryTitle) ? MemoryTitle : Name;
            string beat = biome == 0 ? "Belonging / loss" : biome == 1 ? "Responsibility" : "Acceptance / agency";
            return new MemoryVignette(title, Story, beat, biome);
        }
    }

    public sealed class MemoryDescriptor
    {
        public readonly int Biome;
        public readonly string Title;
        public readonly string Beat;

        public MemoryDescriptor(int biome, string title, string beat)
        {
            Biome = biome;
            Title = title;
            Beat = beat;
        }

        public static readonly MemoryDescriptor[] All =
        {
            new MemoryDescriptor(0, "First Pawprints", "Belonging / loss"),
            new MemoryDescriptor(1, "The Keeper's Lantern", "Responsibility"),
            new MemoryDescriptor(2, "Starflower Crown", "Acceptance / agency")
        };

        public static MemoryDescriptor ForBiome(int biome)
        {
            if (biome < 0 || biome >= All.Length) return All[0];
            return All[biome];
        }
    }
}
