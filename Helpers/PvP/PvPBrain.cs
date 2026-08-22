using System;
using BuddyCron;
using DefaultCombat.Helpers;

namespace DefaultCombat.Helpers.PvP
{
    public static class PvPBrain
    {
        private static string _lastDecision;

        public static bool IsPvPContext
        {
            get
            {
                var target = Core.Player.Target;
                if (target == null)
                    return false;

                var typeName = target.GetType().Name;
                return typeName.IndexOf("Player", StringComparison.OrdinalIgnoreCase) >= 0;
            }
        }

        public static bool ShouldBreakCrowdControl()
        {
            if (!IsPvPContext)
                return Core.Player.IsStunned;

            var ready = PvPHumanizer.Ready("cc-break", Core.Player.IsStunned, PvPReactionUrgency.Critical);
            LogDecision(ready ? "CC_BREAK -> use: control confirmed" : "CC_BREAK -> hold: reaction window");
            return ready;
        }

        public static bool ShouldInterruptCurrentTarget()
        {
            var target = Core.Player.Target;
            if (target == null || !target.IsCasting || !CombatHotkeys.EnableInterrupts)
            {
                PvPHumanizer.Reset("interrupt-current");
                return false;
            }

            if (!IsPvPContext)
                return true;

            var urgency = target.HealthPercent <= 35
                ? PvPReactionUrgency.Critical
                : PvPReactionUrgency.Important;

            var ready = PvPHumanizer.Ready("interrupt-current", true, urgency);
            LogDecision(ready ? "JOLT -> use: cast confirmed" : "JOLT -> hold: reaction window");
            return ready;
        }

        public static bool ShouldPreserveTargetPosition()
        {
            if (!IsPvPContext)
                return false;

            var target = Core.Player.Target;
            if (target == null)
                return false;

            var preserve = target.HealthPercent <= 30;
            if (preserve)
                LogDecision("DISPLACEMENT -> rejected: current kill pressure");

            return preserve;
        }

        public static bool ShouldUseDefensive(int healthPercent, int threshold)
        {
            if (!IsPvPContext)
                return healthPercent <= threshold;

            if (healthPercent > threshold)
                return false;

            var urgency = healthPercent <= 20 ? PvPReactionUrgency.Critical : PvPReactionUrgency.Important;
            return PvPHumanizer.Ready("defensive-" + threshold, true, urgency);
        }

        private static void LogDecision(string decision)
        {
            if (decision == _lastDecision)
                return;

            _lastDecision = decision;
            Logger.Write("[PvP] " + decision);
        }
    }
}
