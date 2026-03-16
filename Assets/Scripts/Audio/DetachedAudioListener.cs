using UnityEngine;

namespace FPTSim.Audio
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1000)]
    [RequireComponent(typeof(Camera))]
    public class DetachedAudioListener : MonoBehaviour
    {
        [Header("Follow Target")]
        [SerializeField] private Transform positionTarget;
        [SerializeField] private Vector3 localOffset = new(0f, 1.6f, 0f);
        [SerializeField] private bool autoFindPlayer = true;
        [SerializeField] private string playerTag = "Player";

        [Header("Orientation")]
        [SerializeField] private bool useCameraRotation = true;
        [SerializeField] private Transform rotationTarget;

        private AudioListener cameraListener;
        private AudioListener proxyListener;
        private GameObject proxyObject;

        private void Awake()
        {
            if (!Application.isPlaying) return;

            cameraListener = GetComponent<AudioListener>();
            EnsureTargets();
            EnsureProxyListener();
            ApplyListenerState();
        }

        private void OnEnable()
        {
            if (!Application.isPlaying) return;

            EnsureTargets();
            EnsureProxyListener();
            ApplyListenerState();
            UpdateProxyTransform();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying) return;

            EnsureTargets();
            EnsureProxyListener();
            ApplyListenerState();

            if (proxyListener != null && proxyListener.enabled)
                UpdateProxyTransform();
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;

            if (cameraListener == null)
                cameraListener = GetComponent<AudioListener>();

            if (cameraListener != null)
                cameraListener.enabled = true;

            if (proxyListener != null)
                proxyListener.enabled = false;
        }

        private void OnDestroy()
        {
            if (proxyObject != null)
            {
                Destroy(proxyObject);
                proxyObject = null;
                proxyListener = null;
            }
        }

        private void EnsureTargets()
        {
            if (positionTarget == null && autoFindPlayer)
            {
                GameObject player = GameObject.FindGameObjectWithTag(playerTag);
                if (player != null)
                    positionTarget = player.transform;
            }

            if (rotationTarget == null)
                rotationTarget = transform;
        }

        private void EnsureProxyListener()
        {
            if (positionTarget == null || proxyListener != null) return;

            proxyObject = new GameObject($"{name}_AudioListenerProxy");
            proxyObject.hideFlags = HideFlags.HideAndDontSave;
            proxyListener = proxyObject.AddComponent<AudioListener>();
        }

        private void ApplyListenerState()
        {
            bool canDetach = positionTarget != null && proxyListener != null;

            if (cameraListener == null)
                cameraListener = GetComponent<AudioListener>();

            if (cameraListener != null)
                cameraListener.enabled = !canDetach;

            if (proxyListener != null)
                proxyListener.enabled = canDetach;
        }

        private void UpdateProxyTransform()
        {
            Transform anchor = positionTarget != null ? positionTarget : transform;
            Transform rotationSource = useCameraRotation ? transform : (rotationTarget != null ? rotationTarget : anchor);

            proxyObject.transform.SetPositionAndRotation(
                anchor.TransformPoint(localOffset),
                rotationSource.rotation);
        }
    }
}
