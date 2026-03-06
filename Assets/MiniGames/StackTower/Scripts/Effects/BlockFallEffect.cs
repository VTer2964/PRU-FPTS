using System.Collections;
using UnityEngine;

namespace StackTower
{
    /// <summary>
    /// Attached to a fallen block piece. Fades it out and destroys it.
    /// Optionally adds an initial random force for visual variety.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class BlockFallEffect : MonoBehaviour
    {
        [SerializeField] private float fadeDelay = 0.5f;
        [SerializeField] private float fadeDuration = 0.8f;

        private Renderer _renderer;
        private Rigidbody _rb;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            // Add a slight random push for visual interest
            if (_rb != null)
            {
                _rb.AddForce(Random.insideUnitSphere * 1.5f, ForceMode.Impulse);
                _rb.AddTorque(Random.insideUnitSphere * 3f, ForceMode.Impulse);
            }

            StartCoroutine(FadeAndDestroy());
        }

        private IEnumerator FadeAndDestroy()
        {
            yield return new WaitForSeconds(fadeDelay);

            // We need a material with alpha support
            Material mat = _renderer.material;

            // Try to enable transparency
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;

            Color startColor = mat.color;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                float t = elapsed / fadeDuration;
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                mat.color = c;
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
