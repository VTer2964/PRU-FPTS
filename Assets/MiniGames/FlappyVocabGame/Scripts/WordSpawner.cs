using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace FlappyVocabGame
{
    [System.Serializable]
    public class VocabQuestion
    {
        public string   question;
        public string   correct;
        public string[] wrongs;
    }

    public class WordSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject wordPrefab;
        [SerializeField] private float spawnX           =  9f;
        [SerializeField] private float yMin             = -3f;
        [SerializeField] private float yMax             =  3f;
        [SerializeField] private float minYGap          =  1.5f;
        [SerializeField] private float baseSpeed        =  4f;
        [SerializeField] private float delayBetweenSets =  2f;

        [Header("UI (auto-create nếu để trống)")]
        [SerializeField] private TMP_Text questionText;

        private VocabQuestion[] _questions;
        private VocabQuestion   _current;
        private Coroutine       _loop;
        private int             _correctCount;
        private bool            _spawning;

        // ──────────────────────────────────────────────────────────
        private void Awake()
        {
            BuildDatabase();
        }

        private void Start()
        {
            EnsureQuestionText();
        }

        // ──────────────────────────────────────────────────────────
        public void StartSpawning()
        {
            _spawning     = true;
            _correctCount = 0;
            PickQuestion();
            _loop = StartCoroutine(SpawnLoop());
        }

        public void StopSpawning()
        {
            _spawning = false;
            if (_loop != null) { StopCoroutine(_loop); _loop = null; }
            ClearWords();
        }

        public void SpawnNextQuestion()
        {
            _correctCount++;
            ClearWords(); // All.Count → 0 → WaitUntil trong SpawnLoop tự trigger spawn set mới
        }

        // ──────────────────────────────────────────────────────────
        private IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(0.5f);
            while (_spawning)
            {
                PickQuestion();
                SpawnSet();

                // Chờ đến khi TẤT CẢ words bị destroy (hit hoặc đi qua màn hình)
                // Không dùng timer cố định — tránh clear words trước khi đến vị trí bird
                yield return new WaitUntil(() => VocabWordBehaviour.All.Count == 0 || !_spawning);

                if (!_spawning) break;
                yield return new WaitForSeconds(0.4f); // pause ngắn giữa 2 set
            }
        }

        private void SpawnSet()
        {
            if (_current == null || wordPrefab == null)
            {
                if (wordPrefab == null)
                    Debug.LogWarning("[WordSpawner] wordPrefab chưa assign trong Inspector!");
                return;
            }

            // Pool: 1 đúng + 2 sai
            var pool = new List<(string w, bool ok)> { (_current.correct, true) };
            var wrong = new List<string>(_current.wrongs);
            for (int i = 0; i < Mathf.Min(2, wrong.Count); i++)
            {
                int idx = Random.Range(0, wrong.Count);
                pool.Add((wrong[idx], false));
                wrong.RemoveAt(idx);
            }

            // Shuffle
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            float speed = baseSpeed + _correctCount * 0.15f;
            var   usedY = new List<float>();

            foreach (var (w, ok) in pool)
            {
                float y  = GetY(usedY);
                usedY.Add(y);

                var obj = Instantiate(wordPrefab, new Vector3(spawnX, y, 0f), Quaternion.identity);
                var beh = obj.GetComponent<VocabWordBehaviour>();
                if (beh != null) { beh.Word = w; beh.IsCorrect = ok; beh.Speed = speed; }
            }
        }

        private void PickQuestion()
        {
            _current = _questions[Random.Range(0, _questions.Length)];
            if (questionText) questionText.text = _current.question;
        }

        private static void ClearWords()
        {
            foreach (var w in new List<VocabWordBehaviour>(VocabWordBehaviour.All))
                if (w != null) Destroy(w.gameObject);
        }

        private float GetY(List<float> used)
        {
            for (int i = 0; i < 20; i++)
            {
                float c = Random.Range(yMin, yMax);
                bool  ok = true;
                foreach (float u in used) if (Mathf.Abs(c - u) < minYGap) { ok = false; break; }
                if (ok) return c;
            }
            return used.Count == 0 ? 0f : used[used.Count - 1] + minYGap;
        }

        // ──────────────────────────────────────────────────────────
        /// <summary>
        /// Luôn tạo/tìm world-space TextMeshPro cho question.
        /// Không dùng Canvas UGUI để tránh lỗi Canvas setup (camera, sorting, v.v).
        /// </summary>
        private void EnsureQuestionText()
        {
            // Nếu questionText đã assign (UGUI hoặc world-space) → dùng luôn
            if (questionText != null)
            {
                questionText.gameObject.SetActive(true);
                questionText.color = Color.white;
                return;
            }

            // Tìm world-space TMP đã tạo trước đó (reuse khi restart)
            var existing = GameObject.Find("_QuestionLabel");
            if (existing != null)
            {
                var t = existing.GetComponent<TextMeshPro>();
                if (t != null) { questionText = t; return; }
            }

            // Tạo mới world-space TextMeshPro — không phụ thuộc Canvas
            var node = new GameObject("_QuestionLabel");
            var tmp  = node.AddComponent<TextMeshPro>();
            tmp.alignment             = TextAlignmentOptions.Center;
            tmp.fontSize              = 2.2f;
            tmp.color                 = Color.white;
            tmp.fontStyle             = FontStyles.Bold;
            tmp.rectTransform.sizeDelta = new Vector2(16f, 2f);

            // Top-center, z âm để render trước background sprite
            node.transform.position = new Vector3(0f, 4f, -1f);

            var mr = node.GetComponent<MeshRenderer>();
            if (mr != null) mr.sortingOrder = 100;

            questionText = tmp;
            Debug.Log("[WordSpawner] Created world-space question label");
        }

        // ──────────────────────────────────────────────────────────
        private void BuildDatabase()
        {
            _questions = new[]
            {
                new VocabQuestion { question="Translate: CON MÈO",   correct="CAT",    wrongs=new[]{"DOG","BIRD","FISH"} },
                new VocabQuestion { question="Translate: CON CHÓ",   correct="DOG",    wrongs=new[]{"CAT","COW","PIG"} },
                new VocabQuestion { question="Translate: CON CHIM",  correct="BIRD",   wrongs=new[]{"CAT","FISH","BEE"} },
                new VocabQuestion { question="Translate: MẶT TRỜI",  correct="SUN",    wrongs=new[]{"MOON","STAR","CLOUD"} },
                new VocabQuestion { question="Translate: NƯỚC",      correct="WATER",  wrongs=new[]{"FIRE","AIR","EARTH"} },
                new VocabQuestion { question="Translate: LỬA",       correct="FIRE",   wrongs=new[]{"WATER","ICE","SMOKE"} },
                new VocabQuestion { question="Translate: NHÀ",       correct="HOUSE",  wrongs=new[]{"ROAD","TREE","DOOR"} },
                new VocabQuestion { question="Translate: SÁCH",      correct="BOOK",   wrongs=new[]{"PEN","BAG","TABLE"} },
                new VocabQuestion { question="Translate: MÀU ĐỎ",    correct="RED",    wrongs=new[]{"BLUE","GREEN","PINK"} },
                new VocabQuestion { question="Translate: MÀU XANH",  correct="BLUE",   wrongs=new[]{"RED","GRAY","BROWN"} },
                new VocabQuestion { question="Translate: MÀU VÀNG",  correct="YELLOW", wrongs=new[]{"ORANGE","PURPLE","WHITE"} },
                new VocabQuestion { question="Antonym of HOT",   correct="COLD",  wrongs=new[]{"WARM","COOL","NICE"} },
                new VocabQuestion { question="Antonym of BIG",   correct="SMALL", wrongs=new[]{"LARGE","HUGE","TALL"} },
                new VocabQuestion { question="Antonym of HAPPY", correct="SAD",   wrongs=new[]{"MAD","GLAD","BOLD"} },
                new VocabQuestion { question="Antonym of FAST",  correct="SLOW",  wrongs=new[]{"QUICK","SWIFT","BUSY"} },
                new VocabQuestion { question="Antonym of LIGHT", correct="DARK",  wrongs=new[]{"BRIGHT","GLOW","SOFT"} },
                new VocabQuestion { question="Antonym of OPEN",  correct="CLOSE", wrongs=new[]{"LOCK","SHUT","SEAL"} },
                new VocabQuestion { question="Synonym of HAPPY", correct="JOYFUL", wrongs=new[]{"ANGRY","BORED","TIRED"} },
                new VocabQuestion { question="Synonym of SMALL", correct="TINY",   wrongs=new[]{"HUGE","WIDE","TALL"} },
                new VocabQuestion { question="Synonym of ANGRY", correct="MAD",    wrongs=new[]{"CALM","GLAD","NICE"} },
                new VocabQuestion { question="Synonym of BRAVE", correct="BOLD",   wrongs=new[]{"WEAK","SLOW","DULL"} },
            };
        }
    }
}
