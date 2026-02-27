using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField]
    private float invincibleTime = 1f;

    [SerializeField]
    private Image hpImage;

    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private UnityEvent onDeath;

    private bool isInvincible;
    private Coroutine invincibleRoutine;
    private Color originalColor = Color.white;
    private PlayerStatus playerStatus;

    public float MaxHP => playerStatus != null ? playerStatus.CurrentMaxHP : 0f;
    public float CurrentHP => playerStatus != null ? playerStatus.CurrentHP : 0f;
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

        playerStatus = GetComponent<PlayerStatus>();
        if (playerStatus == null)
        {
            playerStatus = gameObject.AddComponent<PlayerStatus>();
        }

        playerStatus.InitializeHealthForBattle();
        RefreshHpUI();
    }

    public bool TryTakeDamage(float damage)
    {
        if (playerStatus == null)
        {
            return false;
        }

        if (GameplayPauseState.IsGameplayPaused || damage <= 0f || playerStatus.CurrentHP <= 0f || isInvincible)
        {
            return false;
        }

        playerStatus.TryTakeDamage(damage);
        RefreshHpUI();

        if (playerStatus.CurrentHP <= 0f)
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
        if (playerStatus == null)
        {
            return;
        }

        if (playerStatus.TryHeal(amount))
        {
            RefreshHpUI();
        }
    }

    public void IncreaseMaxHP(float amount, bool healByIncrease = false)
    {
        if (playerStatus == null || amount <= 0f)
        {
            return;
        }

        playerStatus.IncreaseMaxHPFlat(amount, healByIncrease);
        RefreshHpUI();
    }

    public void SyncFromStatus()
    {
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

        if (playerStatus == null)
        {
            hpImage.fillAmount = 0f;
            return;
        }

        hpImage.fillAmount = playerStatus.CurrentMaxHP > 0f ? playerStatus.CurrentHP / playerStatus.CurrentMaxHP : 0f;
    }

    private void OnValidate()
    {
        invincibleTime = Mathf.Max(0f, invincibleTime);
    }
}
