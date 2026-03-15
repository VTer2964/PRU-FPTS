using UnityEngine;
using FPTSim.Core;

namespace FPTSim.NPC
{
    public class NPCPatrolToggleByFlags : MonoBehaviour
    {
        [System.Serializable]
        public class PatrolRule
        {
            [Header("Flag can check")]
            public string requiredFlag;

            [Header("Dao dieu kien flag")]
            public bool invertMatch;

            [Header("Trang thai patrol khi rule match")]
            public bool enablePatrol = true;
        }

        [Header("Target")]
        [SerializeField] private NPCPatrol patrol;

        [Header("Default")]
        [SerializeField] private bool defaultEnabled = true;

        [Header("Rules (uu tien tu tren xuong duoi)")]
        [SerializeField] private PatrolRule[] rules;

        [Header("Auto update")]
        [SerializeField] private bool updateEveryFrame = true;

        private bool hasApplied;
        private bool lastApplied;

        private void Awake()
        {
            if (patrol == null)
                patrol = GetComponent<NPCPatrol>();
        }

        private void Start()
        {
            RefreshPatrol();
        }

        private void Update()
        {
            if (updateEveryFrame)
                RefreshPatrol();
        }

        public void RefreshPatrol()
        {
            if (patrol == null) return;

            bool target = ResolvePatrolEnabled();
            if (hasApplied && lastApplied == target) return;

            patrol.SetPatrolEnabled(target);
            lastApplied = target;
            hasApplied = true;
        }

        private bool ResolvePatrolEnabled()
        {
            if (GameManager.I == null)
                return defaultEnabled;

            if (rules != null)
            {
                for (int i = 0; i < rules.Length; i++)
                {
                    PatrolRule rule = rules[i];
                    if (rule == null) continue;
                    if (string.IsNullOrWhiteSpace(rule.requiredFlag)) continue;

                    bool hasFlag = GameManager.I.HasFlag(rule.requiredFlag);
                    bool matches = rule.invertMatch ? !hasFlag : hasFlag;
                    if (matches)
                        return rule.enablePatrol;
                }
            }

            return defaultEnabled;
        }
    }
}
