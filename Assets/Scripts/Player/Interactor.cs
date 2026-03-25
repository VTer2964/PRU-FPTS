using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using FPTSim.Core;

namespace FPTSim.Player
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float distance = 3f;
        [SerializeField] private LayerMask mask = ~0;
        [SerializeField] private TMP_Text hintText;
        [SerializeField] private Key interactKey = Key.E;

        private IInteractable[] currentInteractables;

        private void Update()
        {
            Scan();

            if (currentInteractables != null && currentInteractables.Length > 0 &&
                Keyboard.current != null &&
                Keyboard.current[interactKey].wasPressedThisFrame)
            {
                foreach (var it in currentInteractables)
                {
                    it.Interact();
                }
            }
        }

        private void Scan()
        {
            currentInteractables = null;

            if (!cam)
            {
                SetHint("");
                return;
            }

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, distance, mask, QueryTriggerInteraction.Collide))
            {
                var interactables = hit.collider.GetComponentsInParent<IInteractable>();
                if (interactables != null && interactables.Length > 0)
                {
                    currentInteractables = interactables;
                    
                    // Nối tất cả các prompt text lại với nhau nếu có nhiều IInteractable
                    string prompt = "";
                    foreach (var it in interactables)
                    {
                        string text = it.GetPromptText();
                        if (!string.IsNullOrEmpty(text))
                        {
                            if (prompt.Length > 0) prompt += " / ";
                            prompt += text;
                        }
                    }
                    
                    SetHint($"{prompt} (E)");
                    return;
                }
            }

            SetHint("");
        }

        private void SetHint(string msg)
        {
            if (hintText) hintText.text = msg;
        }
    }
}