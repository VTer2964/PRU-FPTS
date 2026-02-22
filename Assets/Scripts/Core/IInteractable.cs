using UnityEngine;

namespace FPTSim.Core
{
    public interface IInteractable
    {
        string GetPromptText();
        void Interact();
    }
}