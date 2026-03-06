using System.Collections.Generic;
using UnityEngine;

namespace StackTower
{
    /// <summary>
    /// Manages the stack of placed blocks that form the tower.
    /// </summary>
    public class TowerBuilder : MonoBehaviour
    {
        // ── State ────────────────────────────────────────────────────────────
        private readonly List<Transform> _placedBlocks = new List<Transform>();
        private float _blockHeight = 0.4f;

        // ── Public API ───────────────────────────────────────────────────────

        public int FloorCount => _placedBlocks.Count;

        /// <summary>World Y position of the top surface of the topmost block.</summary>
        public float TopSurfaceY
        {
            get
            {
                if (_placedBlocks.Count == 0) return 0f;
                var top = _placedBlocks[_placedBlocks.Count - 1];
                return top.position.y + top.localScale.y * 0.5f;
            }
        }

        public Transform GetTopBlock()
        {
            if (_placedBlocks.Count == 0) return null;
            return _placedBlocks[_placedBlocks.Count - 1];
        }

        public void Initialize(float blockHeight)
        {
            _blockHeight = blockHeight;
            ClearTower();
        }

        /// <summary>
        /// Registers the base (floor 0) static block that the tower starts on.
        /// </summary>
        public void SetBaseBlock(Transform baseBlock)
        {
            _placedBlocks.Clear();
            _placedBlocks.Add(baseBlock);
        }

        /// <summary>
        /// Adds a newly sliced/placed block to the tower.
        /// </summary>
        public void AddBlock(Transform block)
        {
            _placedBlocks.Add(block);
        }

        /// <summary>
        /// Returns the world-space Y center position where the NEXT new block should be placed.
        /// </summary>
        public float GetNextBlockY()
        {
            return TopSurfaceY + _blockHeight * 0.5f;
        }

        /// <summary>
        /// Destroys all placed blocks and resets the tower.
        /// </summary>
        public void ClearTower()
        {
            foreach (var b in _placedBlocks)
            {
                if (b != null)
                    Destroy(b.gameObject);
            }
            _placedBlocks.Clear();
        }
    }
}
