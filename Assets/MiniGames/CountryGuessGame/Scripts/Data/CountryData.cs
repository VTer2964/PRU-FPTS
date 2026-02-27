using UnityEngine;

namespace CountryGuessGame.Data
{
    /// <summary>
    /// ScriptableObject representing a single country
    /// </summary>
    [CreateAssetMenu(fileName = "CountryData", menuName = "Country Guess Game/Country Data")]
    public class CountryData : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string countryNameEnglish;
        [SerializeField] private string countryNameVietnamese;
        
        [Header("Visual")]
        [SerializeField] private Sprite flagSprite;
        
        [Header("Metadata")]
        [SerializeField] private int difficulty = 1; // 1 = Easy, 2 = Medium, 3 = Hard
        [SerializeField] private string region; // Asia, Europe, Americas, Africa, Oceania
        [SerializeField] private string capital; // For future expansion
        
        // Properties
        public string CountryNameEnglish => countryNameEnglish;
        public string CountryNameVietnamese => countryNameVietnamese;
        public Sprite FlagSprite => flagSprite;
        public int Difficulty => difficulty;
        public string Region => region;
        public string Capital => capital;
        
        /// <summary>
        /// Get display name based on language preference
        /// </summary>
        public string GetDisplayName(bool useVietnamese = false)
        {
            return useVietnamese ? countryNameVietnamese : countryNameEnglish;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Setup country data (for editor use)
        /// </summary>
        public void Setup(string nameEn, string nameVi, int diff, string reg, string cap = "")
        {
            countryNameEnglish = nameEn;
            countryNameVietnamese = nameVi;
            difficulty = diff;
            region = reg;
            capital = cap;
            UnityEditor.EditorUtility.SetDirty(this);
        }
        
        public void SetFlagSprite(Sprite sprite)
        {
            flagSprite = sprite;
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
