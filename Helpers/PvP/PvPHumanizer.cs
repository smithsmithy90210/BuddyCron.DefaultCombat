using System;
using System.Collections.Generic;

namespace DefaultCombat.Helpers.PvP
{
    public enum PvPReactionUrgency
    {
        Routine,
        Important,
        Critical
    }

    public static class PvPHumanizer
    {
        private sealed class ReactionState
        {
            public DateTime ReadyAt;
        }

        private static readonly Dictionary<string, ReactionState> Reactions = new Dictionary<string, ReactionState>();
        private static readonly Random Random = new Random();

        public static bool Ready(string key, bool observed, PvPReactionUrgency urgency)
        {
            if (!observed)
            {
                Reactions.Remove(key);
                return false;
            }

            ReactionState state;
            if (!Reactions.TryGetValue(key, out state))
            {
                state = new ReactionState
                {
                    ReadyAt = DateTime.UtcNow.AddMilliseconds(ReactionDelay(urgency))
                };
                Reactions[key] = state;
                return false;
            }

            return DateTime.UtcNow >= state.ReadyAt;
        }

        public static void Reset(string key)
        {
            Reactions.Remove(key);
        }

        private static int ReactionDelay(PvPReactionUrgency urgency)
        {
            switch (urgency)
            {
                case PvPReactionUrgency.Critical:
                    return Random.Next(120, 221);
                case PvPReactionUrgency.Important:
                    return Random.Next(190, 341);
                default:
                    return Random.Next(300, 551);
            }
        }
    }
}
