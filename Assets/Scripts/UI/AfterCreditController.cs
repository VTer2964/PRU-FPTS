using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using FPTSim.Core;

namespace FPTSim.UI
{
    public class AfterCreditController : MonoBehaviour
    {
        [Header("Credit Text")]
        [SerializeField] private RectTransform creditsTransform;
        [SerializeField] private float scrollSpeed = 40f;
        [SerializeField] private float endY = 1200f;

        [Header("Auto Return")]
        [SerializeField] private float fallbackReturnTime = 20f;

        private float timer;

        private void Update()
        {
            timer += Time.deltaTime;

            if (creditsTransform != null)
            {
                creditsTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

                if (creditsTransform.anchoredPosition.y >= endY)
                {
                    ReturnToMenu();
                    return;
                }
            }

            if (timer >= fallbackReturnTime)
            {
                ReturnToMenu();
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnToMenu();
            }
        }

        private void ReturnToMenu()
        {
            if (GameManager.I != null)
                GameManager.I.GoMainMenu();
            else
                SceneManager.LoadScene(SceneNames.MainMenu);
        }
    }
}