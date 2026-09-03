namespace Wildbound.Core
{
    public enum JourneyScreen { Title, Playing, Pause, Controls, Map, Ending, ConfirmNewJourney }

    /// <summary>Owns the visible screen and its simulation pause state as one transition.</summary>
    public sealed class JourneyFlow
    {
        public GameSession Session { get; private set; }
        public JourneyScreen Screen { get; private set; } = JourneyScreen.Title;
        public bool Started { get; private set; }
        private JourneyScreen overlayReturn = JourneyScreen.Title, resetReturn = JourneyScreen.Pause;

        public JourneyFlow(JourneySave save = null)
        { Session = new GameSession(save); Session.SetPaused(true); }

        private bool Change(JourneyScreen next)
        {
            if (Screen == next) return false;
            Screen = next; Session.SetPaused(next != JourneyScreen.Playing); return true;
        }
        public bool Begin()
        {
            if (Started) return false;
            Started = true; return Change(JourneyScreen.Playing);
        }
        public bool Resume() { return Started && Change(JourneyScreen.Playing); }
        public bool ToggleControls() { return ToggleOverlay(JourneyScreen.Controls); }
        public bool ToggleMap() { return Started && ToggleOverlay(JourneyScreen.Map); }
        private bool ToggleOverlay(JourneyScreen next)
        {
            if (Screen == JourneyScreen.ConfirmNewJourney) return false;
            if (Screen == next) return Change(overlayReturn);
            if (Screen != JourneyScreen.Controls && Screen != JourneyScreen.Map) overlayReturn = Screen;
            return Change(next);
        }
        public bool Back()
        {
            switch (Screen)
            {
                case JourneyScreen.Playing: return Change(JourneyScreen.Pause);
                case JourneyScreen.Pause:
                case JourneyScreen.Ending: return Resume();
                case JourneyScreen.Controls:
                case JourneyScreen.Map: return Change(overlayReturn);
                case JourneyScreen.ConfirmNewJourney: return Change(resetReturn);
                default: return false;
            }
        }
        public bool LoseFocus()
        {
            if (Screen == JourneyScreen.ConfirmNewJourney) return Back();
            if (Screen == JourneyScreen.Playing) return Change(JourneyScreen.Pause);
            // Closing a guide after switching back to the app must not resume unattended play.
            if ((Screen == JourneyScreen.Controls || Screen == JourneyScreen.Map) && overlayReturn == JourneyScreen.Playing)
            { overlayReturn = JourneyScreen.Pause; return true; }
            return false;
        }
        public bool RequestNewJourney()
        {
            if (Screen != JourneyScreen.Pause && Screen != JourneyScreen.Ending) return false;
            resetReturn = Screen; return Change(JourneyScreen.ConfirmNewJourney);
        }
        public bool ConfirmNewJourney()
        {
            if (Screen != JourneyScreen.ConfirmNewJourney) return false;
            Session = new GameSession(); return Change(JourneyScreen.Playing);
        }
        public bool TravelTo(int biome)
        {
            if (!Started || Screen == JourneyScreen.ConfirmNewJourney || !Session.TravelTo(biome)) return false;
            Resume(); return true;
        }
        public bool LeaveTrial()
        {
            if (!Started || Screen == JourneyScreen.ConfirmNewJourney || !Session.LeaveTrial()) return false;
            Resume(); return true;
        }
        public void Step(PlayerInput input, float dt = GameSession.StepSeconds)
        {
            if (Screen != JourneyScreen.Playing) return;
            bool completed = Session.Save.Completed;
            Session.Step(input, dt);
            if (!completed && Session.Save.Completed) Change(JourneyScreen.Ending);
        }
    }
}
