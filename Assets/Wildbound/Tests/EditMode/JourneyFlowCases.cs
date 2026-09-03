using System;
using System.Collections.Generic;
using Wildbound.Core;

namespace Wildbound.Tests
{
    public static class JourneyFlowCases
    {
        public static readonly Dictionary<string, Action> All = new Dictionary<string, Action>
        {
            { "Title guide returns to title without starting simulation", Title },
            { "Guide opened during play pauses and returns to play", PlayGuide },
            { "Guide opened from pause returns to pause", PauseGuide },
            { "Switching map and guide preserves one overlay and its origin", SwapOverlays },
            { "Opening a menu cancels a charged pounce", CancelCoil },
            { "Focus loss stops play until explicit resume", FocusPlaying },
            { "Focus loss inside an overlay makes its return stay paused", FocusOverlay },
            { "New journey requires an explicit confirmation screen", RequestReset },
            { "Canceling reset retains the original session and saved progress", CancelReset },
            { "Resuming cancels reset authorization before the next visit", ResumeCancelsReset },
            { "Focus loss cancels reset authorization", FocusCancelsReset },
            { "Only explicit confirmation replaces the session and only once", ConfirmReset },
            { "A final arch opens the ending with the simulation paused", Ending },
            { "Back leaves the ending and allows continued exploration", LeaveEnding },
            { "Ending overlays and reset cancellation return to the ending", EndingOverlays },
            { "An already completed save can begin exploring normally", CompletedSave },
            { "Map travel resumes with practice discoveries and collectibles retained", MapTravel },
            { "Leaving a trial through its map restores the outside session", LeaveTrial },
            { "Random menu sequences keep pause and session ownership consistent", NavigationStress }
        };
        private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
        private static JourneyFlow Start(JourneySave save = null)
        { var f = new JourneyFlow(save); f.Begin(); Tick(f, 5); return f; }
        private static void Tick(JourneyFlow f, int count, PlayerInput input = default(PlayerInput))
        {
            for (int i = 0; i < count; i++)
            {
                f.Step(input);
                input.JumpPressed = input.PouncePressed = input.PounceReleased = input.AttackPressed = input.InteractPressed = false;
            }
        }
        private static void At(JourneyFlow f, JourneyScreen screen)
        { Check(f.Screen == screen && f.Session.Paused == (screen != JourneyScreen.Playing), "Screen and simulation disagree: " + f.Screen); }
        private static JourneySave Progress()
        { return new JourneySave { Biome = 1, FurthestBiome = 2, Collected = new[] { 7, 3, 1 }, Checkpoints = new[] { 1, 0, 1 }, Discoveries = 17, Waystones = 5, Practiced = 63 }; }
        private static void Title()
        {
            var f = new JourneyFlow(); At(f, JourneyScreen.Title);
            Check(!f.Resume() && !f.ToggleMap() && !f.TravelTo(0) && !f.ConfirmNewJourney(), "Title allowed play or reset");
            f.ToggleControls(); At(f, JourneyScreen.Controls); Tick(f, 60, new PlayerInput { Move = 1, JumpPressed = true });
            Check(f.Session.Time == 0 && !f.Started && f.Session.Save.Practiced == 0, "Title guide advanced game");
            f.Back(); At(f, JourneyScreen.Title); f.ToggleControls(); Check(f.Begin(), "Could not begin from title guide"); At(f, JourneyScreen.Playing);
        }
        private static void PlayGuide()
        {
            var f = Start(); float time = f.Session.Time; f.ToggleControls(); At(f, JourneyScreen.Controls);
            Tick(f, 60, new PlayerInput { Move = 1 }); Check(f.Session.Time == time, "Guide did not freeze simulation");
            f.ToggleControls(); At(f, JourneyScreen.Playing); f.Step(new PlayerInput()); Check(f.Session.Time > time, "Guide return did not resume");
        }
        private static void PauseGuide()
        {
            var f = Start(); f.Back(); f.ToggleControls(); At(f, JourneyScreen.Controls);
            f.ToggleControls(); At(f, JourneyScreen.Pause); f.Back(); At(f, JourneyScreen.Playing);
        }
        private static void SwapOverlays()
        {
            foreach (bool paused in new[] { false, true })
            {
                var f = Start(); if (paused) f.Back();
                f.ToggleControls(); f.ToggleMap(); At(f, JourneyScreen.Map);
                f.ToggleControls(); At(f, JourneyScreen.Controls); f.ToggleMap(); f.Back();
                At(f, paused ? JourneyScreen.Pause : JourneyScreen.Playing);
            }
        }
        private static void CancelCoil()
        {
            var f = Start(); Tick(f, 20, new PlayerInput { PouncePressed = true, PounceHeld = true });
            Check(f.Session.Player.Charging, "Coil did not start"); f.ToggleMap();
            Check(!f.Session.Player.Charging, "Menu retained pounce charge"); f.ToggleMap();
            f.Step(new PlayerInput { PounceReleased = true });
            Check((f.Session.Events & GameEvent.Pounce) == 0 && !PracticeGuide.Has(f.Session.Save, PracticeSkill.Pounce), "Closing map released canceled coil");
        }
        private static void FocusPlaying()
        {
            var f = Start(); f.LoseFocus(); At(f, JourneyScreen.Pause); float time = f.Session.Time;
            Tick(f, 60, new PlayerInput { Move = 1 }); Check(f.Session.Time == time, "Lost focus kept playing");
            f.Back(); At(f, JourneyScreen.Playing);
        }
        private static void FocusOverlay()
        {
            foreach (bool map in new[] { false, true })
            {
                var f = Start(); if (map) f.ToggleMap(); else f.ToggleControls(); f.LoseFocus();
                // Also switch overlay after focus returns, before closing it.
                if (map) f.ToggleControls(); else f.ToggleMap();
                f.Back(); At(f, JourneyScreen.Pause);
            }
        }
        private static void RequestReset()
        {
            var f = Start(Progress()); var original = f.Session;
            Check(!f.RequestNewJourney() && !f.ConfirmNewJourney(), "Running game authorized reset");
            f.ToggleControls(); Check(!f.RequestNewJourney(), "Guide authorized reset"); f.Back(); f.Back();
            Check(f.RequestNewJourney(), "Pause could not open confirmation"); At(f, JourneyScreen.ConfirmNewJourney);
            Check(!f.RequestNewJourney() && !f.ToggleControls() && !f.ToggleMap() && !f.TravelTo(0) && !f.LeaveTrial(), "Confirmation allowed another navigation action");
            Tick(f, 90, new PlayerInput { InteractPressed = true, AttackPressed = true });
            Check(ReferenceEquals(f.Session, original), "Repeated request or input erased progress");
        }
        private static void CancelReset()
        {
            var f = Start(Progress()); var original = f.Session; f.Back(); f.RequestNewJourney(); f.Back();
            At(f, JourneyScreen.Pause);
            Check(!f.ConfirmNewJourney() && ReferenceEquals(original, f.Session) && f.Session.Save.Collected[0] == 7
                && f.Session.Save.Discoveries == 17 && f.Session.Save.Practiced == 63, "Cancel replaced saved trail");
        }
        private static void ResumeCancelsReset()
        {
            var f = Start(Progress()); var original = f.Session; f.Back(); f.RequestNewJourney(); f.Resume();
            f.Back(); Check(!f.ConfirmNewJourney(), "Old confirmation stayed armed after resuming");
            f.RequestNewJourney(); At(f, JourneyScreen.ConfirmNewJourney);
            Check(ReferenceEquals(f.Session, original), "A later New Journey click immediately erased progress");
        }
        private static void FocusCancelsReset()
        {
            var f = Start(Progress()); var original = f.Session; f.Back(); f.RequestNewJourney(); f.LoseFocus();
            At(f, JourneyScreen.Pause); Check(!f.ConfirmNewJourney() && ReferenceEquals(f.Session, original), "Focus loss left destructive confirmation armed");
        }
        private static void ConfirmReset()
        {
            var f = Start(Progress()); var original = f.Session; f.Back(); f.RequestNewJourney();
            Check(f.ConfirmNewJourney() && !ReferenceEquals(f.Session, original), "Explicit reset did not replace session");
            At(f, JourneyScreen.Playing); var fresh = f.Session; var save = fresh.Save;
            Check(save.Biome == 0 && save.FurthestBiome == 0 && save.Collected[0] == 0 && save.Waystones == 0 && save.Discoveries == 0
                && save.Practiced == 0 && save.Checkpoints[0] == -1 && !save.Completed, "New journey retained progress");
            Check(!f.ConfirmNewJourney() && ReferenceEquals(fresh, f.Session) && original.Save.Discoveries == 17, "Duplicate confirm reset again or mutated old save object");
        }
        private static JourneyFlow Finish()
        {
            var f = Start(new JourneySave { Biome = 2 }); f.Session.Player.Reset(f.Session.World.Exit); Tick(f, 3);
            f.Step(new PlayerInput { InteractPressed = true }); return f;
        }
        private static void Ending()
        {
            var f = Finish(); At(f, JourneyScreen.Ending); float time = f.Session.Time;
            Check(f.Session.Save.Completed && (f.Session.Events & GameEvent.Portal) != 0, "Completion save event lost");
            Tick(f, 120, new PlayerInput { Move = 1 }); Check(f.Session.Time == time, "Ending kept running");
        }
        private static void LeaveEnding()
        {
            var f = Finish(); f.Back(); At(f, JourneyScreen.Playing); float time = f.Session.Time;
            f.Step(new PlayerInput { InteractPressed = true }); At(f, JourneyScreen.Playing);
            Check(f.Session.Time > time && f.Session.Save.Completed, "Ending became stuck or completion was erased");
        }
        private static void EndingOverlays()
        {
            var f = Finish(); f.ToggleControls(); f.ToggleMap(); f.Back(); At(f, JourneyScreen.Ending);
            f.RequestNewJourney(); f.Back(); At(f, JourneyScreen.Ending);
            f.RequestNewJourney(); f.LoseFocus(); At(f, JourneyScreen.Ending);
            Check(!f.ConfirmNewJourney() && f.Session.Save.Completed, "Ending reset cancellation lost progress");
        }
        private static void CompletedSave()
        {
            var save = Progress(); save.Completed = true; var f = Start(save);
            At(f, JourneyScreen.Playing); Check(f.Session.Save.Completed, "Existing completion was erased");
        }
        private static void MapTravel()
        {
            var f = Start(Progress()); var save = f.Session.Save; f.Back(); f.ToggleMap();
            Check(!f.TravelTo(8), "Invalid region accepted"); At(f, JourneyScreen.Map);
            Check(f.TravelTo(2), "Discovered region unavailable"); At(f, JourneyScreen.Playing);
            Check(ReferenceEquals(save, f.Session.Save) && save.Collected[0] == 7 && save.Practiced == 63 && save.Discoveries == 17, "Map travel lost progress");
        }
        private static void LeaveTrial()
        {
            var f = Start(); var outside = f.Session.World; f.Session.Player.Reset(Moontrial.Entrance); Tick(f, 3);
            f.Step(new PlayerInput { InteractPressed = true }); Check(f.Session.InTrial, "Did not enter trial");
            f.ToggleMap(); Check(f.LeaveTrial(), "Map could not leave trial"); At(f, JourneyScreen.Playing);
            Check(ReferenceEquals(f.Session.World, outside), "Leaving trial replaced outside world");
        }
        private static void NavigationStress()
        {
            var f = new JourneyFlow(Progress()); var random = new Random(407);
            for (int i = 0; i < 5000; i++)
            {
                var before = f.Session; bool reset = false;
                switch (random.Next(10))
                {
                    case 0: f.Begin(); break;
                    case 1: f.Back(); break;
                    case 2: f.ToggleControls(); break;
                    case 3: f.ToggleMap(); break;
                    case 4: f.LoseFocus(); break;
                    case 5: f.RequestNewJourney(); break;
                    case 6: reset = f.ConfirmNewJourney(); break;
                    case 7: f.Resume(); break;
                    case 8: f.TravelTo(random.Next(4)); break;
                    case 9: f.Step(new PlayerInput { Move = .1f }); break;
                }
                Check(f.Session.Paused == (f.Screen != JourneyScreen.Playing), "Pause diverged at step " + i);
                Check(ReferenceEquals(before, f.Session) != reset, "Unconfirmed session replacement at step " + i);
            }
        }
    }
}
