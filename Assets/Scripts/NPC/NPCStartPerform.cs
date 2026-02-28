using UnityEngine;

namespace FPTSim.NPC
{
    public class NPCStartPerform : MonoBehaviour
    {
        [SerializeField] private NPCBrain brain;
        [SerializeField] private bool startPerforming = true;

        private void Start()
        {
            if (!brain) brain = GetComponent<NPCBrain>();
            if (brain != null) brain.SetPerforming(startPerforming);
        }
    }
}