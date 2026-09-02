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
            // The hollow is a real low passage, with a standing approach at both ends.
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
                "The tracks slip beneath the roots. Roll (L / B), then pad through the hollow.", new V2(16.5f, 1),
                new V2(10.8f, 1), new V2(12.8f, 1), new V2(14.5f, 1), new V2(16.5f, 1)));
            w.Places.Add(new WildPlace(WildPlaceId.AmberOverlook, "AMBER OVERLOOK",
                "The whole forest fits beneath her paws. A golden branch leads back toward shelter.",
                "The scent climbs the branches. Aim a charged pounce upward; land before the next leap.", new V2(41, 11.1f),
                new V2(19, 2.95f), new V2(24, 5.2f), new V2(31, 8.1f), new V2(39, 11.1f)));
            PickupsAt(w, new V2(41, 12.3f), new V2(7, 2.1f), new V2(10, 4.7f), new V2(16.5f, 1.45f),
                new V2(24, 6.3f), new V2(31, 9.2f), new V2(36.5f, 3.8f), new V2(40, 2.5f), new V2(45, 7.5f),
                new V2(46.5f, 10.2f), new V2(55, 6.1f), new V2(64, 4.8f), new V2(71, 7.3f));
            w.Signs.Add(new Sign(10.5f, 1, "SMALL PATHS, SOFT PAWS", "Faint pawprints lead under the roots. Hold Q / LT to see nearby scent. L / B rolls low; keep moving to crawl clear."));
            w.Signs.Add(new Sign(21, 1, "COIL INTO THE CANOPY", "Climb onto the roots from either end. Hold SHIFT / X, aim up, then release toward the branches. Claw blue flowers to wake crossings."));
            w.Signs.Add(new Sign(39, 1, "BORROW A LITTLE SPRING", "A pink flower restores your pounce in midair. Follow the upper branches back toward the memory."));
            w.Signs.Add(new Sign(68, 1, "LET HIM COMMIT", "The bristleback guards the clearing. Vault his charge, roll through at the right moment, or take the branches above."));
        }

        private static void BuildGrotto(WorldDefinition w)
        {
            w.CameraMaxY = 26; w.MapBounds = new Box(-6, -3, 88, 34);
            w.Add(-5, -3, 86, 4); w.Add(-5, 29, 86, 2, Surface.Stone);
            // Switchback shelves form one vertical landmark above a safe lower trail.
            w.Add(11, 3, 5, .7f, Surface.Stone); w.Add(20, 6, 5, .7f, Surface.Stone);
            w.Add(12, 9, 6, .7f, Surface.Stone); w.Add(22, 12, 6, .7f, Surface.Stone);
            w.Add(31, 15, 6, .7f, Surface.Stone); w.Add(23, 18, 5, .7f, Surface.Stone);
            w.Add(33, 21, 7, .7f, Surface.Stone);
            w.Add(49, 4, 5); w.Add(58, 8, 4); w.Add(64, 12, 7);
            w.Add(18, 1, 1, 4, Surface.Stone); w.Add(29, 6, 1, 6, Surface.Stone);
            w.Add(54, 1.75f, 3, 1.1f, Surface.Stone);
            w.Hazards.Add(new Box(39, 1, 2, .4f));
            w.Enemies.Add(new Enemy(EnemyKind.LanternMoth, 20, 11, 1));
            w.Enemies.Add(new Enemy(EnemyKind.LanternMoth, 47.5f, 5.5f, 1));
            w.Enemies.Add(new Enemy(EnemyKind.ReedSpitter, 63, 1));
            w.Blooms.Add(new Moonbloom(9.5f, 1.6f)); w.Blooms.Add(new Moonbloom(47.5f, 1.6f));
            Moonpath(w, 0, 16, 7.5f, 4); Moonpath(w, 1, 54, 6.2f, 4);
            ReturnPath(w, WildPlaceId.StillwaterShelf, 17, 5.3f, 3);
            ReturnPath(w, WildPlaceId.LanternRoost, 40, 15, 7);
            w.Places.Add(new WildPlace(WildPlaceId.StillwaterShelf, "STILLWATER SHELF",
                "A quiet pool reflects a second sky. Golden stepping stones curl around the pillar.",
                "The prints stop above the pool. Jump onto the low stone shelf.", new V2(14, 3.7f),
                new V2(8, 1), new V2(10, 1), new V2(12, 3.7f), new V2(14, 3.7f)));
            w.Places.Add(new WildPlace(WildPlaceId.LanternRoost, "LANTERN ROOST",
                "Sleeping lights gather in the stone crown. A golden shelf opens the eastern descent.",
                "Follow the shelves back and forth. Land, turn, and coil toward the next lantern.", new V2(37, 21.7f),
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
            w.Add(15, 5, 5, .7f); w.Add(24, 9, 6, .7f); w.Add(35, 13, 6, .7f);
            w.Add(45, 17, 6, .7f); w.Add(51, 18, 4, .6f, Surface.Moving, 1.2f);
            w.Add(58, 21, 7, .7f); w.Add(66, 5, 5);
            w.Enemies.Add(new Enemy(EnemyKind.Thornling, 40, 1, 1));
            w.Enemies.Add(new Enemy(EnemyKind.LanternMoth, 42, 18, 1));
            w.Enemies.Add(new Enemy(EnemyKind.LanternMoth, 47.5f, 5.5f, 1));
            w.Enemies.Add(new Enemy(EnemyKind.ReedSpitter, 68, 6));
            w.Blooms.Add(new Moonbloom(14, 1.6f)); w.Blooms.Add(new Moonbloom(47.5f, 1.6f));
            Moonpath(w, 0, 18, 3.5f, 3); Moonpath(w, 1, 42, 12, 4);
            ReturnPath(w, WildPlaceId.CloudNest, 30, 11.5f, 5);
            ReturnPath(w, WildPlaceId.StarflowerCrown, 64, 18.5f, 5);
            w.Places.Add(new WildPlace(WildPlaceId.CloudNest, "CLOUD NEST",
                "Feathers caught in a garden above the clouds. A golden path reaches the next island.",
                "A spring starts the climb. Spend its fresh pounce on the wide island above.", new V2(26, 9.7f),
                new V2(9, 1), new V2(17, 5.7f), new V2(26, 9.7f)));
            w.Places.Add(new WildPlace(WildPlaceId.StarflowerCrown, "STARFLOWER CROWN",
                "One small cat beneath a thousand stars. Golden petals lead down toward home.",
                "The scent crosses the high islands. Settle on a perch before coiling for the crown.", new V2(61, 21.7f),
                new V2(38, 13.7f), new V2(48, 17.7f), new V2(61, 21.7f)));
            PickupsAt(w, new V2(61, 22.9f), new V2(7, 2.1f), new V2(11, 2.5f), new V2(17, 6.8f),
                new V2(26, 10.8f), new V2(32, 12.8f), new V2(38, 14.8f), new V2(48, 18.8f), new V2(53, 19.7f),
                new V2(67, 19.8f), new V2(60, 2.1f), new V2(68, 7.1f), new V2(73, 2.1f));
            w.Signs.Add(new Sign(9, 1, "BORROW THE SKY", "Jump onto a pink flower to launch and restore your pounce. Hold SHIFT / X in the air, aim up, and release toward the broad island."));
            w.Signs.Add(new Sign(26, 9.7f, "A NEST BETWEEN LEAPS", "Land to refresh your pounce and dash. Follow the golden path; the crown waits above the moving perch."));
            w.Signs.Add(new Sign(47, 1, "LIGHT BELOW THE WINGS", "Claw the blue flower to interrupt the moth. The lower islands offer another route across the garden."));
        }
    }
}
