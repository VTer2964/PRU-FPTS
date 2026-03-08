using System.Collections;
using UnityEngine;

namespace FlappyVocabGame
{
    /// <summary>
    /// Bird dùng Rigidbody2D physics + CircleCollider2D.
    /// Collision xử lý qua OnTriggerEnter2D khi chạm gate (VocabWordBehaviour).
    /// Tham khảo: VocabFlappyBird/Scripts/Player/PlayerController.cs
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BirdController : MonoBehaviour
    {
        [Header("Physics")]
        [SerializeField] private float flapForce    =  8f;
        [SerializeField] private float maxFallSpeed = -14f;
        [SerializeField] private float yMin         = -4.5f;
        [SerializeField] private float yMax         =  4.5f;

        [Header("Tilt")]
        [SerializeField] private float tiltUp   =  30f;
        [SerializeField] private float tiltDown = -60f;

        [Header("Sprites")]
        [SerializeField] private Sprite[] flyFrames;
        [SerializeField] private float    animFPS = 10f;

        [Header("Invincibility (wrong answer)")]
        [SerializeField] private float invincibilityDuration = 0.8f;
        [SerializeField] private float flashInterval         = 0.1f;

        [Header("References")]
        [SerializeField] private FlappyVocabGameManager gameManager;

        private Rigidbody2D      rb;
        private SpriteRenderer   sr;
        private CircleCollider2D col;
        private bool             active;
        private bool             isInvincible;

        // ──────────────────────────────────────────────────────────
        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();

            if (gameManager == null)
                gameManager = FindFirstObjectByType<FlappyVocabGameManager>();

            // Thêm Rigidbody2D nếu chưa có
            rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale           = 3f;
            rb.constraints            = RigidbodyConstraints2D.FreezeRotation;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.simulated              = false; // tắt đến khi SetActive(true)

            // Thêm CircleCollider2D nếu chưa có
            col = GetComponent<CircleCollider2D>();
            if (col == null) col = gameObject.AddComponent<CircleCollider2D>();
            col.radius    = 0.28f;
            col.isTrigger = false; // Bird là solid; gate là trigger

            // Xóa BoxCollider2D/collider cũ nếu còn
            foreach (var bc in GetComponents<BoxCollider2D>()) Destroy(bc);

            EnsureSprite();
        }

        private void EnsureSprite()
        {
            if (sr.sprite != null) return;
            if (flyFrames != null && flyFrames.Length > 0 && flyFrames[0] != null)
                { sr.sprite = flyFrames[0]; return; }
            sr.sprite = MakeCircleSprite(Color.yellow);
        }

        private static Sprite MakeCircleSprite(Color c)
        {
            const int S = 64;
            var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
            float r = S / 2f;
            for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                float dx = x - r, dy = y - r;
                tex.SetPixel(x, y, dx*dx+dy*dy <= r*r ? c : Color.clear);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0,0,S,S), new Vector2(.5f,.5f), S);
        }

        // ──────────────────────────────────────────────────────────
        public void SetActive(bool enable)
        {
            active           = enable;
            rb.simulated     = enable;
            isInvincible     = false;
            sr.enabled       = true;
            if (!enable)
            {
                rb.linearVelocity  = Vector2.zero;
                transform.rotation = Quaternion.identity;
            }
        }

        // ──────────────────────────────────────────────────────────
        private void Update()
        {
            Animate();
            if (!active) return;

            HandleInput();
            ClampVelocityAndPosition();
            UpdateTilt();
        }

        private void Animate()
        {
            if (flyFrames == null || flyFrames.Length == 0) return;
            int i = (int)(Time.time * animFPS) % flyFrames.Length;
            if (flyFrames[i] != null) sr.sprite = flyFrames[i];
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                rb.linearVelocity = new Vector2(0f, flapForce);
        }

        private void ClampVelocityAndPosition()
        {
            var vel = rb.linearVelocity;
            if (vel.y < maxFallSpeed) { vel.y = maxFallSpeed; rb.linearVelocity = vel; }

            var p = rb.position;
            if (p.y < yMin) { p.y = yMin; rb.linearVelocity = new Vector2(vel.x, 0f); rb.position = p; }
            if (p.y > yMax) { p.y = yMax; rb.linearVelocity = new Vector2(vel.x, 0f); rb.position = p; }
        }

        private void UpdateTilt()
        {
            float t     = Mathf.InverseLerp(maxFallSpeed, flapForce, rb.linearVelocity.y);
            float angle = Mathf.Lerp(tiltDown, tiltUp, t);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // ──────────────────────────────────────────────────────────
        /// <summary>
        /// Unity gọi khi bird (CircleCollider2D) chạm vào trigger collider của gate.
        /// Điều kiện: gate phải có isTrigger=true, bird phải có Rigidbody2D.
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!active || isInvincible) return;

            var gate = other.GetComponent<VocabWordBehaviour>();
            if (gate == null || gate.HasBeenUsed) return;

            gate.HasBeenUsed = true;

            if (gate.IsCorrect)
            {
                gate.ShowCorrectFeedback();
                gameManager?.OnCorrectAnswer();
            }
            else
            {
                gate.ShowWrongFeedback();
                gameManager?.OnWrongAnswer();
                StartCoroutine(InvincibilityCoroutine());
            }
        }

        private IEnumerator InvincibilityCoroutine()
        {
            isInvincible = true;
            float elapsed = 0f;
            while (elapsed < invincibilityDuration)
            {
                sr.enabled = !sr.enabled;
                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;
            }
            sr.enabled   = true;
            isInvincible = false;
        }
    }
}
