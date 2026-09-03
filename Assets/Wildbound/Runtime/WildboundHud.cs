using UnityEngine;
using Wildbound.Core;

namespace Wildbound.Unity
{
    public sealed class WildboundHud : MonoBehaviour
    {
        private WildboundGame game;
        private GUIStyle text, title, eyebrow, button;
        private int guidePage;
        private int mapPage;
        private readonly Color ink = new Color(.055f, .12f, .15f, .94f);
        private readonly Color paper = new Color(1, .94f, .82f);
        private readonly Color muted = new Color(.65f, .79f, .76f);
        public void Initialize(WildboundGame owner) { game = owner; }
        private void Styles()
        {
            if (text != null) return;
            text = new GUIStyle(GUI.skin.label) { fontSize = 19, wordWrap = true, richText = false, padding = new RectOffset(0, 0, 0, 0) };
            text.normal.textColor = paper;
            title = new GUIStyle(text) { fontSize = 68, fontStyle = FontStyle.Bold };
            eyebrow = new GUIStyle(text) { fontSize = 14 }; eyebrow.normal.textColor = muted;
            button = new GUIStyle(GUI.skin.button) { fontSize = 18, padding = new RectOffset(16, 16, 10, 10) };
            button.normal.textColor = paper; button.hover.textColor = Color.white;
        }
        private void OnGUI()
        {
            if (game == null) return;
            Styles();
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            Matrix4x4 previous = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 720 * scale) / 2, 0), Quaternion.identity, Vector3.one * scale);
            if (game.ShowResetConfirmation) ResetConfirmation();
            else if (game.ShowControls) { if (game.Playing) HUD(); Controls(); }
            else if (!game.Playing) Title();
            else if (game.ShowEnding) Ending();
            else
            {
                HUD();
                if (game.ShowMap) Map();
                else if (game.Session.Paused) Pause();
            }
            if (game.ToastTime > 0 && !game.ShowControls && !game.ShowMap && !game.ShowResetConfirmation)
            {
                Panel(325, 94, 565, 76, ink); Label(345, 109, 525, 57, game.Toast, new GUIStyle(text) { fontSize = 17 });
            }
            GUI.matrix = previous;
        }
        private void Title()
        {
            Panel(0, 0, 590, 720, new Color(ink.r, ink.g, ink.b, .88f));
            Label(65, 108, 490, 25, "P U M A   /   T H E   N I G H T   I S   H E R S", eyebrow);
            Label(60, 153, 560, 95, "WILDBOUND", title);
            Label(65, 267, 430, 80, "Quiet paws.\nA wild heart after dark.", new GUIStyle(text) { fontSize = 29 });
            Label(65, 370, 405, 80, "Follow scents through three night worlds. Discover quiet hollows, high roosts, and paths that remember your paws.", text);
            if (Button(65, 488, 280, 54, game.Session.Save.Biome > 0 || game.Session.Save.Collected[0] != 0 || game.Session.WaystoneCount > 0 || game.Session.DiscoveryCount > 0 ? "Continue your trail  /  ENTER" : "Begin exploring  /  ENTER")) game.Begin();
            if (Button(65, 557, 190, 43, "Field guide  /  C")) game.ToggleControls();
            Label(65, 656, 465, 26, "A SID HULYALKAR GAME   /   DEVELOPMENT SLICE", eyebrow);
        }
        private void HUD()
        {
            Panel(24, 23, 415, 57, ink);
            Label(42, 32, 385, 23, game.Session.World.Name, eyebrow);
            Label(42, 54, 365, 22, game.Session.InTrial ? "WAYSTONE TRIAL  /  TAB journal" : "Follow tracks. Find the arch.", new GUIStyle(eyebrow) { fontSize = 12 });
            Panel(863, 23, 393, 57, ink);
            Label(884, 41, 360, 29, game.Session.InTrial ? "WAYSTONES  " + game.Session.WaystoneCount + " / 3" : "LIGHT  " + game.Session.Motes + " / 12     MEMORIES  " + game.Session.Memories + " / 3", text);
            Panel(455, 23, 390, 57, ink);
            Label(472, 31, 368, 24, "VITALITY  " + game.Session.Combat.Health + " / 5     INSTINCT  " + game.Session.Combat.Instinct + " / 3", eyebrow);
            Label(472, 55, 368, 22, game.Session.Combat.Busy ? game.Session.Combat.MoveName : "HUNTS  " + game.Session.Combat.Hunts + "   /   THE NIGHT IS HERS", new GUIStyle(eyebrow) { fontSize = 12 });
            Label(28, 684, 1020, 23, "SPACE jump   SHIFT pounce   J claw   K dash-claw   L roll   Q stalk   E explore   TAB map   C guide", new GUIStyle(eyebrow) { fontSize = 12 });
            var p = game.Session.Player;
            if (!game.Session.Paused)
            {
                CreatureHint();
                ObjectiveHint();
                TrailHint();
                if (p.Charging)
                {
                    Panel(490, 500, 300, 50, ink); Label(505, 507, 270, 23, p.Charge >= 1 ? "FULL COIL  /  RELEASE SHIFT" : "COILING  /  RELEASE TO POUNCE", eyebrow);
                    Panel(505, 535, 270, 5, new Color(.22f, .36f, .37f)); Panel(505, 535, 270 * p.Charge, 5, paper);
                }
                else Label(1050, 684, 205, 23, p.Stalking ? "SCENT SIGHT" : p.PounceReady ? "POUNCE READY" : "LAND TO RECHARGE", eyebrow);
            }
        }
        private void TrailHint()
        {
            if (game.Session.Recovery > 0) return;
            var practice = game.Session.Practice;
            var lesson = practice.NoticeSeconds > 0 && !game.Session.InTrial ? practice.Recent : practice.NearbyLesson(game.Session);
            Sign sign = game.Session.NearbySign();
            if (game.Session.Player.LowProfile && game.Session.Player.RollTime <= 0)
            {
                Panel(320, 566, 640, 99, ink); Label(341, 581, 598, 24, "KEEP YOUR HEAD LOW", eyebrow);
                Label(341, 610, 598, 53, "Keep moving with A / D or the stick until you clear the roots. Stand tall before coiling or clawing.", new GUIStyle(text) { fontSize = 17 });
            }
            else if (lesson != null)
            {
                bool confirmed = lesson == practice.Recent && practice.NoticeSeconds > 0;
                Panel(320, 566, 640, 99, ink);
                Label(341, 581, 598, 24, (confirmed ? "PRACTICED  /  " : "TRY  /  ") + lesson.Name, eyebrow);
                Label(341, 610, 598, 53, confirmed ? lesson.Feedback : lesson.Instruction, new GUIStyle(text) { fontSize = 17 });
            }
            else if (sign != null)
            {
                Panel(320, 566, 640, 99, ink); Label(341, 581, 598, 24, sign.Heading, eyebrow);
                Label(341, 610, 598, 53, sign.Text, text);
            }
            else
            {
                var trail = game.Session.NearbyTrail();
                if (trail != null)
                {
                    Panel(320, 566, 640, 99, ink); Label(341, 581, 598, 24, "A SCENT ON THE NIGHT AIR", eyebrow);
                    Label(341, 610, 598, 53, trail.Hint, new GUIStyle(text) { fontSize = 17 });
                }
            }
        }
        private void Controls()
        {
            Panel(230, 75, 820, 590, ink);
            Label(277, 106, 520, 40, guidePage == 2 ? "MOONTRAIL FIELD NOTES" : guidePage == 1 ? "FIRST PAWS" : "THE FIELD GUIDE", new GUIStyle(title) { fontSize = 31 });
            if (Button(810, 105, 194, 38, guidePage == 0 ? "Practice notes" : guidePage == 1 ? "Trial strategy" : "Move controls")) guidePage = (guidePage + 1) % 3;
            if (guidePage == 2) TrialGuide();
            else if (guidePage == 1) PracticeNotes();
            else
            {
                string[] keys = { "A / D or arrows", "SPACE  /  gamepad A", "SHIFT  /  gamepad X", "J / click  /  RB", "K  /  gamepad RT", "L  /  gamepad B", "Hold Q  /  gamepad LT", "E  /  gamepad Y", "TAB   /   C   /   ESC", "R   /   M" };
                string[] actions = { "Roam. W/S or the stick aims a pounce.", "Jump; hold for height. Jump against a wall to kick.", "Hold to coil, release to pounce. Land to recharge.", "Claw chain. W+J rises; airborne S+J rakes down.", "Dash-claw. Three defeats empower the next rush.", "Ground roll. Dodge in the middle; beware recovery.", "Stalk prey, reveal local tracks, and balance on perches.", "Use an arch, enter a trial, or restore a waystone.", "Map / journal, field guide, pause. Start also pauses.", "Return to checkpoint / toggle sound." };
                for (int i = 0; i < keys.Length; i++)
                {
                    Label(278, 163 + i * 37, 245, 37, keys[i], eyebrow);
                    Label(536, 159 + i * 37, 465, 39, actions[i], new GUIStyle(text) { fontSize = 17 });
                }
                Label(277, 544, 728, 48, "A crescent stone near each region's start leads to a trial. Restore its waystone to keep that region's light bridges awake. Trial strategy explains the combinations.", eyebrow);
            }
            if (Button(277, 606, 205, 40, "Back  /  C")) game.ToggleControls();
            if (Button(500, 606, 205, 40, game.Muted ? "Sound: off" : "Sound: on")) game.ToggleMute();
            if (Button(723, 606, 280, 40, game.ReducedMotion ? "Motion: reduced" : "Motion: full")) game.ToggleMotion();
        }
        private void Pause()
        {
            Panel(400, 157, 480, 417, ink);
            Label(442, 196, 400, 50, "A MOMENT TO BREATHE", new GUIStyle(title) { fontSize = 29 });
            if (Button(444, 273, 392, 48, "Keep exploring")) game.Resume();
            if (Button(444, 335, 392, 48, "Field guide")) game.ToggleControls();
            if (Button(444, 397, 392, 48, "New journey")) game.NewJourney();
            if (Button(444, 460, 185, 42, game.Muted ? "Sound: off" : "Sound: on")) game.ToggleMute();
            if (Button(642, 460, 194, 42, game.ReducedMotion ? "Less motion" : "Full motion")) game.ToggleMotion();
            Label(444, 526, 392, 24, "Your trail is saved on this device.", eyebrow);
        }
        private void Map()
        {
            if (game.Session.InTrial && mapPage == 2) mapPage = 1;
            Panel(150, 95, 980, 530, ink);
            Label(196, 126, 655, 40, mapPage == 1 ? "THE MOONTRAIL JOURNAL" : mapPage == 2 ? "WILD PLACES" : "YOUR TRAIL THROUGH THE NIGHT", new GUIStyle(title) { fontSize = 29 });
            if (Button(870, 124, 215, 38, mapPage == 0 ? "Show objectives" : mapPage == 1 && !game.Session.InTrial ? "Show wild places" : "Show map")) mapPage = (mapPage + 1) % (game.Session.InTrial ? 2 : 3);
            Label(196, 184, 888, 32, game.Session.World.Name + "   /   You are the amber mark", eyebrow);
            if (mapPage == 1) Journal();
            else if (mapPage == 2) PlacesJournal();
            else
            {
                foreach (var p in game.Session.World.Platforms)
                {
                    if (p.Surface == Surface.RootGate && !p.Enabled) continue;
                    var b = p.Bounds; if (b.H > 10 && p.Surface != Surface.RootGate) continue;
                    Vector2 mark = OnMap(new V2(b.X, b.Top));
                    Color color = p.Surface == Surface.Trailbridge ? new Color(1, .8f, .4f, p.Enabled ? 1 : .2f) : p.Enabled ? new Color(.4f, .65f, .64f) : new Color(.3f, .45f, .55f, .2f);
                    Panel(mark.x, mark.y, b.W * MapScale, Mathf.Max(2, Mathf.Min(20, b.H * MapScale)), color);
                }
                var pos = game.Session.Player.Position;
                MapMark(pos, 10, new Color(1, .73f, .4f));
                foreach (var checkpoint in game.Session.World.Checkpoints) MapMark(checkpoint, 7, paper);
                foreach (var place in game.Session.World.Places) if (place.Found) MapMark(place.Position, 8, new Color(1, .86f, .5f));
                V2 objective = PracticeGuide.ObjectivePosition(game.Session);
                MapMark(objective, 10, new Color(.7f, .8f, 1));
                if (!game.Session.InTrial) MapMark(Moontrial.Entrance, 6, new Color(.7f, .8f, 1));
                Label(196, 510, 888, 42, game.Session.InTrial ? "Violet mark: next mechanism or sanctuary. The crescent at the start returns to your trail." : "Large violet mark: arch onward. Small violet mark: optional waystone trial. Discoveries and trials never block the main trail.", new GUIStyle(text) { fontSize = 17 });
            }
            string[] names = { "Canopy", "Grotto", "Sky garden" };
            if (game.Session.InTrial)
            {
                if (Button(785, 569, 300, 37, "Leave trial")) game.LeaveTrial();
            }
            else for (int i = 0; i <= game.Session.Save.FurthestBiome; i++)
                    if (Button(470 + i * 195, 569, 180, 37, names[i])) game.TravelTo(i);
            if (Button(196, 569, 250, 37, "Back  /  TAB")) game.ToggleMap();
        }
        private float MapScale { get { Box b = game.Session.World.MapBounds; return Mathf.Min(860 / b.W, 268 / b.H); } }
        private Vector2 OnMap(V2 position)
        {
            Box b = game.Session.World.MapBounds; float scale = MapScale;
            return new Vector2(210 + (860 - b.W * scale) / 2 + (position.X - b.X) * scale, 493 - (position.Y - b.Y) * scale);
        }
        private void MapMark(V2 point, float size, Color color)
        { Vector2 p = OnMap(point); Panel(p.x - size / 2, p.y - size, size, size, color); }
        private void PlacesJournal()
        {
            var s = game.Session;
            for (int i = 0; i < s.World.Places.Count; i++)
            {
                var place = s.World.Places[i];
                JournalRow(i, place.Found ? place.Name : "A TRAIL TO FOLLOW", place.Found ? place.Story : place.Hint, place.Found);
            }
            Label(196, 429, 888, 55, "WILD PLACES  " + s.DiscoveryCount + " / 6     MEMORIES  " + s.Memories + " / 3\nReach a resting place on your paws to discover it. Its golden return path stays open.", new GUIStyle(text) { fontSize = 18 });
            Label(196, 513, 888, 41, "Q / LT brings nearby tracks into focus. Discovery survives falls and return visits. Shelter stars remember the memories you found.", new GUIStyle(text) { fontSize = 17 });
        }
        private void Ending()
        {
            Panel(287, 122, 706, 475, ink);
            Label(339, 159, 610, 30, "THREE WORLDS. ONE VERY CURIOUS CAT.", eyebrow);
            Label(336, 207, 610, 95, "Still wild at heart.", new GUIStyle(title) { fontSize = 49 });
            Label(340, 309, 600, 75, "You followed the trail from forest floor to open sky. There are always little things left to discover.", text);
            Label(340, 397, 600, 54, "Memories: " + game.Session.Memories + " / 3    Waystones: " + game.Session.WaystoneCount + " / 3\n" + (game.Session.WaystoneCount == 3 ? "The whole moontrail shines again." : "The waystones still have stories for your paws."), text);
            if (Button(340, 471, 280, 49, "Keep exploring")) game.Resume();
            if (Button(640, 471, 296, 49, "Start a new journey")) game.NewJourney();
            Label(340, 552, 600, 24, "Thank you for taking the scenic route.", eyebrow);
        }
        private void ResetConfirmation()
        {
            Panel(287, 180, 706, 354, ink);
            Label(333, 218, 615, 49, "START A NEW JOURNEY?", new GUIStyle(title) { fontSize = 34 });
            Label(333, 283, 615, 105, "This replaces your saved trail on this device. You will return to the Canopy with no collected light, memories, wild places, waystones, or practice notes.", text);
            if (Button(333, 429, 295, 52, "Keep my trail  /  ESC")) game.TogglePause();
            if (Button(652, 429, 295, 52, "Start fresh")) game.ConfirmNewJourney();
        }
        private void CreatureHint()
        {
            Enemy closest = null; float range = 5.5f;
            foreach (var enemy in game.Session.World.Enemies)
            {
                float distance = (enemy.Position - game.Session.Player.Position).Length;
                if (enemy.Alive && enemy.Kind != EnemyKind.ClawPost && distance < range) { range = distance; closest = enemy; }
            }
            if (closest == null) return;
            string[] tips = { "Practice linking three claws.", "Approach quietly. Catch the landing.", "Curl, leap, rest. Find your moment.", "Guarded front. Claw from above or behind.", "Three seeds, then an opening.", "The glowing line shows her dive." };
            string state = closest.Phase == EnemyPhase.Tell ? "WARNING" : closest.Phase == EnemyPhase.Recover ? "OPENING" : closest.Phase == EnemyPhase.Stunned ? "STAGGERED" : closest.Phase == EnemyPhase.Active ? "COMMITTED" : "WATCHING";
            Panel(24, 100, 284, 96, ink);
            Label(39, 112, 255, 25, closest.Name.ToUpperInvariant() + "  /  " + state, new GUIStyle(eyebrow) { fontSize = 12 });
            Label(39, 143, 255, 46, tips[(int)closest.Kind], new GUIStyle(text) { fontSize = 16 });
        }
        private void ObjectiveHint()
        {
            var s = game.Session; var trial = s.World.Trial;
            Panel(913, 100, 343, 158, ink);
            string heading = trial == null ? "THE MAIN TRAIL" : "MECHANISMS  " + trial.FinishedGoals(s.World) + " / " + trial.GoalCount;
            Label(930, 114, 309, 24, heading, eyebrow);
            string hint = trial != null ? trial.NextGoal(s.World) : (s.Player.Position - Moontrial.Entrance).Length < 1.6f
                ? "E / Y: optional waystone trial. The main trail continues toward the arch."
                : s.Save.Biome == 2 ? "Find the last arch. E / Y finishes the journey; you can keep exploring afterward."
                : "Follow the trail to the arch. E / Y opens the next region. Tracks and waystones offer optional detours.";
            Label(930, 147, 307, 79, hint, new GUIStyle(text) { fontSize = 16 });
            V2 target = PracticeGuide.ObjectivePosition(s);
            V2 delta = target - s.Player.Position;
            Label(930, 230, 309, 22, "NEXT  " + Mathf.RoundToInt(delta.Length) + "m  " + (Mathf.Abs(delta.X) < 1 ? "HERE" : delta.X > 0 ? "RIGHT" : "LEFT") + (delta.Y > 2 ? " / ABOVE" : "") + "    TAB: journal", new GUIStyle(eyebrow) { fontSize = 12 });
            if (trial == null || trial.Balance == null || trial.Balance.Attuned || s.Player.Charging || s.Player.GroundIndex != trial.Balance.PlatformIndex) return;
            float progress = trial.Balance.Charge / BalancePerch.SettleSeconds;
            Panel(455, 480, 370, 70, ink);
            Label(471, 491, 340, 25, "Q / LT + STEER   /   STEADY " + Mathf.RoundToInt(progress * 100) + "%", eyebrow);
            Panel(471, 529, 338, 7, new Color(.22f, .36f, .37f)); Panel(471, 529, 338 * progress, 7, paper);
        }
        private void TrialGuide()
        {
            string[] headings = { "STEADY PAWS", "TURN A STRIKE INTO A PLATFORM", "COMMIT, THEN CHOOSE A ROUTE" };
            string[] notes = {
                "Jump or pounce onto the swaying perch. Hold Q / LT and tap against the wind ribbon to stay inside its ring. Fill the lights before charging your next leap.",
                "Get above a violet moonbell and use down + claw. Only a real hit rebounds and restores pounce / air dash. As the rake enters recovery, coil or dash toward the next ledge.",
                "K / RT breaks braided roots. A ground roll fits under the grotto's low arch; the upper ledge offers another way past the spitter. Watch the guardian's charge before crossing." };
            for (int i = 0; i < 3; i++)
            {
                Label(277, 167 + i * 117, 728, 26, headings[i], eyebrow);
                Label(277, 199 + i * 117, 728, 79, notes[i], new GUIStyle(text) { fontSize = 18 });
            }
            Label(277, 535, 728, 60, "Each restored waystone permanently lights its region's bridges. Falls keep lit mechanisms; leaving or reloading resets an unfinished trial. E / Y at the start or Leave trial in the map returns you safely.", eyebrow);
        }
        private void PracticeNotes()
        {
            int row = 0;
            foreach (var lesson in PracticeGuide.Lessons)
            {
                bool tried = PracticeGuide.Has(game.Session.Save, lesson.Skill);
                Label(277, 161 + row * 43, 235, 40, (tried ? "TRIED  /  " : "TRY  /  ") + lesson.Name, new GUIStyle(eyebrow) { fontSize = 12 });
                Label(525, 158 + row * 43, 480, 43, lesson.Instruction, new GUIStyle(text) { fontSize = 15 });
                row++;
            }
            Label(277, 558, 728, 41, "These notes remember actions you have tried. They are optional, can be practiced in any order, and never lock a path or ability.", new GUIStyle(text) { fontSize = 16 });
        }
        private void Journal()
        {
            var s = game.Session;
            if (s.InTrial)
            {
                var t = s.World.Trial; int row = 0;
                if (t.BloomIndex >= 0) JournalRow(row++, "WAKE THE CROSSING", "Claw the blue moonbloom; its bridge stays solid after the pulse.", s.World.Blooms[t.BloomIndex].Awakened);
                if (t.Balance != null) JournalRow(row++, "SETTLE THE LANTERN", "Reach the moving perch. Hold Q / LT and steer within its center ring.", t.Balance.Attuned);
                if (t.Bell != null) JournalRow(row++, "RING THE MOONBELL", "Rake downward from above; use the rebound to continue across.", t.Bell.Rung);
                if (t.Gate != null) JournalRow(row++, "OPEN THE ROOT GATE", "Dash-claw into the braided roots, then read the guardian beyond.", t.Gate.Broken);
                Label(196, 513, 888, 41, t.Ready(s.World) ? "The far waystone is ready. Reach it and press E / Y to restore the region's light." : "Lit mechanisms survive a fall. Finish them, then reach the far waystone. C: trial strategy.", new GUIStyle(text) { fontSize = 17 });
            }
            else
            {
                string[] descriptions = { "Canopy: wake a crossing, then balance in the wind before the next pounce.", "Grotto: wake a crossing, climb, and rake a bell to rebound past the patrol.", "Sky: balance, rebound, and dash through roots to reach the guardian's far side." };
                for (int i = 0; i < 3; i++) JournalRow(i, Moontrial.Title(i), i > s.Save.FurthestBiome ? "Discover this region through the next arch first." : descriptions[i], s.WaystoneRestored(i));
                Label(196, 513, 888, 41, "Reward: that region's light bridges stay awake across return visits and saves. Find a crescent near its start.", new GUIStyle(text) { fontSize = 17 });
            }
        }
        private void JournalRow(int row, string name, string detail, bool done)
        {
            float y = 222 + row * 94;
            Label(196, y, 888, 25, (done ? "LIT  /  " : "TO DO  /  ") + name, eyebrow);
            Label(196, y + 29, 888, 54, detail, new GUIStyle(text) { fontSize = 18 });
        }
        private static void Panel(float x, float y, float w, float h, Color color)
        { Color old = GUI.color; GUI.color = color; GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture); GUI.color = old; }
        private static void Label(float x, float y, float w, float h, string value, GUIStyle style) { GUI.Label(new Rect(x, y, w, h), value, style); }
        private bool Button(float x, float y, float w, float h, string label) { return GUI.Button(new Rect(x, y, w, h), label, button); }
    }
}
