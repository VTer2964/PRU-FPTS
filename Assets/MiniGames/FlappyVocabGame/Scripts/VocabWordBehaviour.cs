using UnityEngine;
using TMPro;

namespace FlappyVocabGame
{
    public class VocabWordBehaviour : MonoBehaviour
    {
        public string Word;
        public bool IsCorrect;
        public float Speed = 4f;

        [SerializeField] private TMP_Text label;
        [SerializeField] private SpriteRenderer background;

        [Header("Colors")]
        [SerializeField] private Color correctColor = new Color(0.3f, 0.9f, 0.3f);
        [SerializeField] private Color wrongColor   = new Color(0.9f, 0.3f, 0.3f);

        private void Start()
        {
            if (label != null)
                label.text = Word;

            // Tint background màu xanh (correct) hoặc đỏ (wrong) để làm hint nhẹ
            // Chỉ show màu mờ — player vẫn cần đọc câu hỏi để biết từ nào đúng
            if (background != null)
            {
                Color tint = IsCorrect ? correctColor : wrongColor;
                tint.a = 0.15f;  // rất mờ, không spoil đáp án quá lộ
                background.color = tint;
            }
        }

        private void Update()
        {
            transform.Translate(Vector3.left * Speed * Time.deltaTime);
            if (transform.position.x < -12f)
                Destroy(gameObject);
        }
    }
}
