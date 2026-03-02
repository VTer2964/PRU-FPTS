using UnityEngine;

namespace FlappyVocabGame
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class BirdController : MonoBehaviour
    {
        [Header("Physics")]
        [SerializeField] private float flapForce = 7f;
        [SerializeField] private float maxFallSpeed = -8f;
        [SerializeField] private float yMin = -4.5f;
        [SerializeField] private float yMax = 4.5f;

        [Header("Animation")]
        [SerializeField] private Sprite[] flyFrames;
        [SerializeField] private float animFPS = 10f;

        [Header("References")]
        [SerializeField] private FlappyVocabGameManager gameManager;
        [SerializeField] private WordSpawner wordSpawner;

        private Rigidbody2D rb;
        private SpriteRenderer sr;
        private bool inputEnabled;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
        }

        public void EnableInput(bool enabled)
        {
            inputEnabled = enabled;
            rb.linearVelocity = Vector2.zero;
        }

        private void Update()
        {
            // Sprite animation
            if (flyFrames != null && flyFrames.Length > 0 && sr != null)
            {
                int idx = (int)(Time.time * animFPS) % flyFrames.Length;
                sr.sprite = flyFrames[idx];
            }

            if (!inputEnabled) return;

            // Input
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(Vector2.up * flapForce, ForceMode2D.Impulse);
            }

            // Clamp fall speed
            if (rb.linearVelocity.y < maxFallSpeed)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, maxFallSpeed);

            // Tilt bird based on vertical velocity (like real Flappy Bird)
            float tilt = Mathf.Clamp(rb.linearVelocity.y * 4f, -90f, 30f);
            transform.rotation = Quaternion.Euler(0f, 0f, tilt);

            // Clamp Y position to screen bounds
            float clampedY = Mathf.Clamp(transform.position.y, yMin, yMax);
            if (transform.position.y != clampedY)
            {
                transform.position = new Vector3(transform.position.x, clampedY, transform.position.z);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            }
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            VocabWordBehaviour word = col.GetComponent<VocabWordBehaviour>();
            if (word == null) return;

            if (word.IsCorrect)
            {
                gameManager?.AddScore(10);
                wordSpawner?.SpawnNextQuestion();
            }
            else
            {
                gameManager?.LoseLife();
            }

            Destroy(col.gameObject);
        }
    }
}
