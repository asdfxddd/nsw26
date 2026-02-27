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

    private void Awake()
    {
        InitializeHealthForBattle();
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

        // 카드 Value 규칙: 125 = 1.25배(+25%).
        // 하위 호환: 10 같은 값은 +10%로 간주.
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

    private void OnValidate()
    {
        baseAttack = Mathf.Max(0f, baseAttack);
        attackUpBonusPercent = Mathf.Max(0f, attackUpBonusPercent);
        externalAttackMultiplier = Mathf.Max(0f, externalAttackMultiplier);
        baseMaxHP = Mathf.Max(1f, baseMaxHP);
        hpUpMultiplier = Mathf.Max(0.01f, hpUpMultiplier);
        currentMaxHP = Mathf.Max(1f, currentMaxHP);
        currentHP = Mathf.Clamp(currentHP, 0f, currentMaxHP);
    }
}
