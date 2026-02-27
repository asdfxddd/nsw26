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

    public float BaseAttack => baseAttack;

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
    }
}
