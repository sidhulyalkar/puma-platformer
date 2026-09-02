using System;

namespace Wildbound.Core
{
    // Persistent IDs: never renumber these when reordering or moving places.
    public enum WildPlaceId { RootHollow = 0, AmberOverlook = 1, StillwaterShelf = 2, LanternRoost = 3, CloudNest = 4, StarflowerCrown = 5 }

    public sealed class WildPlace
    {
        public readonly WildPlaceId Id;
        public readonly string Name, Story, Hint;
        public readonly V2 Position;
        public readonly V2[] Tracks;
        public bool Found;
        public int Mask { get { return 1 << (int)Id; } }
        public WildPlace(WildPlaceId id, string name, string story, string hint, V2 position, params V2[] tracks)
        { Id = id; Name = name; Story = story; Hint = hint; Position = position; Tracks = tracks; }

        public bool Reached(PumaMotor puma)
        { return puma.Grounded && Math.Abs(puma.Position.X - Position.X) < 1.1f && Math.Abs(puma.Position.Y - Position.Y) < .25f; }

        public static bool ScentVisible(WorldDefinition world, PumaMotor puma, V2 track)
        {
            return puma.Stalking && (track - puma.Position).Length < 8
                && WorldCollision.ClearLine(world, puma.Bounds.Center, track + new V2(0, .18f));
        }

        public void OpenPath(WorldDefinition world)
        {
            Found = true;
            foreach (var platform in world.Platforms)
                if (platform.DiscoverySource == (int)Id) platform.Enabled = true;
        }
    }
}
