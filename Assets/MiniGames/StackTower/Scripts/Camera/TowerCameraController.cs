using System.Collections;
using UnityEngine;

namespace StackTower
{
    /// <summary>
    /// Controls the orthographic camera with an isometric-angled view.
    /// As the tower grows, the camera tracks upward so the full tower height is visible.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class TowerCameraController : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("Settings")]
        [SerializeField] private StackTowerSettings settings;

        [Header("Target Override (optional)")]
        [Tooltip("X/Z center of the tower. Leave 0,0 for world origin.")]
        [SerializeField] private Vector2 lookAtCenter = Vector2.zero;

        // ── Private ──────────────────────────────────────────────────────────
        private Camera _cam;
        private float _targetOrthoSize;
        private float _towerTopY;
        private float _sizeVelocity;
        private Vector3 _posVelocity;

        // Shake state
        private bool _shaking;

        // ── Unity ────────────────────────────────────────────────────────────
        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;

            if (settings == null)
                settings = Resources.Load<StackTowerSettings>("StackTower/StackTowerSettings");

            if (settings != null)
            {
                _targetOrthoSize = settings.cameraBaseOrthoSize;
                _cam.orthographicSize = _targetOrthoSize;
            }

            _towerTopY = 0f;
            SnapCameraToTarget();
        }

        private void LateUpdate()
        {
            float smooth = settings != null ? settings.cameraSmoothSpeed : 4f;

            // Smooth ortho size
            _cam.orthographicSize = Mathf.SmoothDamp(
                _cam.orthographicSize, _targetOrthoSize,
                ref _sizeVelocity, 1f / smooth);

            if (!_shaking)
            {
                // Smooth position toward angled target
                Vector3 targetPos = GetTargetPosition();
                transform.position = Vector3.SmoothDamp(
                    transform.position, targetPos,
                    ref _posVelocity, 1f / smooth);

                // Always look at tower mid-point
                transform.LookAt(GetLookAtPoint());
            }
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>Call this every time a new floor is placed.</summary>
        public void UpdateTarget(float nextBlockY, int floorCount)
        {
            if (settings == null) return;
            _towerTopY = nextBlockY;
            _targetOrthoSize = settings.cameraBaseOrthoSize + settings.cameraSizePerFloor * floorCount;
        }

        /// <summary>Instantly snaps camera back to starting state (call on level start/restart).</summary>
        public void ResetCamera()
        {
            StopAllCoroutines();
            _shaking = false;

            if (settings == null) return;
            _towerTopY = 0f;
            _targetOrthoSize = settings.cameraBaseOrthoSize;
            _cam.orthographicSize = _targetOrthoSize;

            SnapCameraToTarget();
        }

        /// <summary>Briefly shakes the camera (game-over effect).</summary>
        public void TriggerShake(float magnitude, float duration)
        {
            if (_shaking) return;
            StartCoroutine(ShakeRoutine(magnitude, duration));
        }

        // ── Private ──────────────────────────────────────────────────────────

        /// <summary>
        /// World-space point the camera looks at: 40% up from tower base.
        /// </summary>
        private Vector3 GetLookAtPoint()
        {
            float lookY = _towerTopY * 0.4f;
            return new Vector3(lookAtCenter.x, lookY, lookAtCenter.y);
        }

        /// <summary>
        /// Computes camera world position using elevation/azimuth/distance from settings.
        /// </summary>
        private Vector3 GetTargetPosition()
        {
            if (settings == null) return Vector3.up * 20f;

            float elevRad = settings.cameraElevationDeg * Mathf.Deg2Rad;
            float azimRad = settings.cameraAzimuthDeg   * Mathf.Deg2Rad;
            float dist    = settings.cameraViewDistance;

            // Spherical coordinate offset around the look-at point
            Vector3 offset = new Vector3(
                -Mathf.Sin(azimRad) * Mathf.Cos(elevRad) * dist,
                 Mathf.Sin(elevRad) * dist,
                -Mathf.Cos(azimRad) * Mathf.Cos(elevRad) * dist
            );

            return GetLookAtPoint() + offset;
        }

        private void SnapCameraToTarget()
        {
            transform.position = GetTargetPosition();
            transform.LookAt(GetLookAtPoint());
        }

        private IEnumerator ShakeRoutine(float magnitude, float duration)
        {
            _shaking = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                Vector3 basePos = GetTargetPosition();
                float x = Random.Range(-1f, 1f) * magnitude;
                float z = Random.Range(-1f, 1f) * magnitude;
                transform.position = new Vector3(basePos.x + x, basePos.y, basePos.z + z);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _shaking = false;
        }
    }
}
