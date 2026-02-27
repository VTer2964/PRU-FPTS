using UnityEngine;

namespace MemoryCardMatch
{
    [CreateAssetMenu(fileName = "CardData", menuName = "MemoryCardMatch/Card Data")]
    public class CardData : ScriptableObject
    {
        [Header("Card Identity")]
        [Tooltip("Unique identifier for this card type")]
        public int cardId;

        [Tooltip("Display name for this card")]
        public string cardName;

        [Header("Visuals")]
        [Tooltip("Sprite shown on the front face of the card")]
        public Sprite cardSprite;

        [Header("Classification")]
        [Tooltip("Category grouping (e.g. Fruits, Animals, Colors)")]
        public string cardCategory;
    }
}
