using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerStatus : MonoBehaviour
{
    [SerializeField]
    private float baseAttack = 10f;

    [SerializeField, Tooltip("ATKUP 카드로 누적되는 추가 공격력 퍼센트. 예: 30 = +30%")]
    private float attackUpBonusPercent;

    [SerializeField, Tooltip("기타 강화(버프/디버프)용 곱연산 배율. 1 = 100%")]
    private float externalAttackMultiplier = 1f;

    [SerializeField]
    private float baseMaxHP = 100f;

    [SerializeField, Tooltip("HPUP 카드로 누적되는 최대 체력 배율. 1 = 100%")]
    private float hpUpMultiplier = 1f;

    [SerializeField]
    private float currentMaxHP;

    [SerializeField]
    private float currentHP;

    [SerializeField, Tooltip("HEAL 카드로 누적되는 흡혈(실제 피해 대비 회복) 퍼센트. 예: 30 = 30%")]
    private float healOnDamagePercent;

    [SerializeField, Tooltip("Split 카드로 설정되는 최대 분할 횟수")]
    private int projectileSplitCount;

    [SerializeField, Tooltip("기본 아이템 흡수 반경")]
    private float basePickupRadius = 2.5f;

    [SerializeField, Tooltip("MAGNET 카드 누적 배율. 1 = 100%")]
    private float pickupRadiusMultiplier = 1f;

    [SerializeField, Tooltip("자석으로 끌려오는 아이템의 이동 속도")]
    private float pickupMoveSpeed = 10f;

    [SerializeField, Tooltip("플레이어가 현재 보유한 코인 수")]
    private int currentCoins;

    public event Action<float> OnPickupRadiusChanged;

    public float BaseAttack => baseAttack;

    public float BaseMaxHP => baseMaxHP;

    public float CurrentAttack => Mathf.Max(0f, baseAttack) * AttackMultiplier;

    public float AttackMultiplier
    {
        get
        {
            float atkUpMultiplier = 1f + Mathf.Max(0f, attackUpBonusPercent) / 100f;
            return atkUpMultiplier * Mathf.Max(0f, externalAttackMultiplier);
        }
    }

    public float AttackUpBonusPercent => attackUpBonusPercent;

    public float HpUpMultiplier => hpUpMultiplier;

    public float CurrentMaxHP => currentMaxHP;

    public float CurrentHP => currentHP;

    public float HealOnDamagePercent => healOnDamagePercent;

    public int ProjectileSplitCount => Mathf.Max(0, projectileSplitCount);

    public float BasePickupRadius => basePickupRadius;

    public float PickupRadiusMultiplier => pickupRadiusMultiplier;

    public float CurrentPickupRadius => Mathf.Max(0.01f, basePickupRadius) * Mathf.Max(0.01f, pickupRadiusMultiplier);

    public float PickupMoveSpeed => Mathf.Max(0.01f, pickupMoveSpeed);

    public int CurrentCoins => Mathf.Max(0, currentCoins);

    private void Awake()
    {
        InitializeHealthForBattle();

        if (!TryGetComponent<PlayerMagnetCollector>(out _))
        {
            gameObject.AddComponent<PlayerMagnetCollector>();
        }
    }

    private void Start()
    {
        NotifyPickupRadiusChanged();
    }

    public void InitializeHealthForBattle()
    {
        baseMaxHP = Mathf.Max(1f, baseMaxHP);
        hpUpMultiplier = Mathf.Max(0.01f, hpUpMultiplier);

        currentMaxHP = baseMaxHP * hpUpMultiplier;
        currentHP = currentMaxHP;
    }

    public bool TryTakeDamage(float damage)
    {
        if (damage <= 0f || currentHP <= 0f)
        {
            return false;
        }

        currentHP = Mathf.Max(0f, currentHP - damage);
        return true;
    }

    public bool TryHeal(float amount)
    {
        if (amount <= 0f || currentHP <= 0f)
        {
            return false;
        }

        currentHP = Mathf.Min(currentMaxHP, currentHP + amount);
        return true;
    }

    public void IncreaseMaxHPFlat(float amount, bool healByIncrease)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentMaxHP = Mathf.Max(1f, currentMaxHP + amount);
        if (healByIncrease)
        {
            currentHP = Mathf.Min(currentMaxHP, currentHP + amount);
        }
        else
        {
            currentHP = Mathf.Min(currentHP, currentMaxHP);
        }
    }

    public void ApplyMaxHealthUpCardPercent(float cardPercent)
    {
        if (cardPercent <= 0f)
        {
            return;
        }

        float cardMultiplier = cardPercent / 100f;
        if (cardMultiplier <= 0f)
        {
            return;
        }

        hpUpMultiplier *= cardMultiplier;

        float nextMaxHP = currentMaxHP * cardMultiplier;
        float currentHpIncrease = currentHP * (cardMultiplier - 1f);

        currentMaxHP = Mathf.Max(1f, nextMaxHP);
        currentHP = Mathf.Clamp(currentHP + currentHpIncrease, 0f, currentMaxHP);
    }

    public void ApplyAttackUpCardPercent(float cardPercent)
    {
        if (cardPercent <= 0f)
        {
            return;
        }

        float bonusPercent = cardPercent >= 100f ? cardPercent - 100f : cardPercent;
        attackUpBonusPercent += Mathf.Max(0f, bonusPercent);
    }

    public void ApplyAttackMultiplier(float multiplier)
    {
        if (multiplier <= 0f)
        {
            return;
        }

        externalAttackMultiplier *= multiplier;
    }

    public void ApplyHealOnDamageCardPercent(float cardPercent)
    {
        if (cardPercent <= 0f)
        {
            return;
        }

        healOnDamagePercent += cardPercent;
    }

    public void ApplyPickupRadiusCardPercent(float cardPercent)
    {
        if (cardPercent <= 0f)
        {
            return;
        }

        float cardMultiplier = cardPercent / 100f;
        if (cardMultiplier <= 0f)
        {
            return;
        }

        pickupRadiusMultiplier *= cardMultiplier;
        NotifyPickupRadiusChanged();
    }

    public float CalculateHealFromDamage(float appliedDamage)
    {
        if (appliedDamage <= 0f || healOnDamagePercent <= 0f)
        {
            return 0f;
        }

        return appliedDamage * (healOnDamagePercent / 100f);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentCoins = Mathf.Max(0, currentCoins + amount);
    }

    public void SetProjectileSplitCount(int maxSplitCount)
    {
        projectileSplitCount = Mathf.Max(0, maxSplitCount);
    }

    private void NotifyPickupRadiusChanged()
    {
        OnPickupRadiusChanged?.Invoke(CurrentPickupRadius);
    }

    private void OnValidate()
    {
        baseAttack = Mathf.Max(0f, baseAttack);
        attackUpBonusPercent = Mathf.Max(0f, attackUpBonusPercent);
        externalAttackMultiplier = Mathf.Max(0f, externalAttackMultiplier);
        baseMaxHP = Mathf.Max(1f, baseMaxHP);
        hpUpMultiplier = Mathf.Max(0.01f, hpUpMultiplier);
        currentMaxHP = Mathf.Max(1f, currentMaxHP);
        currentHP = Mathf.Clamp(currentHP, 0f, currentMaxHP);
        healOnDamagePercent = Mathf.Max(0f, healOnDamagePercent);
        projectileSplitCount = Mathf.Max(0, projectileSplitCount);
        basePickupRadius = Mathf.Max(0.01f, basePickupRadius);
        pickupRadiusMultiplier = Mathf.Max(0.01f, pickupRadiusMultiplier);
        pickupMoveSpeed = Mathf.Max(0.01f, pickupMoveSpeed);

        NotifyPickupRadiusChanged();
    }
}
