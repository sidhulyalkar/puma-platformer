using System;
using System.Collections.Generic;

namespace Wildbound.Core
{
    // Persistent IDs: never renumber these when reordering or moving places.
    public enum WildPlaceId { RootHollow = 0, AmberOverlook = 1, StillwaterShelf = 2, LanternRoost = 3, CloudNest = 4, StarflowerCrown = 5, CharcoilDen = 6, QuietFireRidge = 7 }

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

        public bool Reached(PumaMotor player)
        {
            if (Found) return false;
            if ((player.Position - Position).Length > 1.4f) return false;
            if (Id == WildPlaceId.RootHollow && !player.LowProfile) return false;
            return true;
        }

        public void OpenPath(WorldDefinition world)
        {
            Found = true;
            foreach (var p in world.Platforms)
                if (p.DiscoverySource == (int)Id) p.Enabled = true;
        }

        public static bool ScentVisible(WorldDefinition world, PumaMotor puma, V2 track)
        {
            return puma.Stalking
                && (track - puma.Position).Length < 7.5f
                && WorldCollision.ClearLine(world, puma.Bounds.Center, track + new V2(0, .2f));
        }

        public MemoryVignette ToVignette(int biome)
        {
            return new MemoryVignette(string.IsNullOrEmpty(MemoryTitle) ? Name : MemoryTitle, Story, Hint, biome);
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
            new MemoryDescriptor(2, "Starflower Crown", "Acceptance / agency"),
            new MemoryDescriptor(3, "Quiet Fire", "Endurance / warmth")
        };

        public static MemoryDescriptor ForBiome(int biome)
        {
            if (biome < 0 || biome >= All.Length) return All[0];
            return All[biome];
        }
    }
}
