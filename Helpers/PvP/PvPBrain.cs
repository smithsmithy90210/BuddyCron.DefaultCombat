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

                var property = target.GetType().GetProperty("IsPlayer");
                if (property != null && property.PropertyType == typeof(bool))
                {
                    var value = property.GetValue(target, null);
                    if (value is bool && (bool)value)
                        return true;
                }

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
            var key = "defensive-" + threshold;

            if (!IsPvPContext)
            {
                PvPHumanizer.Reset(key);
                return healthPercent <= threshold;
            }

            if (healthPercent > threshold)
            {
                PvPHumanizer.Reset(key);
                return false;
            }

            var urgency = healthPercent <= 20 ? PvPReactionUrgency.Critical : PvPReactionUrgency.Important;
            var ready = PvPHumanizer.Ready(key, true, urgency);
            LogDecision(ready ? "DEFENSIVE -> use: pressure confirmed" : "DEFENSIVE -> hold: reaction window");
            return ready;
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
