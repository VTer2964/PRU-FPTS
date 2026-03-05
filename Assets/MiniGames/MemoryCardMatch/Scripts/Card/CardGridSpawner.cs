using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryCardMatch
{
    public class CardGridSpawner : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Prefab for a single card (must have CardController component)")]
        [SerializeField] private CardController cardPrefab;

        [Tooltip("GridLayoutGroup container where cards will be spawned")]
        [SerializeField] private GridLayoutGroup gridLayoutGroup;

        [Tooltip("Card database to pull card data from")]
        [SerializeField] private CardDatabase cardDatabase;

        // -----------------------------------------------------------------------
        // Public API
        // -----------------------------------------------------------------------

        /// <summary>
        /// Spawns a grid of cards based on the given settings.
        /// Destroys any previously spawned cards first.
        /// </summary>
        /// <returns>List of spawned CardControllers (already initialized)</returns>
        public List<CardController> SpawnGrid(DifficultySettings settings, float flipDuration)
        {
            ClearGrid();

            int rows = settings.gridRows;
            int columns = settings.gridColumns;
            int totalCards = rows * columns;
            int pairsNeeded = totalCards / 2;

            // Validate grid is even
            if (totalCards % 2 != 0)
            {
                Debug.LogWarning($"[CardGridSpawner] Grid {rows}x{columns} has odd number of cells ({totalCards}). One card will be removed.");
                totalCards--;
                pairsNeeded = totalCards / 2;
            }

            // Get unique card data from the database
            List<CardData> uniqueCards = cardDatabase.GetRandomCards(pairsNeeded);

            if (uniqueCards.Count < pairsNeeded)
            {
                Debug.LogError($"[CardGridSpawner] Not enough cards in database. Need {pairsNeeded} pairs, got {uniqueCards.Count}.");
                pairsNeeded = uniqueCards.Count;
                totalCards = pairsNeeded * 2;
            }

            // Build paired list: each card data appears twice
            List<CardData> pairedCards = new List<CardData>(totalCards);
            foreach (CardData data in uniqueCards)
            {
                pairedCards.Add(data);
                pairedCards.Add(data);
            }

            // Shuffle using Fisher-Yates
            ShuffleList(pairedCards);

            // Configure grid layout
            ConfigureGridLayout(rows, columns);

            // Spawn card GameObjects
            List<CardController> spawnedCards = new List<CardController>(totalCards);
            foreach (CardData data in pairedCards)
            {
                CardController card = Instantiate(cardPrefab, gridLayoutGroup.transform);
                card.Initialize(data, flipDuration);
                spawnedCards.Add(card);
            }

            Debug.Log($"[CardGridSpawner] Spawned {spawnedCards.Count} cards ({pairsNeeded} pairs) in a {rows}x{columns} grid.");
            return spawnedCards;
        }

        /// <summary>
        /// Destroys all card children in the grid container.
        /// </summary>
        public void ClearGrid()
        {
            if (gridLayoutGroup == null) return;

            foreach (Transform child in gridLayoutGroup.transform)
            {
                Destroy(child.gameObject);
            }
        }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Fisher-Yates shuffle for any list.
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        /// <summary>
        /// Adjusts the GridLayoutGroup constraint and cell size to match the desired grid.
        /// Uses Canvas.ForceUpdateCanvases() so the RectTransform rect is valid at runtime.
        /// </summary>
        private void ConfigureGridLayout(int rows, int columns)
        {
            if (gridLayoutGroup == null) return;

            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = columns;

            // Force layout rebuild so containerRect.rect is populated
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(
                gridLayoutGroup.GetComponent<RectTransform>());
            Canvas.ForceUpdateCanvases();

            RectTransform containerRect = gridLayoutGroup.GetComponent<RectTransform>();
            if (containerRect != null)
            {
                float spaceX  = gridLayoutGroup.spacing.x;
                float spaceY  = gridLayoutGroup.spacing.y;
                float padL    = gridLayoutGroup.padding.left;
                float padR    = gridLayoutGroup.padding.right;
                float padT    = gridLayoutGroup.padding.top;
                float padB    = gridLayoutGroup.padding.bottom;

                float w = containerRect.rect.width;
                float h = containerRect.rect.height;

                // Fallback if rect still 0 (happens when container uses stretch anchors before first frame)
                if (w <= 1f || h <= 1f)
                {
                    // Use reference resolution as base (1080×1920 minus top bar ~120, padding ~80)
                    w = 1040f;
                    h = 1680f;
                }

                float availableWidth  = w - padL - padR  - spaceX * (columns - 1);
                float availableHeight = h - padT - padB  - spaceY * (rows    - 1);

                float cellWidth  = availableWidth  / columns;
                float cellHeight = availableHeight / rows;

                // Keep cells square — use the smaller dimension
                float cellSize = Mathf.Floor(Mathf.Min(cellWidth, cellHeight));
                cellSize = Mathf.Max(cellSize, 60f); // minimum readable size

                gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
                Debug.Log($"[CardGridSpawner] Grid {rows}x{columns} → cellSize={cellSize} (container {w:F0}×{h:F0})");
            }
        }
    }
}
