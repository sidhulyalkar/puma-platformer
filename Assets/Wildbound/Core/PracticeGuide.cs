using System;
using System.Collections.Generic;

namespace Wildbound.Core
{
    // Saved bits: keep these identities stable when changing the teaching order.
    [Flags]
    public enum PracticeSkill
    {
        None = 0, Jump = 1, Claw = 2, Scent = 4, Roll = 8, Pounce = 16,
        Moonwake = 32, Dash = 64, Spring = 128, WallKick = 256
    }

    public sealed class PracticeLesson
    {
        public readonly PracticeSkill Skill;
        public readonly string Name, Instruction, Feedback;
        public PracticeLesson(PracticeSkill skill, string name, string instruction, string feedback)
        { Skill = skill; Name = name; Instruction = instruction; Feedback = feedback; }
    }

    /// <summary>Optional teaching, driven by successful simulation actions rather than button presses.</summary>
    public sealed class PracticeGuide
    {
        public const int AllSkills = 511;
        public static readonly IReadOnlyList<PracticeLesson> Lessons = Array.AsReadOnly(new[]
        {
            new PracticeLesson(PracticeSkill.Jump, "A SOFT LANDING", "A / D or left stick moves. SPACE / A jumps; hold for height, release for a shorter hop.", "A jump of your own. Hold longer to reach a higher branch."),
            new PracticeLesson(PracticeSkill.Claw, "LEAVE YOUR MARK", "Face the scratch post and tap J / RB to connect a claw. Tap again to link the next strike.", "Claws connected. Another tap links the next strike."),
            new PracticeLesson(PracticeSkill.Scent, "FOLLOW YOUR NOSE", "Hold Q / LT beside the pawprints. Nearby scents reveal a path worth following.", "A scent found. Follow the pawprints toward their resting place."),
            new PracticeLesson(PracticeSkill.Roll, "SMALL PATHS", "L / B rolls along the ground. Keep moving beneath low roots to crawl clear.", "Low and quick. Keep moving if the ceiling is too low to stand."),
            new PracticeLesson(PracticeSkill.Pounce, "COIL AND RELEASE", "Hold SHIFT / X to coil, aim with W / S or the stick, then release. Land before the next pounce.", "A pounce launched. A longer coil carries you farther."),
            new PracticeLesson(PracticeSkill.Moonwake, "WAKE THE CROSSING", "Claw a blue moonbloom with J / RB. Its light bridge stays solid after the flare fades.", "The flower answered. Its bridge stays awake for this visit."),
            new PracticeLesson(PracticeSkill.Dash, "CLOSE THE DISTANCE", "K / RT launches a dash-claw. You have one aerial dash before landing or a refill.", "A dash launched. Land or touch a spring to refill your aerial dash."),
            new PracticeLesson(PracticeSkill.Spring, "BORROW A LITTLE SPRING", "Land on a pink flower. It launches you and restores your pounce and aerial dash.", "Fresh momentum. Your pounce and aerial dash are ready again."),
            new PracticeLesson(PracticeSkill.WallKick, "CLAWS ON STONE", "Press toward a wall while falling to slide. SPACE / A kicks away from it.", "A wall kick. Let it carry you clear before steering back.")
        });

        public PracticeLesson Recent { get; private set; }
        public float NoticeSeconds { get; private set; }
        public static bool Has(JourneySave save, PracticeSkill skills)
        { return ((PracticeSkill)save.Practiced & skills) == skills; }
        public void ClearNotice() { Recent = null; NoticeSeconds = 0; }

        internal bool Observe(GameSession session, float dt)
        {
            NoticeSeconds = Math.Max(0, NoticeSeconds - dt);
            if (NoticeSeconds == 0) Recent = null;
            PracticeSkill seen = PracticeSkill.None;
            GameEvent events = session.Events;
            if ((events & GameEvent.Jump) != 0) seen |= PracticeSkill.Jump;
            if ((events & GameEvent.ClawHit) != 0) seen |= PracticeSkill.Claw;
            if ((events & GameEvent.Roll) != 0) seen |= PracticeSkill.Roll;
            if ((events & GameEvent.Pounce) != 0) seen |= PracticeSkill.Pounce;
            if ((events & GameEvent.Bloom) != 0) seen |= PracticeSkill.Moonwake;
            if ((events & GameEvent.DashClaw) != 0) seen |= PracticeSkill.Dash;
            if ((events & GameEvent.Spring) != 0) seen |= PracticeSkill.Spring;
            if ((events & GameEvent.WallKick) != 0) seen |= PracticeSkill.WallKick;
            // Holding stalk in an empty room, or behind a wall, is not finding a scent.
            if (!Has(session.Save, PracticeSkill.Scent) && session.Player.Grounded && session.NearbyTrail() != null)
                seen |= PracticeSkill.Scent;
            PracticeSkill added = seen & ~(PracticeSkill)session.Save.Practiced;
            if (added == PracticeSkill.None) return false;
            session.Save.Practiced |= (int)added;
            foreach (var lesson in Lessons)
                if ((added & lesson.Skill) != 0) { Recent = lesson; break; }
            NoticeSeconds = 2.5f;
            return true;
        }

        public PracticeLesson NearbyLesson(GameSession session)
        {
            if (session.Paused || session.Recovery > 0 || session.InTrial || session.Player.LowProfile) return null;
            Sign sign = session.NearbySign();
            if (sign == null) return null;
            foreach (var lesson in Lessons)
                if ((sign.Skills & lesson.Skill) != 0 && !Has(session.Save, lesson.Skill)) return lesson;
            return null;
        }

        public static V2 ObjectivePosition(GameSession session)
        { return session.InTrial ? session.World.Trial.NextPosition(session.World) : session.World.Exit; }
    }
}
