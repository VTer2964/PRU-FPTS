using UnityEngine;

namespace StackTower
{
    /// <summary>
    /// Handles the math and GameObject creation for slicing a moving block
    /// against the current tower top.
    /// </summary>
    public static class BlockSlicer
    {
        // ── Data Types ───────────────────────────────────────────────────────

        public enum SliceResultType
        {
            Perfect,    // Overlap within perfect threshold → snap, no cut
            Partial,    // Partial overlap → cut and keep overlap
            Miss        // Zero overlap → game over
        }

        public struct SliceResult
        {
            public SliceResultType type;
            /// <summary>The GameObject that remains on the tower (already repositioned/scaled).</summary>
            public GameObject keptBlock;
            /// <summary>The cut-off piece that should fall (may be null on Perfect or Miss).</summary>
            public GameObject fallenBlock;
            /// <summary>New size of the kept block on the moving axis.</summary>
            public float newSize;
            /// <summary>Overlap length on the moving axis.</summary>
            public float overlapAmount;
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Attempts to slice <paramref name="movingBlock"/> against <paramref name="topBlock"/>.
        /// Modifies the movingBlock transform in-place for the kept part and
        /// instantiates a new GameObject for the fallen part.
        /// </summary>
        /// <param name="movingBlock">The player-stopped block (will become the kept portion).</param>
        /// <param name="topBlock">The current top of the tower.</param>
        /// <param name="movingOnX">True if this block was moving along the X axis.</param>
        /// <param name="perfectThreshold">Max offset to be counted as Perfect.</param>
        /// <param name="fallenBlockMaterial">Material assigned to fallen piece.</param>
        public static SliceResult Slice(
            Transform movingBlock,
            Transform topBlock,
            bool movingOnX,
            float perfectThreshold,
            Material fallenBlockMaterial)
        {
            // Moving block info
            Vector3 movPos = movingBlock.position;
            Vector3 movScale = movingBlock.localScale;

            // Top block info
            Vector3 topPos = topBlock.position;
            Vector3 topScale = topBlock.localScale;

            // ── Calculate offset and overlap on the movement axis ────────────
            float movingCoord  = movingOnX ? movPos.x  : movPos.z;
            float topCoord     = movingOnX ? topPos.x  : topPos.z;
            float movingSize   = movingOnX ? movScale.x : movScale.z;
            float topSize      = movingOnX ? topScale.x : topScale.z;

            float offset  = movingCoord - topCoord;
            float absOff  = Mathf.Abs(offset);

            float topLeft     = topCoord  - topSize  * 0.5f;
            float topRight    = topCoord  + topSize  * 0.5f;
            float movLeft     = movingCoord - movingSize * 0.5f;
            float movRight    = movingCoord + movingSize * 0.5f;

            float overlapMin  = Mathf.Max(topLeft, movLeft);
            float overlapMax  = Mathf.Min(topRight, movRight);
            float overlap     = overlapMax - overlapMin;

            SliceResult result = new SliceResult();

            // ── Miss: no overlap at all ──────────────────────────────────────
            if (overlap <= 0f)
            {
                result.type = SliceResultType.Miss;
                result.keptBlock = null;
                result.fallenBlock = null;
                result.overlapAmount = 0f;
                result.newSize = 0f;
                return result;
            }

            // ── Perfect: within threshold → snap to top block ────────────────
            if (absOff <= perfectThreshold)
            {
                // Snap position to match the top block perfectly
                if (movingOnX)
                    movingBlock.position = new Vector3(topPos.x, movPos.y, movPos.z);
                else
                    movingBlock.position = new Vector3(movPos.x, movPos.y, topPos.z);

                result.type = SliceResultType.Perfect;
                result.keptBlock = movingBlock.gameObject;
                result.fallenBlock = null;
                result.overlapAmount = movingSize; // full size preserved
                result.newSize = movingSize;
                return result;
            }

            // ── Partial: slice the block ─────────────────────────────────────
            float overlapCenter = (overlapMin + overlapMax) * 0.5f;
            float fallenSize    = movingSize - overlap;
            float fallenCenter  = offset > 0f
                ? movRight - fallenSize * 0.5f
                : movLeft  + fallenSize * 0.5f;

            // Resize the kept (moving) block
            if (movingOnX)
            {
                movingBlock.localScale   = new Vector3(overlap, movScale.y, movScale.z);
                movingBlock.position     = new Vector3(overlapCenter, movPos.y, movPos.z);
            }
            else
            {
                movingBlock.localScale   = new Vector3(movScale.x, movScale.y, overlap);
                movingBlock.position     = new Vector3(movPos.x, movPos.y, overlapCenter);
            }

            // Create the fallen piece
            GameObject fallen = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fallen.name = "FallenPiece";

            if (movingOnX)
            {
                fallen.transform.localScale = new Vector3(fallenSize, movScale.y, movScale.z);
                fallen.transform.position   = new Vector3(fallenCenter, movPos.y, movPos.z);
            }
            else
            {
                fallen.transform.localScale = new Vector3(movScale.x, movScale.y, fallenSize);
                fallen.transform.position   = new Vector3(movPos.x, movPos.y, fallenCenter);
            }

            // Assign material
            var renderer = fallen.GetComponent<Renderer>();
            if (renderer != null && fallenBlockMaterial != null)
                renderer.material = fallenBlockMaterial;

            // Add physics so it falls
            var rb = fallen.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.AddTorque(Random.insideUnitSphere * 2f, ForceMode.Impulse);

            result.type          = SliceResultType.Partial;
            result.keptBlock     = movingBlock.gameObject;
            result.fallenBlock   = fallen;
            result.overlapAmount = overlap;
            result.newSize       = overlap;
            return result;
        }
    }
}
