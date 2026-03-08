using System.Collections;
using UnityEngine;

namespace StackTower
{
    /// <summary>
    /// Spawns blocks and drives their oscillating movement until the player taps.
    /// </summary>
    public class BlockSpawner : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────
        [Header("References")]
        [SerializeField] private StackTowerSettings settings;

        [Header("Materials")]
        [SerializeField] private Material blockMaterial;
        [SerializeField] private Material fallenBlockMaterial;

        // ── State ────────────────────────────────────────────────────────────
        private Transform _currentMovingBlock;
        private bool _movingOnX = true;           // Alternates each floor
        private float _moveSpeed;
        private float _spawnDistance;
        private float _blockSizeX;
        private float _blockSizeZ;
        private bool _isMoving;
        private float _direction = 1f;

        // ── Events ───────────────────────────────────────────────────────────
        public System.Action<BlockSlicer.SliceResult> OnBlockDropped;

        // ── Public API ───────────────────────────────────────────────────────

        public Transform CurrentBlock => _currentMovingBlock;
        public bool IsMoving => _isMoving;

        private void Awake()
        {
            if (settings == null)
                settings = Resources.Load<StackTowerSettings>("StackTower/StackTowerSettings");
        }

        public void Initialize(StackTowerSettings globalSettings)
        {
            settings = globalSettings;
        }

        /// <summary>
        /// Spawns a new moving block above the current tower top.
        /// </summary>
        /// <param name="topBlock">Current topmost placed block.</param>
        /// <param name="floorIndex">0-based index of this new block (determines X/Z axis).</param>
        /// <param name="blockSizeX">Width on X axis.</param>
        /// <param name="blockSizeZ">Width on Z axis.</param>
        /// <param name="speed">Oscillation speed (units/s).</param>
        /// <param name="blockY">World Y center position for the new block.</param>
        /// <param name="color">Color of this block.</param>
        public void SpawnBlock(
            Transform topBlock,
            int floorIndex,
            float blockSizeX,
            float blockSizeZ,
            float speed,
            float blockY,
            Color color)
        {
            // Alternate axis
            _movingOnX = (floorIndex % 2 == 0);
            _moveSpeed = speed;
            _blockSizeX = blockSizeX;
            _blockSizeZ = blockSizeZ;
            _spawnDistance = settings != null ? settings.blockSpawnDistance : 10f;
            _direction = 1f;

            // Create block primitive
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = $"Block_{floorIndex}";

            float blockH = settings != null ? settings.blockHeight : 0.4f;
            block.transform.localScale = new Vector3(blockSizeX, blockH, blockSizeZ);

            // Spawn position
            Vector3 topCenter = topBlock != null ? new Vector3(topBlock.position.x, blockY, topBlock.position.z)
                                                  : new Vector3(0f, blockY, 0f);
            if (_movingOnX)
                block.transform.position = new Vector3(topCenter.x - _spawnDistance, blockY, topCenter.z);
            else
                block.transform.position = new Vector3(topCenter.x, blockY, topCenter.z - _spawnDistance);

            // Apply material/color
            var rend = block.GetComponent<Renderer>();
            if (rend != null)
            {
                if (blockMaterial != null)
                {
                    rend.material = new Material(blockMaterial);
                    rend.material.color = color;
                }
                else
                {
                    // URP requires a URP-compatible shader; "Standard" shows as magenta in URP
                    var shader = Shader.Find("Universal Render Pipeline/Lit")
                               ?? Shader.Find("Universal Render Pipeline/Unlit")
                               ?? Shader.Find("Standard");
                    rend.material = new Material(shader);
                    rend.material.color = color;
                }
            }

            // Remove auto-added collider from blocking physics checks
            var col = block.GetComponent<BoxCollider>();
            if (col != null) col.isTrigger = true;

            _currentMovingBlock = block.transform;
            _isMoving = true;
        }

        private void Update()
        {
            if (!_isMoving || _currentMovingBlock == null) return;

            MoveBlock();
        }

        private void MoveBlock()
        {
            float step = _direction * _moveSpeed * Time.deltaTime;
            Vector3 pos = _currentMovingBlock.position;

            if (_movingOnX)
            {
                pos.x += step;
                // Bounce between bounds
                float bound = _spawnDistance;
                if (pos.x > bound) { pos.x = bound; _direction = -1f; }
                else if (pos.x < -bound) { pos.x = -bound; _direction = 1f; }
            }
            else
            {
                pos.z += step;
                float bound = _spawnDistance;
                if (pos.z > bound) { pos.z = bound; _direction = -1f; }
                else if (pos.z < -bound) { pos.z = -bound; _direction = 1f; }
            }

            _currentMovingBlock.position = pos;
        }

        /// <summary>
        /// Called when player taps. Stops the block and returns the slice result.
        /// </summary>
        public BlockSlicer.SliceResult DropBlock(Transform topBlock, float perfectThreshold)
        {
            if (!_isMoving || _currentMovingBlock == null)
                return default;

            _isMoving = false;

            var result = BlockSlicer.Slice(
                _currentMovingBlock,
                topBlock,
                _movingOnX,
                perfectThreshold,
                fallenBlockMaterial);

            // Auto-destroy fallen piece after a delay
            if (result.fallenBlock != null)
            {
                float lifetime = settings != null ? settings.fallingBlockLifetime : 2f;
                StartCoroutine(DestroyAfter(result.fallenBlock, lifetime));
            }

            // On Miss, also remove the current block
            if (result.type == BlockSlicer.SliceResultType.Miss && _currentMovingBlock != null)
            {
                // Add physics to make it fall away
                var rb = _currentMovingBlock.gameObject.AddComponent<Rigidbody>();
                rb.useGravity = true;
                float lifetime = settings != null ? settings.fallingBlockLifetime : 2f;
                StartCoroutine(DestroyAfter(_currentMovingBlock.gameObject, lifetime));
                _currentMovingBlock = null;
            }

            OnBlockDropped?.Invoke(result);
            return result;
        }

        private IEnumerator DestroyAfter(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (obj != null)
                Destroy(obj);
        }

        /// <summary>Remove the current moving block without slicing (called on game reset).</summary>
        public void DestroyCurrentBlock()
        {
            if (_currentMovingBlock != null)
            {
                Destroy(_currentMovingBlock.gameObject);
                _currentMovingBlock = null;
            }
            _isMoving = false;
        }
    }
}
