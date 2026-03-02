using UnityEngine;
using UnityEngine.SceneManagement;
using FPTSim.Core;

namespace FPTSim.Minigames
{
    public abstract class MinigameBase : MonoBehaviour
    {
        [Header("Minigame")]
        [SerializeField] protected string minigameId = "Unknown";
        [SerializeField] protected float timeLimit = 30f;

        protected float timeLeft;
        protected bool finished;

        protected virtual void Start()
        {
            timeLeft = timeLimit;
            finished = false;

            // Unlock cursor — Campus MouseLook có thể đã lock trước khi load scene
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        protected virtual void Update()
        {
            if (finished) return;

            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                OnTimeUp();
            }
        }

        protected virtual void OnTimeUp()
        {
            Finish(new MinigameResult
            {
                minigameId = minigameId,
                medal = Medal.None,
                scoreAwarded = 0,
                success = false
            });
        }

        protected void Finish(MinigameResult result)
        {
            if (finished) return;
            finished = true;

            if (GameManager.I != null)
            {
                GameManager.I.RegisterMinigameResult(result);
            }

            // Hiển thị kết quả, sau đó load Campus
            MinigameResultPanel.Show(result, () => SceneManager.LoadScene(SceneNames.Campus));
        }
    }
}