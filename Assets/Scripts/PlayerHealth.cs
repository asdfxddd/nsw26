using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private float maxHP = 100f;

    [SerializeField]
    private float invincibleTime = 1f;

    [SerializeField]
    private Image hpImage;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private UnityEvent onDeath;

    private float currentHP;
    private bool isInvincible;
    private Coroutine invincibleRoutine;
    private Color originalColor = Color.white;

    public float MaxHP => maxHP;
    public float CurrentHP => currentHP;
    public bool IsInvincible => isInvincible;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (hpImage == null)
        {
            GameObject hpImageObject = GameObject.Find("HPImage");
            if (hpImageObject != null)
            {
                hpImage = hpImageObject.GetComponent<Image>();
            }
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        maxHP = Mathf.Max(1f, maxHP);
        currentHP = maxHP;
        RefreshHpUI();
    }

    public bool TryTakeDamage(float damage)
    {
        if (GameplayPauseState.IsGameplayPaused || damage <= 0f || currentHP <= 0f || isInvincible)
        {
            return false;
        }

        currentHP = Mathf.Max(0f, currentHP - damage);
        RefreshHpUI();

        if (currentHP <= 0f)
        {
            Die();
            return true;
        }

        StartInvincibility();
        return true;
    }

    public void TakeDamage(float damage)
    {
        TryTakeDamage(damage);
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || currentHP <= 0f)
        {
            return;
        }

        currentHP = Mathf.Min(maxHP, currentHP + amount);
        RefreshHpUI();
    }

    public void IncreaseMaxHP(float amount, bool healByIncrease = false)
    {
        if (amount <= 0f)
        {
            return;
        }

        maxHP += amount;
        if (healByIncrease)
        {
            currentHP = Mathf.Min(maxHP, currentHP + amount);
        }
        else
        {
            currentHP = Mathf.Min(currentHP, maxHP);
        }

        RefreshHpUI();
    }

    private void StartInvincibility()
    {
        if (invincibleRoutine != null)
        {
            StopCoroutine(invincibleRoutine);
        }

        invincibleRoutine = StartCoroutine(InvincibleRoutine());
    }

    private IEnumerator InvincibleRoutine()
    {
        isInvincible = true;

        float elapsed = 0f;
        const float blinkInterval = 0.1f;
        while (elapsed < invincibleTime)
        {
            if (spriteRenderer != null)
            {
                Color color = originalColor;
                color.a = 0.35f;
                spriteRenderer.color = color;
            }

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        isInvincible = false;
        invincibleRoutine = null;
    }

    private void Die()
    {
        if (invincibleRoutine != null)
        {
            StopCoroutine(invincibleRoutine);
            invincibleRoutine = null;
        }

        isInvincible = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        onDeath?.Invoke();
        Debug.Log("Player died. Game Over.");
    }

    private void RefreshHpUI()
    {
        if (hpImage == null)
        {
            return;
        }

        hpImage.fillAmount = maxHP > 0f ? currentHP / maxHP : 0f;
    }

    private void OnValidate()
    {
        maxHP = Mathf.Max(1f, maxHP);
        invincibleTime = Mathf.Max(0f, invincibleTime);
    }
}
