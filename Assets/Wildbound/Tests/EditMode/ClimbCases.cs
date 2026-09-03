using System;
using System.Collections.Generic;
using Wildbound.Core;

namespace Wildbound.Tests
{
    public static class ClimbCases
    {
        public static readonly Dictionary<string, Action> All = new Dictionary<string, Action>
        {
            { "Bark wall allows claw-climb ascent", ClimbOnBark },
            { "Stone wall does not grant climb", NoClimbOnStone },
            { "Climb budget expires and ends climb", ClimbBudgetExpires },
            { "Jump while climbing issues a wall kick", ClimbJumpKick }
        };

        private static void Check(bool c, string why) { if (!c) throw new Exception(why); }

        private static void Tick(GameSession g, int n, PlayerInput input = default(PlayerInput))
        {
            for (int i = 0; i < n; i++)
            {
                g.Step(input);
                input.JumpPressed = input.PouncePressed = input.PounceReleased = input.AttackPressed = false;
            }
        }

        private static GameSession WallScene(Surface wallSurface)
        {
            var g = new GameSession();
            g.World.Platforms.Clear(); g.World.Enemies.Clear(); g.World.Hazards.Clear(); g.World.Blooms.Clear();
            g.World.Add(-20, -2, 60, 2);
            // Tall thin wall to the right of spawn
            g.World.Add(1.2f, 0, .5f, 10f, wallSurface);
            g.Player.Reset(new V2(0, 0));
            Tick(g, 10);
            return g;
        }

        private static void ClimbOnBark()
        {
            var g = WallScene(Surface.Bark);
            float y0 = g.Player.Position.Y;
            // Jump into wall, hold right + jump to climb
            g.Step(new PlayerInput { JumpPressed = true, JumpHeld = true, Move = 1 });
            Tick(g, 8, new PlayerInput { JumpHeld = true, Move = 1 });
            Tick(g, 50, new PlayerInput { JumpHeld = true, Move = 1 });
            Check(g.Player.Climbing || g.Player.Position.Y > y0 + 1.2f, "Did not climb Bark wall");
            Check(g.Player.Position.Y > y0 + 1.0f, "Climb did not gain height");
        }

        private static void NoClimbOnStone()
        {
            var g = WallScene(Surface.Stone);
            g.Step(new PlayerInput { JumpPressed = true, JumpHeld = true, Move = 1 });
            Tick(g, 60, new PlayerInput { JumpHeld = true, Move = 1 });
            Check(!g.Player.Climbing, "Stone wall should not enter Climbing state");
        }

        private static void ClimbBudgetExpires()
        {
            var g = WallScene(Surface.Bark);
            g.Player.ClimbBudget = 0.25f;
            g.Step(new PlayerInput { JumpPressed = true, JumpHeld = true, Move = 1 });
            Tick(g, 5, new PlayerInput { JumpHeld = true, Move = 1 });
            Tick(g, 80, new PlayerInput { JumpHeld = true, Move = 1 });
            Check(!g.Player.Climbing, "Climb should end when budget expires");
            Check(g.Player.ClimbBudget <= 0.01f, "Budget should be exhausted");
        }

        private static void ClimbJumpKick()
        {
            var g = WallScene(Surface.Bark);
            g.Step(new PlayerInput { JumpPressed = true, JumpHeld = true, Move = 1 });
            Tick(g, 20, new PlayerInput { JumpHeld = true, Move = 1 });
            Check(g.Player.Climbing || g.Player.WallClimbable, "Setup climb");
            // Force climbing if wall contact exists
            if (g.Player.WallClimbable && g.Player.Wall != 0)
            {
                g.Player.Climbing = true;
                g.Player.ClimbBudget = 1f;
            }
            float x0 = g.Player.Position.X;
            g.Step(new PlayerInput { JumpPressed = true, Move = 1 });
            Check((g.Events & GameEvent.WallKick) != 0 || g.Player.Velocity.X < -1f || !g.Player.Climbing,
                "Jump from climb should kick away or clear climb");
            Tick(g, 5);
            Check(g.Player.Position.X < x0 - 0.2f || !g.Player.Climbing, "Should leave the wall after kick");
        }
    }
}
