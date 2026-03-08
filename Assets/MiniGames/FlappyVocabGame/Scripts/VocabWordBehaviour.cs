using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace FlappyVocabGame
{
    /// <summary>
    /// Gate từ vựng bay từ phải → trái.
    /// Có BoxCollider2D isTrigger=true để BirdController phát hiện qua OnTriggerEnter2D.
    /// Tham khảo: VocabFlappyBird/Scripts/Gates/AnswerGate.cs
    /// </summary>
    public class VocabWordBehaviour : MonoBehaviour
    {
        // ── Static registry ───────────────────────────────────────
        public static readonly HashSet<VocabWordBehaviour> All = new HashSet<VocabWordBehaviour>();

        // ── Data (gán bởi WordSpawner sau Instantiate) ────────────
        public string Word;
        public bool   IsCorrect;
        public float  Speed = 5f;

        /// <summary>
        /// Đánh dấu gate đã được bird chạm — tránh double-trigger.
        /// Reset về false khi OnEnable.
        /// </summary>
        public bool HasBeenUsed { get; set; }

        // ── References (auto-find trên prefab) ────────────────────
        [SerializeField] private TMP_Text       label;
        [SerializeField] private SpriteRenderer background;

        [Header("Colors")]
        [SerializeField] private Color defaultColor = new Color(0.25f, 0.55f, 0.95f, 0.85f);
        [SerializeField] private Color correctColor = new Color(0.1f,  0.85f, 0.1f,  1f);
        [SerializeField] private Color wrongColor   = new Color(0.9f,  0.1f,  0.1f,  1f);

        private BoxCollider2D col;

        // ──────────────────────────────────────────────────────────
        private void Awake()
        {
            if (label      == null) label      = GetComponentInChildren<TMP_Text>();
            if (background == null) background = GetComponent<SpriteRenderer>();

            // Thêm BoxCollider2D trigger nếu chưa có
            col = GetComponent<BoxCollider2D>();
            if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            // Size dựa vào sprite bounds; fallback nếu không có SpriteRenderer
            if (background != null && background.bounds.size.x > 0.1f)
            {
                var b = background.bounds;
                col.size = new Vector2(b.size.x * 0.9f, b.size.y * 0.85f);
            }
            else
            {
                col.size = new Vector2(2.0f, 0.65f);
            }
        }

        private void OnEnable()  { HasBeenUsed = false; All.Add(this); }
        private void OnDisable() => All.Remove(this);
        private void OnDestroy() => All.Remove(this);

        private void Start()
        {
            if (label      != null) label.text       = Word;
            if (background != null) background.color = defaultColor;
        }

        private void Update()
        {
            transform.Translate(Vector3.left * Speed * Time.deltaTime);
            if (transform.position.x < -12f) Destroy(gameObject);
        }

        // ── Feedback ──────────────────────────────────────────────
        public void ShowCorrectFeedback()
        {
            if (background != null) background.color = correctColor;
        }

        public void ShowWrongFeedback()
        {
            if (background != null) background.color = wrongColor;
        }
    }
}
