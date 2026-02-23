using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExpBarController : MonoBehaviour
{
    [SerializeField]
    private Image expBarFill;

    [SerializeField]
    private TMP_Text hudLevelText;

    [SerializeField]
    private float fillAnimationDuration = 0.2f;

    [SerializeField]
    private float fullBarHoldDuration = 0.12f;

    private Coroutine animationRoutine;

    private void Awake()
    {
        if (expBarFill == null)
        {
            expBarFill = GetComponent<Image>();
        }
    }

    private void OnEnable()
    {
        PlayerExperience experience = PlayerExperience.Instance;
        experience.OnExpProcessed += HandleExpProcessed;
        experience.OnLevelChanged += HandleLevelChanged;

        RefreshLevelText(experience.CurrentLevel);
        SetFillImmediate(CalculateRatio(experience.CurrentExp, experience.NeedExp));
    }

    private void OnDisable()
    {
        if (!PlayerExperience.HasInstance)
        {
            return;
        }

        PlayerExperience.Instance.OnExpProcessed -= HandleExpProcessed;
        PlayerExperience.Instance.OnLevelChanged -= HandleLevelChanged;
    }

    private void HandleExpProcessed(IReadOnlyList<PlayerExperience.ExpGainStep> steps)
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(AnimateExpChanges(steps));
    }

    private IEnumerator AnimateExpChanges(IReadOnlyList<PlayerExperience.ExpGainStep> steps)
    {
        if (steps == null || steps.Count == 0)
        {
            yield break;
        }

        for (int i = 0; i < steps.Count; i++)
        {
            PlayerExperience.ExpGainStep step = steps[i];

            float startRatio = CalculateRatio(step.StartExp, step.NeedExp);
            float endRatio = CalculateRatio(step.EndExp, step.NeedExp);
            yield return AnimateFill(startRatio, endRatio, fillAnimationDuration);

            if (step.IsLevelUp)
            {
                yield return new WaitForSeconds(fullBarHoldDuration);
                SetFillImmediate(0f);
            }
        }

        PlayerExperience experience = PlayerExperience.Instance;
        SetFillImmediate(CalculateRatio(experience.CurrentExp, experience.NeedExp));
        animationRoutine = null;
    }

    private IEnumerator AnimateFill(float start, float end, float duration)
    {
        if (duration <= 0f)
        {
            SetFillImmediate(end);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float ratio = Mathf.Lerp(start, end, t);
            SetFillImmediate(ratio);
            yield return null;
        }

        SetFillImmediate(end);
    }

    private void HandleLevelChanged(int newLevel)
    {
        RefreshLevelText(newLevel);
    }

    private void RefreshLevelText(int level)
    {
        if (hudLevelText != null)
        {
            hudLevelText.text = $"Level: {level}";
        }
    }

    private float CalculateRatio(int exp, int needExp)
    {
        if (needExp <= 0)
        {
            return 1f;
        }

        return Mathf.Clamp01((float)exp / needExp);
    }

    private void SetFillImmediate(float value)
    {
        if (expBarFill != null)
        {
            expBarFill.fillAmount = Mathf.Clamp01(value);
        }
    }

    private void OnValidate()
    {
        fillAnimationDuration = Mathf.Max(0f, fillAnimationDuration);
        fullBarHoldDuration = Mathf.Max(0f, fullBarHoldDuration);
    }
}
