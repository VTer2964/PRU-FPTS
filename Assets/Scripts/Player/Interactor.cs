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

        private IInteractable current;

        private void Update()
        {
            Scan();

            if (current != null &&
                Keyboard.current != null &&
                Keyboard.current[interactKey].wasPressedThisFrame)
            {
                current.Interact();
            }
        }

        private void Scan()
        {
            current = null;

            if (!cam)
            {
                SetHint("");
                return;
            }

            Ray ray = new Ray(cam.transform.position, cam.transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, distance, mask, QueryTriggerInteraction.Collide))
            {
                var it = hit.collider.GetComponentInParent<IInteractable>();
                if (it != null)
                {
                    current = it;
                    SetHint($"{it.GetPromptText()} (E)");
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