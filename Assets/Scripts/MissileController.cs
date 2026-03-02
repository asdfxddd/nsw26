using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerStatus))]
public class MissileController : MonoBehaviour
{
    [SerializeField]
    private MissileProjectileController missilePrefab;

    [SerializeField, Tooltip("100 = 100%, 150 = 150%")]
    private float damageMultiplier = 100f;

    [SerializeField, Tooltip("Missile fire interval in seconds")]
    private float coolTime = 1.5f;

    [SerializeField]
    private float speed = 8f;

    [SerializeField]
    private float spawnRadius = 0.35f;

    private readonly List<Transform> reusableTargets = new List<Transform>();

    private PlayerStatus ownerStatus;
    private int missileCount = 1;
    private float nextFireTime;
    private bool isActive;

    public void Activate(int count)
    {
        missileCount = Mathf.Max(1, count);
        isActive = true;
        nextFireTime = Mathf.Min(nextFireTime, Time.time);
    }

    private void Awake()
    {
        ownerStatus = GetComponent<PlayerStatus>();
    }

    private void Update()
    {
        if (!isActive || GameplayPauseState.IsGameplayPaused)
        {
            return;
        }

        if (missilePrefab == null || Time.time < nextFireTime)
        {
            return;
        }

        FireMissiles();
        nextFireTime = Time.time + Mathf.Max(0.01f, coolTime);
    }

    private void FireMissiles()
    {
        int spawnCount = Mathf.Max(1, missileCount);
        FillTargets(spawnCount);

        for (int i = 0; i < spawnCount; i++)
        {
            float angle = spawnCount > 1 ? (360f / spawnCount) * i : 0f;
            Vector3 offset = Quaternion.Euler(0f, 0f, angle) * Vector3.right * spawnRadius;
            Vector3 spawnPosition = transform.position + offset;

            MissileProjectileController missile = Instantiate(missilePrefab, spawnPosition, Quaternion.identity);
            Transform assignedTarget = i < reusableTargets.Count ? reusableTargets[i] : null;
            missile.Initialize(ownerStatus, assignedTarget, speed, Mathf.Max(0f, damageMultiplier) / 100f);
        }
    }

    private void FillTargets(int requiredCount)
    {
        reusableTargets.Clear();

        MonsterController[] monsters = FindObjectsByType<MonsterController>(FindObjectsSortMode.None);
        if (monsters == null || monsters.Length <= 0)
        {
            return;
        }

        List<(float sqrDistance, Transform target)> sorted = new List<(float, Transform)>(monsters.Length);
        Vector3 origin = transform.position;

        for (int i = 0; i < monsters.Length; i++)
        {
            MonsterController monster = monsters[i];
            if (monster == null)
            {
                continue;
            }

            Transform candidate = monster.transform;
            if (candidate == null)
            {
                continue;
            }

            float sqrDistance = (candidate.position - origin).sqrMagnitude;
            sorted.Add((sqrDistance, candidate));
        }

        sorted.Sort((a, b) => a.sqrDistance.CompareTo(b.sqrDistance));

        int uniqueCount = Mathf.Min(requiredCount, sorted.Count);
        for (int i = 0; i < uniqueCount; i++)
        {
            reusableTargets.Add(sorted[i].target);
        }

        int index = 0;
        while (reusableTargets.Count < requiredCount && sorted.Count > 0)
        {
            reusableTargets.Add(sorted[index % sorted.Count].target);
            index++;
        }
    }

    private void OnValidate()
    {
        damageMultiplier = Mathf.Max(0f, damageMultiplier);
        coolTime = Mathf.Max(0.01f, coolTime);
        speed = Mathf.Max(0f, speed);
        spawnRadius = Mathf.Max(0f, spawnRadius);
    }
}
