namespace Wildbound.Core
{
    public sealed partial class WorldDefinition
    {
        private static void PickupsAt(WorldDefinition w, V2 memory, params V2[] motes)
        {
            w.Pickups.Add(new Pickup(memory.X, memory.Y, PickupKind.Memory));
            foreach (var p in motes) w.Pickups.Add(new Pickup(p.X, p.Y));
        }
        private static void Moonpath(WorldDefinition w, int bloom, float x, float y, float width)
        { w.Platforms.Add(new Platform(new Box(x, y, width, .35f), Surface.Moonbridge) { LightSource = bloom, Enabled = false }); }
        private static void ReturnPath(WorldDefinition w, WildPlaceId place, float x, float y, float width)
        { w.Platforms.Add(new Platform(new Box(x, y, width, .35f), Surface.Trailbridge) { DiscoverySource = (int)place, Enabled = false }); }

        private static void BuildCanopy(WorldDefinition w)
        {
            w.CameraMaxY = 18; w.MapBounds = new Box(-6, -3, 88, 24);
            w.Add(-5, -3, 40, 4); w.Add(38, -3, 43, 4);
            w.Add(12, 1.75f, 8, 1.2f); w.Add(9, 3, 3, .7f);
            w.Add(23, 4.2f, 4); w.Add(29, 7.1f, 5); w.Add(37, 10.1f, 6);
            w.Add(39, 1, 2, .35f, Surface.Spring); w.Add(43, 5.7f, 4, .7f);
            w.Add(53, 4, 4); w.Add(62, 3, 5, .7f); w.Add(69, 5.5f, 4, .7f);
            w.Enemies.Add(new Enemy(EnemyKind.Thornling, 27, 1, 1.2f));
            w.Enemies.Add(new Enemy(EnemyKind.Bristleback, 71, 1, 2));
            w.Enemies.Add(new Enemy(EnemyKind.ReedSpitter, 55, 5));
            w.Blooms.Add(new Moonbloom(21, 1.6f)); w.Blooms.Add(new Moonbloom(47.5f, 1.6f));
            Moonpath(w, 0, 26, 6.4f, 3); Moonpath(w, 1, 45, 8.8f, 4);
            ReturnPath(w, WildPlaceId.RootHollow, 35, 2.5f, 3);
            ReturnPath(w, WildPlaceId.AmberOverlook, 30, 10.3f, 7);
            w.Places.Add(new WildPlace(WildPlaceId.RootHollow, "ROOT HOLLOW",
                "Warm leaves. Old pawprints. A golden crossing now spans the forest gap.",
                "The tracks slip beneath the roots. Roll (L / B), then pad through the hollow.", "First Pawprints", new V2(16.5f, 1),
                new V2(10.8f, 1), new V2(12.8f, 1), new V2(14.5f, 1), new V2(16.5f, 1)));
            w.Places.Add(new WildPlace(WildPlaceId.AmberOverlook, "AMBER OVERLOOK",
                "The whole forest fits beneath her paws. A golden branch leads back toward shelter.",
                "The scent climbs the branches. Aim a charged pounce upward; land before the next leap.", "Amber Watch", new V2(41, 11.1f),
                new V2(19, 2.95f), new V2(24, 5.2f), new V2(31, 8.1f), new V2(39, 11.1f)));
            PickupsAt(w, new V2(41, 12.3f), new V2(7, 2.1f), new V2(10, 4.7f), new V2(16.5f, 1.45f),
                new V2(24, 6.3f), new V2(31, 9.2f), new V2(36.5f, 3.8f), new V2(40, 2.5f), new V2(45, 7.5f),
                new V2(46.5f, 10.2f), new V2(55, 6.1f), new V2(64, 4.8f), new V2(71, 7.3f));
            w.Signs.Add(new Sign(10.5f, 1, "SMALL PATHS, SOFT PAWS", "Faint pawprints lead under the roots. Hold Q / LT to see nearby scent. L / B rolls low; keep moving to crawl clear."));
            w.Signs.Add(new Sign(21, 1, "COIL INTO THE CANOPY", "Climb the shelves with short pounces. Pale lips mark the landings."));
            w.Signs.Add(new Sign(47, 1, "WAKE THE CROSSING", "Claw the blue flower to light a moonbridge. The lower trail stays open."));
        }

