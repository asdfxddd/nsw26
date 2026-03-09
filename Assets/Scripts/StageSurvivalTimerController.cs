using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class StageSurvivalTimerController : MonoBehaviour
{
    public enum StageResult
    {
        None,
        Success,
        Failure
    }

    public static event Action<StageResult> OnStageResultTriggered;

    [Serializable]
    private class StageRow
    {
        public int StageId;
        public string SceneName;
        public float TimeSeconds;
    }

    [Header("Stage Source")]
    [SerializeField]
    private string stageResourcePath = "Stage";

    [SerializeField]
    private bool useStageIdOverride = true;

    [SerializeField]
    private int stageIdOverride = 1;

    [Header("HUD")]
    [SerializeField]
    private TMP_Text hudTimeText;

    [SerializeField]
    private string hudTimeObjectName = "HUD_TimeText";

    [SerializeField]
    private Color normalColor = Color.white;

    [SerializeField]
    private Color dangerColor = Color.red;

    [SerializeField]
    private float dangerThresholdSeconds = 10f;

    [Header("Result Triggers")]
    [SerializeField]
    private UnityEvent onStageSuccess;

    [SerializeField]
    private UnityEvent onStageFailure;

    private PlayerHealth playerHealth;
    private float remainingTime;
    private bool timerRunning;
    private StageResult result = StageResult.None;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        StageSurvivalTimerController existing = FindObjectOfType<StageSurvivalTimerController>();
        if (existing != null)
        {
            return;
        }

        GameObject bootstrapObject = new GameObject(nameof(StageSurvivalTimerController));
        bootstrapObject.AddComponent<StageSurvivalTimerController>();
    }

    private void Awake()
    {
        ResolveReferences();
        InitializeTimer();
        RefreshHud();
    }

    private void Update()
    {
        if (!timerRunning)
        {
            return;
        }

        if (result != StageResult.None)
        {
            timerRunning = false;
            return;
        }

        if (IsGameplayPaused())
        {
            RefreshHud();
            return;
        }

        remainingTime = Mathf.Max(0f, remainingTime - Time.deltaTime);

        if (remainingTime <= 0f)
        {
            TriggerSuccess();
            return;
        }

        if (playerHealth != null && playerHealth.CurrentHP <= 0f)
        {
            TriggerFailure();
            return;
        }

        RefreshHud();
    }

    private void ResolveReferences()
    {
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }

        if (hudTimeText == null)
        {
            GameObject timeObject = GameObject.Find(hudTimeObjectName);
            if (timeObject != null)
            {
                hudTimeText = timeObject.GetComponent<TMP_Text>();
            }
        }
    }

    private void InitializeTimer()
    {
        float stageTime = ResolveStageTimeSeconds();
        remainingTime = Mathf.Max(0f, stageTime);
        timerRunning = remainingTime > 0f;

        if (!timerRunning)
        {
            Debug.LogWarning("Stage timer disabled because resolved stage time is zero or missing.");
        }
    }

    private bool IsGameplayPaused()
    {
        return GameplayPauseState.IsGameplayPaused || Mathf.Approximately(Time.timeScale, 0f);
    }

    private float ResolveStageTimeSeconds()
    {
        TextAsset stageCsv = Resources.Load<TextAsset>(stageResourcePath);
        if (stageCsv == null)
        {
            Debug.LogWarning("Stage.csv not found in Resources.");
            return 0f;
        }

        StageRow stageRow = FindStageRow(stageCsv.text);
        return stageRow != null ? stageRow.TimeSeconds : 0f;
    }

    private StageRow FindStageRow(string csvText)
    {
        string sceneName = SceneManager.GetActiveScene().name;
        int stageId = stageIdOverride;

        string[] lines = csvText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');
            if (columns.Length < 4)
            {
                continue;
            }

            if (!int.TryParse(columns[0], out int rowStageId))
            {
                continue;
            }

            string rowSceneName = columns[1].Trim();
            float rowTime = ParseFloat(columns[3]);

            if (useStageIdOverride)
            {
                if (rowStageId == stageId)
                {
                    return new StageRow { StageId = rowStageId, SceneName = rowSceneName, TimeSeconds = rowTime };
                }

                continue;
            }

            if (string.Equals(rowSceneName, sceneName, StringComparison.OrdinalIgnoreCase))
            {
                return new StageRow { StageId = rowStageId, SceneName = rowSceneName, TimeSeconds = rowTime };
            }
        }

        Debug.LogWarning($"No stage row found for timer. Scene='{sceneName}', StageIdOverride={stageId}.");
        return null;
    }

    private static float ParseFloat(string value)
    {
        if (float.TryParse(value, out float parsed))
        {
            return parsed;
        }

        return 0f;
    }

    private void TriggerSuccess()
    {
        if (result != StageResult.None)
        {
            return;
        }

        result = StageResult.Success;
        timerRunning = false;
        remainingTime = 0f;
        RefreshHud();
        onStageSuccess?.Invoke();
        OnStageResultTriggered?.Invoke(StageResult.Success);
        Debug.Log("Stage success triggered by survival timer.");
    }

    private void TriggerFailure()
    {
        if (result != StageResult.None)
        {
            return;
        }

        result = StageResult.Failure;
        timerRunning = false;
        onStageFailure?.Invoke();
        OnStageResultTriggered?.Invoke(StageResult.Failure);
        Debug.Log("Stage failure triggered because player died before timer completion.");
    }

    private void RefreshHud()
    {
        if (hudTimeText == null)
        {
            return;
        }

        int totalSeconds = Mathf.CeilToInt(Mathf.Max(0f, remainingTime));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        hudTimeText.text = $"{minutes:00}:{seconds:00}";

        bool isDanger = remainingTime > 0f && remainingTime <= dangerThresholdSeconds;
        hudTimeText.color = isDanger ? dangerColor : normalColor;
    }
}
