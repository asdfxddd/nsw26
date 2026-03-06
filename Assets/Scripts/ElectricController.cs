using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ElectricController : MonoBehaviour
{
    [SerializeField, Tooltip("전기구체 공전 반지름")]
    private float orbitRadius = 1.5f;

    [SerializeField, Tooltip("초당 회전 각도(도)")]
    private float orbitSpeed = 90f;

    [SerializeField, Tooltip("데미지 배율(1 = 100%, 0.5 = 50%)")]
    private float damageMultiplier = 0.5f;

    [SerializeField]
    private GameObject electricPrefab;

    [SerializeField, Tooltip("같은 적에게 다시 피해를 주기까지의 내부 쿨다운(초)")]
    private float contactDamageCooldown = 0.2f;

    [SerializeField]
    private string monsterLayerName = "monster";

    private readonly List<Transform> activeOrbs = new List<Transform>();
    private PlayerStatus ownerStatus;
    private float orbitAngle;

    public float OrbitRadius
    {
        get => orbitRadius;
        set => orbitRadius = Mathf.Max(0f, value);
    }

    public float OrbitSpeed
    {
        get => orbitSpeed;
        set => orbitSpeed = value;
    }

    public float DamageMultiplier
    {
        get => damageMultiplier;
        set => damageMultiplier = Mathf.Max(0f, value);
    }

    public GameObject ElectricPrefab
    {
        get => electricPrefab;
        set => electricPrefab = value;
    }

    private void Awake()
    {
        ownerStatus = GetComponent<PlayerStatus>();
        if (ownerStatus == null)
        {
            ownerStatus = gameObject.AddComponent<PlayerStatus>();
        }
    }

    private void Update()
    {
        if (activeOrbs.Count == 0)
        {
            return;
        }

        orbitAngle += orbitSpeed * Time.deltaTime;
        PositionOrbs();
    }

    public void ActivateElectric(int requestedCount)
    {
        int nextCount = Mathf.Max(activeOrbs.Count, Mathf.Max(0, requestedCount));
        SetOrbCount(nextCount);
    }

    public void SetOrbCount(int count)
    {
        int desiredCount = Mathf.Max(0, count);
        if (desiredCount > 0 && electricPrefab == null)
        {
            Debug.LogWarning("ElectricController: ElectricPrefab is not assigned.");
            return;
        }

        while (activeOrbs.Count < desiredCount)
        {
            SpawnOrb();
        }

        while (activeOrbs.Count > desiredCount)
        {
            int lastIndex = activeOrbs.Count - 1;
            Transform orb = activeOrbs[lastIndex];
            activeOrbs.RemoveAt(lastIndex);

            if (orb != null)
            {
                Destroy(orb.gameObject);
            }
        }

        PositionOrbs();
    }

    private void SpawnOrb()
    {
        GameObject orbObject = Instantiate(electricPrefab, transform);
        orbObject.name = $"ElectricOrb_{activeOrbs.Count + 1}";

        ElectricOrb orb = orbObject.GetComponent<ElectricOrb>();
        if (orb == null)
        {
            orb = orbObject.AddComponent<ElectricOrb>();
        }

        orb.Configure(ownerStatus, this, monsterLayerName, contactDamageCooldown);
        activeOrbs.Add(orbObject.transform);
    }

    private void PositionOrbs()
    {
        int count = activeOrbs.Count;
        if (count <= 0)
        {
            return;
        }

        float angleStep = 360f / count;
        for (int i = 0; i < count; i++)
        {
            Transform orb = activeOrbs[i];
            if (orb == null)
            {
                continue;
            }

            float angle = orbitAngle + (angleStep * i);
            float rad = angle * Mathf.Deg2Rad;
            Vector3 localOffset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
            orb.localPosition = localOffset;
        }
    }

    private void OnValidate()
    {
        orbitRadius = Mathf.Max(0f, orbitRadius);
        damageMultiplier = Mathf.Max(0f, damageMultiplier);
        contactDamageCooldown = Mathf.Max(0f, contactDamageCooldown);
    }

    private sealed class ElectricOrb : MonoBehaviour
    {
        private readonly Dictionary<int, float> nextDamageTimeByTarget = new Dictionary<int, float>();

        private PlayerStatus ownerStatus;
        private ElectricController controller;
        private string monsterLayerName;
        private float contactDamageCooldown;

        public void Configure(PlayerStatus status, ElectricController ownerController, string monsterLayer, float cooldown)
        {
            ownerStatus = status;
            controller = ownerController;
            monsterLayerName = monsterLayer;
            contactDamageCooldown = Mathf.Max(0f, cooldown);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDamage(other.gameObject);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryDamage(collision.gameObject);
        }

        private void TryDamage(GameObject target)
        {
            if (GameplayPauseState.IsGameplayPaused || target == null || ownerStatus == null || controller == null)
            {
                return;
            }

            if (!IsLayerName(target.layer, monsterLayerName))
            {
                return;
            }

            int targetId = target.GetInstanceID();
            float now = Time.time;
            if (nextDamageTimeByTarget.TryGetValue(targetId, out float nextTime) && now < nextTime)
            {
                return;
            }

            float damage = Mathf.Max(0f, ownerStatus.CurrentAttack * Mathf.Max(0f, controller.DamageMultiplier));
            if (damage <= 0f)
            {
                return;
            }

            float appliedDamage = DamageSystem.ApplyPlayerDamage(target, damage);
            if (appliedDamage > 0f)
            {
                nextDamageTimeByTarget[targetId] = now + contactDamageCooldown;
            }
        }

        private static bool IsLayerName(int layer, string expectedLayerName)
        {
            if (string.IsNullOrWhiteSpace(expectedLayerName))
            {
                return false;
            }

            string actualLayerName = LayerMask.LayerToName(layer);
            return string.Equals(actualLayerName, expectedLayerName, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