        private static void BuildGrotto(WorldDefinition w)
        {
            w.CameraMaxY = 24; w.MapBounds = new Box(-6, -3, 88, 30);
            w.Add(-5, -3, 45, 4); w.Add(42, -3, 39, 4);
            w.Add(12, 3.2f, 4); w.Add(22, 6.2f, 4); w.Add(14, 9.2f, 4);
            w.Add(24, 12.2f, 4); w.Add(33, 15.2f, 4); w.Add(24, 18.2f, 4); w.Add(35, 21.2f, 5);
            w.Add(48, 5, 5); w.Add(55, 6.5f, 4); w.Add(59, 9, 4); w.Add(66, 13, 5);
            w.Add(52, 1, 3, .9f);
            w.Enemies.Add(new Enemy(EnemyKind.Thornling, 30, 1, 1));
            w.Enemies.Add(new Enemy(EnemyKind.ReedSpitter, 58, 1));
            w.Enemies.Add(new Enemy(EnemyKind.LanternMoth, 48, 8, 1));
            w.Blooms.Add(new Moonbloom(20, 1.6f)); w.Blooms.Add(new Moonbloom(46.5f, 1.6f));
            Moonpath(w, 0, 26, 7.4f, 3); Moonpath(w, 1, 49, 11, 4);
            ReturnPath(w, WildPlaceId.StillwaterShelf, 10, 3.7f, 4);
            ReturnPath(w, WildPlaceId.LanternRoost, 38, 21.7f, 5);
            w.Places.Add(new WildPlace(WildPlaceId.StillwaterShelf, "STILLWATER SHELF",
                "Quiet stone above the pool. A golden step opens the lower return.",
                "The prints stop above the pool. Jump onto the low stone shelf.", "Still Water", new V2(14, 3.7f),
                new V2(8, 1), new V2(10, 1), new V2(12, 3.7f), new V2(14, 3.7f)));
            w.Places.Add(new WildPlace(WildPlaceId.LanternRoost, "LANTERN ROOST",
                "Sleeping lights gather in the stone crown. A golden shelf opens the eastern descent.",
                "Follow the shelves back and forth. Land, turn, and coil toward the next lantern.", "The Keeper's Lantern", new V2(37, 21.7f),
                new V2(23, 6.7f), new V2(15, 9.7f), new V2(25, 12.7f), new V2(34, 15.7f), new V2(25, 18.7f), new V2(37, 21.7f)));
            PickupsAt(w, new V2(37, 22.9f), new V2(7, 2.1f), new V2(14, 4.8f), new V2(22, 7.8f),
                new V2(15, 10.8f), new V2(25, 13.8f), new V2(34, 16.8f), new V2(25, 19.8f), new V2(44, 16.1f),
                new V2(51, 6.1f), new V2(56, 7.6f), new V2(60, 10.1f), new V2(68, 14.1f));
            w.Signs.Add(new Sign(9, 1, "LIGHT BELOW STONE", "Claw the blue flower to wake a crossing. The lower trail stays open; lantern shelves climb toward a hidden roost."));
            w.Signs.Add(new Sign(22, 6.7f, "TURN WITH THE TRAIL", "The next shelf is behind you. Face left, aim up, and release a charged pounce. Faint tracks mark the resting places."));
            w.Signs.Add(new Sign(47, 1, "A FLOWER AGAINST THE DIVE", "Claw the moonbloom as the moth marks its dive. The flare interrupts it and lights the crossing above."));
            w.Signs.Add(new Sign(53, 1, "STONE MAKES GOOD COVER", "Roll beneath the low arch, or climb over it. The spitter commits to three seeds; close while it recovers."));
        }

