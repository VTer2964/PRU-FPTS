using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace FlappyVocabGame
{
    [System.Serializable]
    public class VocabQuestion
    {
        public string question;
        public string correct;
        public string[] wrongs;
    }

    public class WordSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject wordPrefab;
        [SerializeField] private float spawnX = 9f;
        [SerializeField] private float yMin = -3f;
        [SerializeField] private float yMax = 3f;
        [SerializeField] private float minYGap = 1.5f;
        [SerializeField] private float baseWordSpeed = 4f;
        [SerializeField] private float delayBetweenSets = 1.5f;

        [Header("UI")]
        [SerializeField] private TMP_Text questionText;

        private VocabQuestion[] questions;
        private VocabQuestion currentQuestion;
        private Coroutine spawnRoutine;
        private int correctAnswersCount;
        private bool isSpawning;

        private void Awake()
        {
            BuildDatabase();
        }

        private void BuildDatabase()
        {
            questions = new VocabQuestion[]
            {
                // Vietnamese → English
                new VocabQuestion { question = "Translate: CON MÈO", correct = "CAT",    wrongs = new[]{"DOG","BIRD","FISH"} },
                new VocabQuestion { question = "Translate: CON CHÓ", correct = "DOG",    wrongs = new[]{"CAT","COW","PIG"} },
                new VocabQuestion { question = "Translate: CON CHIM", correct = "BIRD",  wrongs = new[]{"CAT","FISH","BEE"} },
                new VocabQuestion { question = "Translate: MẶT TRỜI", correct = "SUN",   wrongs = new[]{"MOON","STAR","CLOUD"} },
                new VocabQuestion { question = "Translate: NƯỚC", correct = "WATER",     wrongs = new[]{"FIRE","AIR","EARTH"} },
                new VocabQuestion { question = "Translate: LỬA", correct = "FIRE",       wrongs = new[]{"WATER","ICE","SMOKE"} },
                new VocabQuestion { question = "Translate: NHÀ", correct = "HOUSE",      wrongs = new[]{"ROAD","TREE","DOOR"} },
                new VocabQuestion { question = "Translate: SÁCH", correct = "BOOK",      wrongs = new[]{"PEN","BAG","TABLE"} },
                new VocabQuestion { question = "Translate: MÀU ĐỎ", correct = "RED",     wrongs = new[]{"BLUE","GREEN","PINK"} },
                new VocabQuestion { question = "Translate: MÀU XANH", correct = "BLUE",  wrongs = new[]{"RED","GRAY","BROWN"} },
                new VocabQuestion { question = "Translate: MÀU VÀNG", correct = "YELLOW",wrongs = new[]{"ORANGE","PURPLE","WHITE"} },
                // Antonyms
                new VocabQuestion { question = "Antonym of HOT",      correct = "COLD",  wrongs = new[]{"WARM","COOL","NICE"} },
                new VocabQuestion { question = "Antonym of BIG",      correct = "SMALL", wrongs = new[]{"LARGE","HUGE","TALL"} },
                new VocabQuestion { question = "Antonym of HAPPY",    correct = "SAD",   wrongs = new[]{"MAD","GLAD","BOLD"} },
                new VocabQuestion { question = "Antonym of FAST",     correct = "SLOW",  wrongs = new[]{"QUICK","SWIFT","BUSY"} },
                new VocabQuestion { question = "Antonym of LIGHT",    correct = "DARK",  wrongs = new[]{"BRIGHT","GLOW","SOFT"} },
                new VocabQuestion { question = "Antonym of OPEN",     correct = "CLOSE", wrongs = new[]{"LOCK","SHUT","SEAL"} },
                // Synonyms
                new VocabQuestion { question = "Synonym of HAPPY",    correct = "JOYFUL",wrongs = new[]{"ANGRY","BORED","TIRED"} },
                new VocabQuestion { question = "Synonym of SMALL",    correct = "TINY",  wrongs = new[]{"HUGE","WIDE","TALL"} },
                new VocabQuestion { question = "Synonym of ANGRY",    correct = "MAD",   wrongs = new[]{"CALM","GLAD","NICE"} },
                new VocabQuestion { question = "Synonym of BRAVE",    correct = "BOLD",  wrongs = new[]{"WEAK","SLOW","DULL"} },
            };
        }

        public void StartSpawning()
        {
            isSpawning = true;
            correctAnswersCount = 0;
            PickRandomQuestion();
            spawnRoutine = StartCoroutine(SpawnLoop());
        }

        public void StopSpawning()
        {
            isSpawning = false;
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
        }

        public void SpawnNextQuestion()
        {
            correctAnswersCount++;
            ClearAllWords();
            PickRandomQuestion();
        }

        private void ClearAllWords()
        {
            foreach (var w in FindObjectsByType<VocabWordBehaviour>(FindObjectsSortMode.None))
                Destroy(w.gameObject);
        }

        private void PickRandomQuestion()
        {
            currentQuestion = questions[Random.Range(0, questions.Length)];
            if (questionText != null)
                questionText.text = currentQuestion.question;
        }

        private IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(0.5f);
            while (isSpawning)
            {
                ClearAllWords();
                SpawnCurrentSet();
                yield return new WaitForSeconds(delayBetweenSets);
            }
        }

        private void SpawnCurrentSet()
        {
            if (currentQuestion == null || wordPrefab == null) return;

            // Build answer pool: 1 correct + up to 2 wrongs
            List<(string word, bool isCorrect)> pool = new List<(string, bool)>
            {
                (currentQuestion.correct, true)
            };

            int wrongCount = Mathf.Min(2, currentQuestion.wrongs.Length);
            List<string> wrongPool = new List<string>(currentQuestion.wrongs);
            for (int i = 0; i < wrongCount; i++)
            {
                int idx = Random.Range(0, wrongPool.Count);
                pool.Add((wrongPool[idx], false));
                wrongPool.RemoveAt(idx);
            }

            // Shuffle pool
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            // Assign Y positions with min gap
            float currentSpeed = baseWordSpeed + correctAnswersCount * 0.1f;
            List<float> usedY = new List<float>();

            foreach (var (word, isCorrect) in pool)
            {
                float y = GetValidY(usedY);
                usedY.Add(y);

                Vector3 pos = new Vector3(spawnX, y, 0f);
                GameObject obj = Instantiate(wordPrefab, pos, Quaternion.identity);
                VocabWordBehaviour behaviour = obj.GetComponent<VocabWordBehaviour>();
                if (behaviour != null)
                {
                    behaviour.Word = word;
                    behaviour.IsCorrect = isCorrect;
                    behaviour.Speed = currentSpeed;
                }
            }
        }

        private float GetValidY(List<float> usedY)
        {
            int maxAttempts = 20;
            for (int i = 0; i < maxAttempts; i++)
            {
                float candidate = Random.Range(yMin, yMax);
                bool valid = true;
                foreach (float used in usedY)
                {
                    if (Mathf.Abs(candidate - used) < minYGap)
                    {
                        valid = false;
                        break;
                    }
                }
                if (valid) return candidate;
            }
            // Fallback: evenly distribute
            return usedY.Count == 0 ? 0f : usedY[usedY.Count - 1] + minYGap;
        }
    }
}
