#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using CountryGuessGame.Data;
using System.IO;

namespace CountryGuessGame.Editor
{
    /// <summary>
    /// Editor script to create sample level data assets
    /// </summary>
    public class CountryLevelDataCreator : EditorWindow
    {
        [MenuItem("Country Guess Game/Create Sample Levels")]
        public static void CreateSampleLevels()
        {
            string basePath = "Assets/CountryGuessGame/Data";
            if (!AssetDatabase.IsValidFolder(basePath))
            {
                AssetDatabase.CreateFolder("Assets/CountryGuessGame", "Data");
            }

            // Level 1 - Easy
            CreateLevel(
                levelNumber: 1,
                levelName: "Easy Countries",
                questionCount: 5,
                timePerQuestion: 30f,
                maxDifficulty: 1,
                goldThreshold: 90,
                silverThreshold: 70,
                path: $"{basePath}/Level1.asset"
            );

            // Level 2 - Medium
            CreateLevel(
                levelNumber: 2,
                levelName: "Medium Challenge",
                questionCount: 7,
                timePerQuestion: 25f,
                maxDifficulty: 2,
                goldThreshold: 90,
                silverThreshold: 70,
                path: $"{basePath}/Level2.asset"
            );

            // Level 3 - Hard
            CreateLevel(
                levelNumber: 3,
                levelName: "Hard Expert",
                questionCount: 10,
                timePerQuestion: 20f,
                maxDifficulty: 3,
                goldThreshold: 90,
                silverThreshold: 70,
                path: $"{basePath}/Level3.asset"
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Sample levels created successfully!");
        }

        private static void CreateLevel(
            int levelNumber,
            string levelName,
            int questionCount,
            float timePerQuestion,
            int maxDifficulty,
            int goldThreshold,
            int silverThreshold,
            string path
        )
        {
            CountryLevelData level = ScriptableObject.CreateInstance<CountryLevelData>();
            
            // Use reflection to set private fields
            var levelNumberField = typeof(CountryLevelData).GetField("levelNumber", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var levelNameField = typeof(CountryLevelData).GetField("levelName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var questionCountField = typeof(CountryLevelData).GetField("questionCount", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var timePerQuestionField = typeof(CountryLevelData).GetField("timePerQuestion", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var maxDifficultyField = typeof(CountryLevelData).GetField("maxDifficulty", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var goldThresholdField = typeof(CountryLevelData).GetField("goldThreshold", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var silverThresholdField = typeof(CountryLevelData).GetField("silverThreshold", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pointsPerCorrectField = typeof(CountryLevelData).GetField("pointsPerCorrect", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pointsPerWrongField = typeof(CountryLevelData).GetField("pointsPerWrong", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var answerOptionsCountField = typeof(CountryLevelData).GetField("answerOptionsCount", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            levelNumberField?.SetValue(level, levelNumber);
            levelNameField?.SetValue(level, levelName);
            questionCountField?.SetValue(level, questionCount);
            timePerQuestionField?.SetValue(level, timePerQuestion);
            maxDifficultyField?.SetValue(level, maxDifficulty);
            goldThresholdField?.SetValue(level, goldThreshold);
            silverThresholdField?.SetValue(level, silverThreshold);
            pointsPerCorrectField?.SetValue(level, 10);
            pointsPerWrongField?.SetValue(level, -5);
            answerOptionsCountField?.SetValue(level, 4);

            AssetDatabase.CreateAsset(level, path);
            EditorUtility.SetDirty(level);

            Debug.Log($"Created level: {levelName} at {path}");
        }
    }
}
#endif
