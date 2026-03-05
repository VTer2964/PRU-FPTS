using UnityEngine;

namespace StackTower
{
    [CreateAssetMenu(fileName = "StackTowerSettings", menuName = "StackTower/Settings")]
    public class StackTowerSettings : ScriptableObject
    {
        [Header("Block Visual")]
        [Tooltip("Height of each block (Y scale)")]
        public float blockHeight = 0.4f;
        [Tooltip("How far off-screen a new block spawns")]
        public float blockSpawnDistance = 10f;

        [Header("Camera")]
        [Tooltip("Base orthographic size at floor 0")]
        public float cameraBaseOrthoSize = 8f;
        [Tooltip("How much ortho size grows per floor (larger = zoom out faster)")]
        public float cameraSizePerFloor = 0.25f;
        [Tooltip("Smooth speed for camera position and size changes")]
        public float cameraSmoothSpeed = 4f;

        [Header("Camera Angle (Isometric View)")]
        [Tooltip("Camera elevation above horizontal in degrees. 90=top-down, 45=isometric, 0=side view")]
        public float cameraElevationDeg = 50f;
        [Tooltip("Horizontal rotation of camera around the tower (225=SW corner, classic isometric)")]
        public float cameraAzimuthDeg = 225f;
        [Tooltip("Distance from camera to the look-at point on the tower")]
        public float cameraViewDistance = 28f;

        [Header("Camera (Legacy - unused)")]
        [Tooltip("Kept for backward compatibility")]
        public float cameraHeightAboveTower = 20f;
        [Tooltip("Kept for backward compatibility")]
        public float cameraYPerFloor = 0.4f;

        [Header("Physics")]
        [Tooltip("Gravity multiplier for falling block pieces")]
        public float fallingBlockGravity = -9.8f;
        [Tooltip("Seconds before falling blocks are destroyed")]
        public float fallingBlockLifetime = 2f;

        [Header("Effects")]
        [Tooltip("Duration of the perfect flash effect (seconds)")]
        public float perfectFlashDuration = 0.3f;
        [Tooltip("Color of perfect flash overlay")]
        public Color perfectFlashColor = new Color(1f, 1f, 0.6f, 0.8f);
        [Tooltip("Scale multiplier for perfect block pop animation")]
        public float perfectScalePunch = 1.15f;

        [Header("Save Keys")]
        public string saveKeyPrefix = "StackTower_";

        /// <summary>Returns the PlayerPrefs key for stars on a given level.</summary>
        public string GetStarsKey(int levelNumber) => $"{saveKeyPrefix}Stars_L{levelNumber}";

        /// <summary>Returns the PlayerPrefs key for best score on a given level.</summary>
        public string GetBestScoreKey(int levelNumber) => $"{saveKeyPrefix}Score_L{levelNumber}";
    }
}
