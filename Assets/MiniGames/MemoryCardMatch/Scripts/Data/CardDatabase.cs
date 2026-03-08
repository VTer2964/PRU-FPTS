using System.Collections.Generic;
using UnityEngine;

namespace MemoryCardMatch
{
    [CreateAssetMenu(fileName = "CardDatabase", menuName = "MemoryCardMatch/Card Database")]
    public class CardDatabase : ScriptableObject
    {
        [Header("Card Collection")]
        [Tooltip("All available cards in the database")]
        public List<CardData> cards = new List<CardData>();

        /// <summary>
        /// Returns a random subset of unique cards from the database.
        /// </summary>
        /// <param name="count">Number of unique cards to return (pairs will be created from these)</param>
        public List<CardData> GetRandomCards(int count)
        {
            if (cards == null || cards.Count == 0)
            {
                Debug.LogWarning("[CardDatabase] No cards in database!");
                return new List<CardData>();
            }

            int clampedCount = Mathf.Clamp(count, 1, cards.Count);
            if (count > cards.Count)
            {
                Debug.LogWarning($"[CardDatabase] Requested {count} cards but only {cards.Count} available. Clamping.");
            }

            // Copy list and shuffle
            List<CardData> pool = new List<CardData>(cards);
            ShuffleList(pool);

            return pool.GetRange(0, clampedCount);
        }

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
    }
}
