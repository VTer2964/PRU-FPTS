using UnityEngine;
using FPTSim.Audio;
using FPTSim.Core;

namespace FPTSim.Audio
{
    public class MinigameResultSfxPlayer : MonoBehaviour
    {
        [Header("Medal SFX")]
        [SerializeField] private AudioClip goldClip;
        [SerializeField] private AudioClip silverClip;
        [SerializeField] private AudioClip bronzeClip;

        [Header("Optional")]
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip loseClip;

        public void PlayFor(MinigameResult result)
        {
            if (AudioManager.I == null) return;

            // optional win/lose
            if (result.scoreAwarded > 0)
            {
                if (winClip) AudioManager.I.PlaySfx(winClip);
            }
            else
            {
                if (loseClip) AudioManager.I.PlaySfx(loseClip);
            }

            // medal clip
            switch (result.medal)
            {
                case Medal.Gold:
                    if (goldClip) AudioManager.I.PlaySfx(goldClip);
                    break;
                case Medal.Silver:
                    if (silverClip) AudioManager.I.PlaySfx(silverClip);
                    break;
                case Medal.Bronze:
                    if (bronzeClip) AudioManager.I.PlaySfx(bronzeClip);
                    break;
            }
        }
    }
}