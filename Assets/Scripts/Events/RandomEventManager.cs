using UnityEngine;
using FPTSim.Core;

namespace FPTSim.Events
{
    public static class RandomEventManager
    {
        // Call after each minigame
        public static void TryRollAfterMinigame(GameConfigSO config, GameState state)
        {
            if (config == null || state == null) return;

            float r = Random.value;
            if (r > config.randomEventChancePerMinigame) return;

            // Simple event roll (prototype)
            int roll = Random.Range(0, 3);

            switch (roll)
            {
                case 0:
                    // Lost backpack => -1 score (or stress+)
                    state.totalScore = Mathf.Max(0, state.totalScore - 1);
                    state.stress += 10;
                    break;

                case 1:
                    // Scam => stress+
                    state.stress += 25;
                    break;

                case 2:
                    // "Cambodia" event -> small chance to disappear
                    if (Random.value < 0.15f)
                        state.disappeared = true;
                    else
                        state.stress += 15;
                    break;
            }
        }
    }
}