using System;
using UnityEngine;

namespace CaroGame
{
    public class CaroTimerSystem : MonoBehaviour
    {
        [SerializeField] private float turnTimeLimit = 60f;

        private float currentTurnTime;
        private bool isTimerActive;

        public event Action OnTurnTimeOut;
        public event Action<float, float> OnTimerUpdate; // remaining, total

        public float RemainingTime => currentTurnTime;
        public float TotalTime => turnTimeLimit;
        public float TimePercentage => Mathf.Clamp01(currentTurnTime / turnTimeLimit);
        public bool IsActive => isTimerActive;

        public void SetTimeLimit(float limit)
        {
            turnTimeLimit = limit;
        }

        public void StartTurn()
        {
            currentTurnTime = turnTimeLimit;
            isTimerActive = true;
        }

        public void StopTurn()
        {
            isTimerActive = false;
        }

        public void PauseTurn()
        {
            isTimerActive = false;
        }

        public void ResumeTurn()
        {
            if (currentTurnTime > 0)
                isTimerActive = true;
        }

        private void Update()
        {
            if (!isTimerActive) return;

            currentTurnTime -= Time.deltaTime;
            OnTimerUpdate?.Invoke(currentTurnTime, turnTimeLimit);

            if (currentTurnTime <= 0f)
            {
                currentTurnTime = 0f;
                isTimerActive = false;
                OnTurnTimeOut?.Invoke();
            }
        }
    }
}
