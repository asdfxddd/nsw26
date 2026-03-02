using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class ExplosionEffectController : MonoBehaviour
{
    [SerializeField, Min(0.1f)]
    private float effectSize = 2f;

    [SerializeField, Min(0.05f)]
    private float effectLifeTime = 0.35f;

    [SerializeField, Range(0f, 0.3f)]
    private float startScaleRatio = 0.05f;

    private SpriteRenderer spriteRenderer;
    private Coroutine playRoutine;

    public float EffectSize => effectSize;

    public float EffectLifeTime => effectLifeTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
        }

        playRoutine = StartCoroutine(PlayEffectRoutine());
    }

    private IEnumerator PlayEffectRoutine()
    {
        transform.localScale = Vector3.one * Mathf.Max(0.01f, startScaleRatio);

        Color baseColor = spriteRenderer.color;
        baseColor.a = 1f;
        spriteRenderer.color = baseColor;

        float elapsed = 0f;
        while (elapsed < effectLifeTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / effectLifeTime);

            float currentScale = Mathf.Lerp(startScaleRatio, effectSize, t);
            transform.localScale = Vector3.one * currentScale;

            Color nextColor = baseColor;
            nextColor.a = Mathf.Lerp(1f, 0f, t);
            spriteRenderer.color = nextColor;

            yield return null;
        }

        gameObject.SetActive(false);
    }

    private void OnValidate()
    {
        effectSize = Mathf.Max(0.1f, effectSize);
        effectLifeTime = Mathf.Max(0.05f, effectLifeTime);
        startScaleRatio = Mathf.Clamp(startScaleRatio, 0f, 0.3f);
    }
}
