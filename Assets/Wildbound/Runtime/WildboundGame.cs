using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Wildbound.Core;

namespace Wildbound.Unity
{
    public sealed class WildboundGame : MonoBehaviour
    {
        public GameSession Session { get; private set; }
        public bool Playing { get; private set; }
        public bool ShowControls, ShowMap, ReducedMotion, Muted, ShowEnding;
        public string Toast = "";
        public float ToastTime;
        private const string SaveKey = "wildbound.journey.v1";
        private PlayerInput pending;
        private WorldView view;
        private WildboundAudio sound;
        private bool dirty, resetConfirm;
        private float saveDelay;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (FindFirstObjectByType<WildboundGame>() == null) new GameObject("Wildbound").AddComponent<WildboundGame>();
        }
        private void Awake()
        {
            Application.targetFrameRate = 60; Time.fixedDeltaTime = GameSession.StepSeconds;
            Time.maximumDeltaTime = .067f;
            ReducedMotion = PlayerPrefs.GetInt("wildbound.reducedMotion", 0) == 1;
            Muted = PlayerPrefs.GetInt("wildbound.muted", 0) == 1;
            JourneySave save = null;
            try { string json = PlayerPrefs.GetString(SaveKey, ""); if (json.Length > 0) save = JsonUtility.FromJson<JourneySave>(json); }
            catch (Exception) { Toast = "Your old trail could not be read. A fresh journey is ready."; ToastTime = 8; }
            Session = new GameSession(save); Session.SetPaused(true);
            view = gameObject.AddComponent<WorldView>(); view.Initialize(this);
            sound = gameObject.AddComponent<WildboundAudio>(); sound.Initialize(this);
            gameObject.AddComponent<WildboundHud>().Initialize(this);
        }
        private void Update()
        {
            var k = Keyboard.current; var pad = Gamepad.current;
            bool pause = (k != null && k.escapeKey.wasPressedThisFrame) || (pad != null && pad.startButton.wasPressedThisFrame);
            if (pause && Playing) { ShowControls = ShowMap = false; TogglePause(); }
            if (!Playing && ((k != null && k.enterKey.wasPressedThisFrame) || (pad != null && pad.startButton.wasPressedThisFrame))) Begin();
            if (k != null && k.cKey.wasPressedThisFrame) { ShowControls = !ShowControls; if (Playing) PauseForOverlay(); }
            if (k != null && k.tabKey.wasPressedThisFrame && Playing) { ShowMap = !ShowMap; PauseForOverlay(); }
            if (k != null && k.mKey.wasPressedThisFrame) ToggleMute();
            ToastTime = Mathf.Max(0, ToastTime - Time.unscaledDeltaTime);
            if (dirty && (saveDelay -= Time.unscaledDeltaTime) <= 0) FlushSave();
            if (!Playing || Session.Paused || ShowEnding) { pending = new PlayerInput(); return; }
            if (k != null && k.rKey.wasPressedThisFrame) { Session.Respawn(); pending = new PlayerInput(); view.SnapCamera(); return; }
            float move = k == null ? 0 : ((k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0));
            float aim = k == null ? 0 : ((k.wKey.isPressed || k.upArrowKey.isPressed ? 1 : 0) - (k.sKey.isPressed || k.downArrowKey.isPressed ? 1 : 0));
            if (pad != null && Mathf.Abs(pad.leftStick.x.ReadValue()) > .2f) move = pad.leftStick.x.ReadValue();
            if (pad != null && Mathf.Abs(pad.leftStick.y.ReadValue()) > .2f) aim = pad.leftStick.y.ReadValue();
            pending.Move = move; pending.AimY = aim;
            pending.JumpPressed |= (k != null && k.spaceKey.wasPressedThisFrame) || (pad != null && pad.buttonSouth.wasPressedThisFrame);
            pending.JumpHeld = (k != null && k.spaceKey.isPressed) || (pad != null && pad.buttonSouth.isPressed);
            pending.PouncePressed |= (k != null && k.leftShiftKey.wasPressedThisFrame) || (pad != null && pad.buttonWest.wasPressedThisFrame);
            pending.PounceHeld = (k != null && k.leftShiftKey.isPressed) || (pad != null && pad.buttonWest.isPressed);
            pending.PounceReleased |= (k != null && k.leftShiftKey.wasReleasedThisFrame) || (pad != null && pad.buttonWest.wasReleasedThisFrame);
            pending.InteractPressed |= (k != null && k.eKey.wasPressedThisFrame) || (pad != null && pad.buttonNorth.wasPressedThisFrame);
            pending.AttackPressed |= (k != null && k.jKey.wasPressedThisFrame) || (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) || (pad != null && pad.rightShoulder.wasPressedThisFrame);
            pending.DashPressed |= (k != null && k.kKey.wasPressedThisFrame) || (pad != null && pad.rightTrigger.wasPressedThisFrame);
            pending.RollPressed |= (k != null && k.lKey.wasPressedThisFrame) || (pad != null && pad.buttonEast.wasPressedThisFrame);
            pending.StalkHeld = (k != null && k.qKey.isPressed) || (pad != null && pad.leftTrigger.isPressed);
        }
        private void FixedUpdate()
        {
            if (!Playing || Session.Paused || ShowEnding) return;
            var world = Session.World;
            bool completedBefore = Session.Save.Completed;
            Session.Step(pending);
            pending.JumpPressed = pending.PouncePressed = pending.PounceReleased = pending.InteractPressed = false;
            pending.AttackPressed = pending.DashPressed = pending.RollPressed = false;
            GameEvent e = Session.Events;
            if (Session.World != world) { view.Rebuild(); Announce(Session.World.Subtitle); }
            if ((e & GameEvent.Respawn) != 0) { view.SnapCamera(); Announce("A soft landing. Your discoveries are still yours."); }
            if ((e & GameEvent.Checkpoint) != 0) Announce("Trail remembered.");
            if ((e & GameEvent.Secret) != 0) Announce(Session.World.Memory, 7);
            if ((e & GameEvent.Bloom) != 0) Announce("Moonwake. A bridge of light answers her claws.");
            if ((e & GameEvent.Hunt) != 0) Announce("Prey caught. A heart restored; ready to leap again.");
            if ((e & GameEvent.Block) != 0) Announce("Armored front. Get above or behind him.", 2);
            if ((e & GameEvent.Balance) != 0) Announce("Steady paws. The wind rests, and a crossing wakes.");
            if ((e & GameEvent.Moonbell) != 0) Announce("The bell answers. Pounce and air dash are ready again.", 2);
            if ((e & GameEvent.Breach) != 0) Announce("Roots part. Find your opening beyond them.");
            if ((e & GameEvent.ObjectiveBlocked) != 0 && Session.InTrial) Announce(Session.World.Trial.NextGoal(Session.World), 5);
            if ((e & GameEvent.Waystone) != 0) Announce("Waystone restored. This region's light bridges now stay awake.", 7);
            if ((e & (GameEvent.Collect | GameEvent.Secret | GameEvent.Checkpoint | GameEvent.Portal | GameEvent.Waystone)) != 0) MarkSave();
            if (!completedBefore && Session.Save.Completed) { ShowEnding = true; Session.SetPaused(true); }
            sound.React(e); view.React(e);
        }
        public void Begin()
        {
            Playing = true; ShowControls = ShowMap = ShowEnding = false; resetConfirm = false;
            Session.SetPaused(false); pending = new PlayerInput(); sound.Wake(); view.SnapCamera();
            Announce(Session.World.Subtitle);
        }
        public void TogglePause() { Session.SetPaused(!Session.Paused); pending = new PlayerInput(); }
        public void TravelTo(int biome)
        {
            if (!Session.TravelTo(biome)) return;
            view.Rebuild(); Resume(); MarkSave(); Announce(Session.World.Subtitle);
        }
        public void Resume() { ShowMap = ShowControls = ShowEnding = false; Session.SetPaused(false); pending = new PlayerInput(); }
        public void LeaveTrial()
        {
            if (!Session.LeaveTrial()) return;
            view.Rebuild(); Resume(); Announce("Back on your trail. You can try the waystone again.");
        }
        private void PauseForOverlay() { Session.SetPaused(ShowMap || ShowControls); pending = new PlayerInput(); }
        public void NewJourney()
        {
            if (!resetConfirm) { resetConfirm = true; Announce("Start fresh? Click New Journey again to replace your saved trail.", 6); return; }
            Session = new GameSession(); view.Rebuild(); Begin(); MarkSave();
        }
        public void ToggleMute() { Muted = !Muted; PlayerPrefs.SetInt("wildbound.muted", Muted ? 1 : 0); PlayerPrefs.Save(); }
        public void ToggleMotion() { ReducedMotion = !ReducedMotion; PlayerPrefs.SetInt("wildbound.reducedMotion", ReducedMotion ? 1 : 0); PlayerPrefs.Save(); }
        private void MarkSave() { dirty = true; saveDelay = .5f; }
        private void FlushSave()
        {
            if (!dirty) return;
            try { PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Session.Save)); PlayerPrefs.Save(); dirty = false; }
            catch (Exception) { dirty = false; Announce("Saving is unavailable. You can still explore this session.", 6); }
        }
        public void Announce(string text, float seconds = 4) { Toast = text; ToastTime = seconds; }
        private void OnApplicationFocus(bool focus)
        {
            if (!focus && Session != null) { Session.SetPaused(true); pending = new PlayerInput(); FlushSave(); }
        }
        private void OnApplicationPause(bool pause) { if (pause) OnApplicationFocus(false); }
        private void OnApplicationQuit() { FlushSave(); }
    }
}
