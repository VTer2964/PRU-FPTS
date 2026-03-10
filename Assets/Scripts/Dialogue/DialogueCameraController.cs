using Unity.Cinemachine;
using System;
using Unity.Cinemachine;
using UnityEngine;

namespace FPTSim.Dialogue
{
    [Serializable]
    public class CameraKeyEntry
    {
        public string key;

        // ✅ Base class: nhận Cinemachine Camera (CM3) + VCam cũ (CM2)
        public CinemachineVirtualCameraBase vcam;

        public Transform lookAtOverride;
    }

    public class DialogueCameraController : MonoBehaviour
    {
        [Header("Registry (cameraKey -> vcam)")]
        [SerializeField] private CameraKeyEntry[] cameras;

        [Header("Priority")]
        [SerializeField] private int basePriority = 10;
        [SerializeField] private int activePriority = 100;

        private CinemachineVirtualCameraBase activeVcam;

        private void Awake()
        {
            if (cameras == null) return;

            foreach (var c in cameras)
            {
                if (c != null && c.vcam != null)
                    c.vcam.Priority = basePriority;
            }
        }

        public bool Focus(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            key = key.Trim();

            var entry = Find(key);
            if (entry == null || entry.vcam == null) return false;

            if (activeVcam != null) activeVcam.Priority = basePriority;
            activeVcam = entry.vcam;

            if (entry.lookAtOverride != null)
                activeVcam.LookAt = entry.lookAtOverride;

            activeVcam.Priority = activePriority;
            return true;
        }

        public void Clear()
        {
            if (activeVcam != null)
                activeVcam.Priority = basePriority;

            activeVcam = null;
        }

        private CameraKeyEntry Find(string key)
        {
            if (cameras == null) return null;

            for (int i = 0; i < cameras.Length; i++)
            {
                var c = cameras[i];
                if (c == null) continue;

                if (string.Equals(c.key?.Trim(), key, StringComparison.OrdinalIgnoreCase))
                    return c;
            }

            return null;
        }
    }
}