using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CountryGuessGame.Data
{
    /// <summary>
    /// ScriptableObject database storing all countries for the game
    /// </summary>
    [CreateAssetMenu(fileName = "CountryDatabase", menuName = "Country Guess Game/Country Database")]
    public class CountryDatabase : ScriptableObject
    {
        [SerializeField] private List<CountryData> allCountries = new List<CountryData>();

        public List<CountryData> AllCountries => allCountries;

        /// <summary>
        /// Get random countries for a level
        /// </summary>
        public List<CountryData> GetRandomCountries(int count, int maxDifficulty = 3)
        {
            // Filter by difficulty
            var availableCountries = allCountries.Where(c => c.Difficulty <= maxDifficulty).ToList();

            if (availableCountries.Count < count)
            {
                Debug.LogWarning($"Not enough countries available! Requested: {count}, Available: {availableCountries.Count}");
                return availableCountries;
            }

            // Shuffle and take
            var shuffled = availableCountries.OrderBy(x => Random.value).Take(count).ToList();
            return shuffled;
        }

        /// <summary>
        /// Get countries by region
        /// </summary>
        public List<CountryData> GetCountriesByRegion(string region, int count)
        {
            var regionCountries = allCountries.Where(c => c.Region.Equals(region, System.StringComparison.OrdinalIgnoreCase)).ToList();
            return regionCountries.OrderBy(x => Random.value).Take(count).ToList();
        }

        /// <summary>
        /// Get countries by difficulty level
        /// </summary>
        public List<CountryData> GetCountriesByDifficulty(int difficulty, int count)
        {
            var difficultyCountries = allCountries.Where(c => c.Difficulty == difficulty).ToList();
            return difficultyCountries.OrderBy(x => Random.value).Take(count).ToList();
        }

        /// <summary>
        /// Get distractor countries (wrong answers) that are different from the correct one
        /// </summary>
        public List<CountryData> GetDistractorCountries(CountryData correctCountry, int count, int maxDifficulty = 3)
        {
            // Get countries from the same difficulty range but exclude the correct one
            var availableCountries = allCountries
                .Where(c => c != correctCountry && c.Difficulty <= maxDifficulty)
                .ToList();

            if (availableCountries.Count < count)
            {
                Debug.LogWarning($"Not enough distractor countries! Requested: {count}, Available: {availableCountries.Count}");
                return availableCountries;
            }

            // Prioritize countries from the same region for harder difficulty
            var sameRegion = availableCountries.Where(c => c.Region == correctCountry.Region).ToList();
            var otherRegion = availableCountries.Where(c => c.Region != correctCountry.Region).ToList();

            List<CountryData> distractors = new List<CountryData>();

            // Add some from same region (if difficulty is high) to make it harder
            if (correctCountry.Difficulty >= 2 && sameRegion.Count > 0)
            {
                int sameRegionCount = Mathf.Min(2, sameRegion.Count);
                distractors.AddRange(sameRegion.OrderBy(x => Random.value).Take(sameRegionCount));
            }

            // Fill the rest with random countries
            int remaining = count - distractors.Count;
            if (remaining > 0)
            {
                var remainingPool = availableCountries.Except(distractors).ToList();
                distractors.AddRange(remainingPool.OrderBy(x => Random.value).Take(remaining));
            }

            return distractors.Take(count).ToList();
        }

        /// <summary>
        /// Add a new country (for editor use)
        /// </summary>
        public void AddCountry(CountryData country)
        {
            if (!allCountries.Contains(country))
            {
                allCountries.Add(country);
            }
        }

        /// <summary>
        /// Get total country count
        /// </summary>
        public int GetCountryCount()
        {
            return allCountries.Count;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Clear all countries (for editor use)
        /// </summary>
        [ContextMenu("Clear All Countries")]
        public void ClearAllCountries()
        {
            allCountries.Clear();
            UnityEditor.EditorUtility.SetDirty(this);
            Debug.Log("All countries cleared!");
        }
#endif
    }
}
