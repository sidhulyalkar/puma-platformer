using UnityEngine;
using Wildbound.Core;

namespace Wildbound.Unity
{
    public sealed class WildboundHud : MonoBehaviour
    {
        private WildboundGame game;
        private GUIStyle text, title, eyebrow, button;
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
            if (!game.Playing) Title();
            else if (game.ShowEnding) Ending();
            else
            {
                HUD();
                if (game.ShowMap) Map();
                else if (game.ShowControls) Controls();
                else if (game.Session.Paused) Pause();
            }
            if (!game.Playing && game.ShowControls) Controls();
            if (game.ToastTime > 0 && !game.ShowControls && !game.ShowMap)
            {
                Panel(325, 94, 630, 62, ink); Label(347, 110, 586, 42, game.Toast, text);
            }
            GUI.matrix = previous;
        }
        private void Title()
        {
            Panel(0, 0, 590, 720, new Color(ink.r, ink.g, ink.b, .88f));
            Label(65, 108, 490, 25, "P U M A   /   A N   E X P L O R E R ' S   T A L E", eyebrow);
            Label(60, 153, 560, 95, "WILDBOUND", title);
            Label(65, 267, 430, 80, "Small paws.\nSome very big possibilities.", new GUIStyle(text) { fontSize = 29 });
            Label(65, 370, 405, 80, "Coil into a pounce. Chase a distant light. Discover what waits beyond the canopy.", text);
            if (Button(65, 488, 280, 54, game.Session.Save.Biome > 0 || game.Session.Save.Collected[0] != 0 ? "Continue your trail  /  ENTER" : "Begin exploring  /  ENTER")) game.Begin();
            if (Button(65, 557, 190, 43, "Field guide  /  C")) game.ShowControls = true;
            Label(65, 656, 465, 26, "A SID HULYALKAR GAME   /   DEVELOPMENT SLICE", eyebrow);
        }
        private void HUD()
        {
            Panel(24, 23, 415, 57, ink);
            Label(42, 32, 385, 23, game.Session.World.Name, eyebrow);
            Label(42, 54, 365, 22, "Explore at your own pace.", new GUIStyle(eyebrow) { fontSize = 12 });
            Panel(863, 23, 393, 57, ink);
            Label(884, 41, 360, 29, "LIGHT  " + game.Session.Motes + " / 12     MEMORIES  " + game.Session.Memories + " / 3", text);
            Label(28, 684, 770, 23, "SPACE jump    SHIFT pounce    E explore    TAB trail map    C guide    ESC pause", eyebrow);
            var p = game.Session.Player;
            if (!game.Session.Paused)
            {
                Sign sign = game.Session.NearbySign();
                if (sign != null)
                {
                    Panel(320, 566, 640, 99, ink); Label(341, 581, 598, 24, sign.Heading, eyebrow);
                    Label(341, 610, 598, 53, sign.Text, text);
                }
                if (p.Charging)
                {
                    Panel(490, 500, 300, 50, ink); Label(505, 507, 270, 23, p.Charge >= 1 ? "FULL COIL  /  RELEASE SHIFT" : "COILING  /  RELEASE TO POUNCE", eyebrow);
                    Panel(505, 535, 270, 5, new Color(.22f, .36f, .37f)); Panel(505, 535, 270 * p.Charge, 5, paper);
                }
                else Label(1030, 684, 230, 23, p.PounceReady ? "POUNCE READY" : "LAND TO RECHARGE", eyebrow);
            }
        }
        private void Controls()
        {
            Panel(230, 75, 820, 570, ink);
            Label(277, 106, 725, 40, "THE FIELD GUIDE", new GUIStyle(title) { fontSize = 35 });
            string[] keys = { "A / D or arrows", "SPACE  /  gamepad A", "SHIFT  /  gamepad X", "W / S while pouncing", "SPACE against a wall", "E  /  gamepad Y", "R   /   TAB   /   ESC" };
            string[] actions = { "Roam. The left stick works too.", "Jump. Hold to rise higher; release for a short hop.", "Hold to charge, release to leap. Land to recharge.", "Aim higher or dive. Up / down on the left stick.", "Kick away. Push into a wall to slow your fall.", "Step into an arch and explore the next world.", "Return to checkpoint / trail map / pause." };
            for (int i = 0; i < keys.Length; i++)
            {
                Label(278, 169 + i * 47, 245, 40, keys[i], eyebrow);
                Label(536, 165 + i * 47, 465, 43, actions[i], text);
            }
            Label(277, 510, 728, 50, "Pink flowers launch you and restore your pounce. Light and memories are optional. Falling never takes them away.", eyebrow);
            if (Button(277, 581, 205, 40, "Back  /  C")) { game.ShowControls = false; if (game.Playing) game.Resume(); }
            if (Button(500, 581, 205, 40, game.Muted ? "Sound: off" : "Sound: on")) game.ToggleMute();
            if (Button(723, 581, 280, 40, game.ReducedMotion ? "Motion: reduced" : "Motion: full")) game.ToggleMotion();
        }
        private void Pause()
        {
            Panel(400, 157, 480, 417, ink);
            Label(442, 196, 400, 50, "A MOMENT TO BREATHE", new GUIStyle(title) { fontSize = 29 });
            if (Button(444, 273, 392, 48, "Keep exploring")) game.Resume();
            if (Button(444, 335, 392, 48, "Field guide")) game.ShowControls = true;
            if (Button(444, 397, 392, 48, "New journey")) game.NewJourney();
            if (Button(444, 460, 185, 42, game.Muted ? "Sound: off" : "Sound: on")) game.ToggleMute();
            if (Button(642, 460, 194, 42, game.ReducedMotion ? "Less motion" : "Full motion")) game.ToggleMotion();
            Label(444, 526, 392, 24, "Your trail is saved on this device.", eyebrow);
        }
        private void Map()
        {
            Panel(150, 95, 980, 530, ink);
            Label(196, 126, 890, 40, "YOUR LITTLE CORNER OF THE WILD", new GUIStyle(title) { fontSize = 32 });
            Label(196, 184, 888, 32, game.Session.World.Name + "   /   You are the amber mark", eyebrow);
            foreach (var p in game.Session.World.Platforms)
            {
                var b = p.Bounds; if (b.H > 10) continue;
                Panel(210 + b.X * 10, 477 - b.Top * 11, b.W * 10, Mathf.Min(20, b.H * 11), new Color(.4f, .58f, .53f));
            }
            var pos = game.Session.Player.Position;
            Panel(205 + pos.X * 10, 466 - pos.Y * 11, 10, 10, new Color(1, .73f, .4f));
            foreach (var checkpoint in game.Session.World.Checkpoints) Panel(206 + checkpoint.X * 10, 467 - checkpoint.Y * 11, 8, 8, paper);
            Label(196, 510, 888, 42, "The arch leads onward. High paths hide memories. A missed discovery is a reason to wander.", text);
            string[] names = { "Canopy", "Grotto", "Sky garden" };
            for (int i = 0; i <= game.Session.Save.FurthestBiome; i++)
                if (Button(470 + i * 195, 569, 180, 37, names[i])) game.TravelTo(i);
            if (Button(196, 569, 250, 37, "Back to the trail  /  TAB")) game.Resume();
        }
        private void Ending()
        {
            Panel(287, 122, 706, 475, ink);
            Label(339, 159, 610, 30, "THREE WORLDS. ONE VERY CURIOUS CAT.", eyebrow);
            Label(336, 207, 610, 95, "Still wild at heart.", new GUIStyle(title) { fontSize = 49 });
            Label(340, 309, 600, 75, "You followed the trail from forest floor to open sky. There are always little things left to discover.", text);
            Label(340, 397, 600, 34, "Memories found: " + game.Session.Memories + " / 3", text);
            if (Button(340, 471, 280, 49, "Keep exploring")) game.Resume();
            if (Button(640, 471, 296, 49, "Start a new journey")) game.NewJourney();
            Label(340, 552, 600, 24, "Thank you for taking the scenic route.", eyebrow);
        }
        private static void Panel(float x, float y, float w, float h, Color color)
        { Color old = GUI.color; GUI.color = color; GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture); GUI.color = old; }
        private static void Label(float x, float y, float w, float h, string value, GUIStyle style) { GUI.Label(new Rect(x, y, w, h), value, style); }
        private bool Button(float x, float y, float w, float h, string label) { return GUI.Button(new Rect(x, y, w, h), label, button); }
    }
}
