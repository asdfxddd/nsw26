using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ExplosionController : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float explosionRadius = 2f;

    [SerializeField]
    private LayerMask targetLayer;

    [SerializeField, Min(0f)]
    private float damageMultiplier = 0.5f;

    [SerializeField, Tooltip("꺼짐(false): 폭발로 사망한 몬스터는 추가 폭발을 일으키지 않음")]
    private bool chainPrevention;

    [SerializeField]
    private GameObject effectPrefab;

    [SerializeField, Range(0f, 100f)]
    private float triggerChancePercent;

    private readonly List<GameObject> effectPool = new List<GameObject>();
    private readonly Collider2D[] overlapBuffer = new Collider2D[64];

    public float ExplosionRadius => explosionRadius;

    public LayerMask TargetLayer => targetLayer;

    public float DamageMultiplier => damageMultiplier;

    public bool ChainPrevention => chainPrevention;

    public GameObject EffectPrefab => effectPrefab;

    public void ActivateExplosion(float chancePercent)
    {
        triggerChancePercent = Mathf.Clamp(chancePercent, 0f, 100f);
        enabled = triggerChancePercent > 0f;
    }

    private void Awake()
    {
        enabled = triggerChancePercent > 0f;
    }

    private void OnEnable()
    {
        DamageSystem.OnPlayerDamageApplied += HandlePlayerDamageApplied;
    }

    private void OnDisable()
    {
        DamageSystem.OnPlayerDamageApplied -= HandlePlayerDamageApplied;
    }

    private void HandlePlayerDamageApplied(DamageSystem.PlayerDamageEvent damageEvent)
    {
        if (!damageEvent.WasKilled || damageEvent.Target == null)
        {
            return;
        }

        if (damageEvent.IsExplosionDamage && !chainPrevention)
        {
            return;
        }

        if (triggerChancePercent <= 0f || Random.value > triggerChancePercent / 100f)
        {
            return;
        }

        Vector3 explosionPosition = damageEvent.Target.transform.position;
        TriggerEffect(explosionPosition);
        ApplyExplosionDamage(explosionPosition);
    }

    private void ApplyExplosionDamage(Vector3 explosionPosition)
    {
        PlayerStatus playerStatus = GetComponent<PlayerStatus>();
        if (playerStatus == null)
        {
            return;
        }

        float damage = Mathf.Max(0f, playerStatus.CurrentAttack * damageMultiplier);
        if (damage <= 0f)
        {
            return;
        }

        int hitCount = Physics2D.OverlapCircleNonAlloc(explosionPosition, explosionRadius, overlapBuffer, targetLayer);
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = overlapBuffer[i];
            if (hit == null)
            {
                continue;
            }

            DamageSystem.ApplyPlayerDamage(hit.gameObject, damage, true);
            overlapBuffer[i] = null;
        }
    }

    private void TriggerEffect(Vector3 explosionPosition)
    {
        if (effectPrefab == null)
        {
            return;
        }

        GameObject effectInstance = GetOrCreateEffect();
        if (effectInstance == null)
        {
            return;
        }

        effectInstance.transform.position = explosionPosition;
        effectInstance.transform.rotation = Quaternion.identity;
        effectInstance.SetActive(true);
    }

    private GameObject GetOrCreateEffect()
    {
        for (int i = 0; i < effectPool.Count; i++)
        {
            GameObject pooled = effectPool[i];
            if (pooled != null && !pooled.activeSelf)
            {
                return pooled;
            }
        }

        GameObject created = Instantiate(effectPrefab);
        effectPool.Add(created);
        return created;
    }

    private void OnValidate()
    {
        explosionRadius = Mathf.Max(0f, explosionRadius);
        damageMultiplier = Mathf.Max(0f, damageMultiplier);
        triggerChancePercent = Mathf.Clamp(triggerChancePercent, 0f, 100f);
    }
}