        private static void BuildSky(WorldDefinition w)
        {
            w.CameraMaxY = 26; w.MapBounds = new Box(-6, -3, 88, 34);
            w.Add(-5, -3, 21, 4); w.Add(20, -3, 8, 4); w.Add(32, -3, 11, 4);
            w.Add(46, -3, 7, 4); w.Add(57, -3, 24, 4);
            w.Add(10, 1, 2, .35f, Surface.Spring); w.Add(34, 1, 2, .35f, Surface.Spring);
            w.Add(60.5f, 1, 1.5f, .35f, Surface.Spring);
            // Mild wind ribbons (optional vertical routes). Not required for the main exit path.
            w.WindFields.Add(new WindField(18, 6, 14, 5, 2.2f, 0.4f, "sky-ribbon-east"));
            w.WindFields.Add(new WindField(40, 14, 16, 6, -1.5f, 0.6f, "sky-ribbon-west"));
            w.Add(15, 5, 5, .7f); w.Add(24, 9, 6, .7f); w.Add(35, 13, 6, .7f);
            w.Add(45, 17, 6, .7f); w.Add(51, 18, 4, .6f, Surface.Moving, 1.2f);
            w.Add(58, 21, 7, .7f); w.Add(66, 5, 5);
            w.Enemies.Add(new Enemy(EnemyKind.Thornling, 40, 1, 1));
            w.Enemies.Add(new Enemy(EnemyKind.LanternMoth, 42, 18, 1));
            w.Enemies.Add(new Enemy(EnemyKind.LanternMoth, 47.5f, 5.5f, 1));
            w.Blooms.Add(new Moonbloom(22, 1.6f)); w.Blooms.Add(new Moonbloom(46.5f, 1.6f));
            Moonpath(w, 0, 28, 10, 4); Moonpath(w, 1, 48, 12, 4);
            ReturnPath(w, WildPlaceId.CloudNest, 26, 9.7f, 4);
            ReturnPath(w, WildPlaceId.StarflowerCrown, 58, 21.7f, 6);
            w.Places.Add(new WildPlace(WildPlaceId.CloudNest, "CLOUD NEST",
                "Soft wind and quiet feathers. A golden perch opens the mid garden.",
                "The scent lifts into the first shelf. Pounce, land, and listen to the air.", "Cloud Nest", new V2(17, 5.7f),
                new V2(12, 1), new V2(15, 5.7f), new V2(17, 5.7f)));
            w.Places.Add(new WildPlace(WildPlaceId.StarflowerCrown, "STARFLOWER CROWN",
                "The highest garden holds a quiet crown. A golden path leads home.",
                "Follow the rising islands and the moving perch. The crown waits above.", "Starflower Crown", new V2(61, 21.7f),
                new V2(24, 9.7f), new V2(35, 13.7f), new V2(45, 17.7f), new V2(52, 18.6f), new V2(61, 21.7f)));
            PickupsAt(w, new V2(61, 22.9f), new V2(7, 2.1f), new V2(16, 6.8f), new V2(25, 10.8f),
                new V2(36, 14.8f), new V2(46, 18.8f), new V2(53, 19.5f), new V2(62, 22.5f), new V2(67, 6.8f),
                new V2(30, 2.1f), new V2(48, 2.1f), new V2(55, 7.5f), new V2(70, 2.1f));
            w.Signs.Add(new Sign(11, 1, "WIND IN THE LEAVES", "Upper shelves carry soft ribbons of air. Use them to extend a pounce; the ground path still leads east."));
            w.Signs.Add(new Sign(34, 1, "ISLANDS AND MOVING PERCHES", "Spring flowers refresh your pounce and dash. Follow the golden path; the crown waits above the moving perch."));
            w.Signs.Add(new Sign(47, 1, "LIGHT BELOW THE WINGS", "Claw the blue flower to interrupt the moth. The lower islands offer another route across the garden."));
        }
    }
}
