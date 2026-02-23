using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExperience : MonoBehaviour
{
    private const string LevelXpResourcePath = "LevelXP";

    public readonly struct ExpGainStep
    {
        public ExpGainStep(int startExp, int endExp, int needExp, bool isLevelUp)
        {
            StartExp = startExp;
            EndExp = endExp;
            NeedExp = needExp;
            IsLevelUp = isLevelUp;
        }

        public int StartExp { get; }

        public int EndExp { get; }

        public int NeedExp { get; }

        public bool IsLevelUp { get; }
    }

    private static PlayerExperience instance;

    [SerializeField]
    private int currentLevel = 1;

    [SerializeField]
    private int currentExp;

    [SerializeField]
    private int needExp;

    private readonly Dictionary<int, int> levelNeedXpTable = new Dictionary<int, int>();

    public static bool HasInstance => instance != null;

    public static PlayerExperience Instance
    {
        get
        {
            if (instance == null)
            {
                PlayerExperience existing = FindFirstObjectByType<PlayerExperience>();
                if (existing != null)
                {
                    instance = existing;
                }
                else
                {
                    GameObject objectWithComponent = new GameObject(nameof(PlayerExperience));
                    instance = objectWithComponent.AddComponent<PlayerExperience>();
                }
            }

            return instance;
        }
    }

    public int CurrentLevel => currentLevel;

    public int CurrentExp => currentExp;

    public int NeedExp => needExp;

    public event Action<IReadOnlyList<ExpGainStep>> OnExpProcessed;

    public event Action<int> OnLevelChanged;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        LoadLevelXpTable();
        currentLevel = Mathf.Max(1, currentLevel);
        currentExp = Mathf.Max(0, currentExp);
        needExp = GetNeedExpForLevel(currentLevel);

        NormalizeState();
    }

    public void AddExp(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        List<ExpGainStep> steps = new List<ExpGainStep>();
        int remaining = amount;

        while (remaining > 0)
        {
            int currentNeedExp = GetNeedExpForLevel(currentLevel);
            needExp = currentNeedExp;

            if (currentNeedExp <= 0)
            {
                int start = currentExp;
                currentExp += remaining;
                steps.Add(new ExpGainStep(start, currentExp, Mathf.Max(currentExp, 1), false));
                remaining = 0;
                break;
            }

            int room = currentNeedExp - currentExp;
            if (remaining >= room)
            {
                int start = currentExp;
                currentExp = currentNeedExp;
                steps.Add(new ExpGainStep(start, currentExp, currentNeedExp, true));

                remaining -= room;
                currentLevel++;
                currentExp = 0;
                needExp = GetNeedExpForLevel(currentLevel);
                OnLevelChanged?.Invoke(currentLevel);
                continue;
            }

            int nextExp = currentExp + remaining;
            steps.Add(new ExpGainStep(currentExp, nextExp, currentNeedExp, false));
            currentExp = nextExp;
            remaining = 0;
        }

        OnExpProcessed?.Invoke(steps);
    }

    private int GetNeedExpForLevel(int level)
    {
        return levelNeedXpTable.TryGetValue(level, out int value) ? value : 0;
    }

    private void NormalizeState()
    {
        while (needExp > 0 && currentExp >= needExp)
        {
            currentExp -= needExp;
            currentLevel++;
            needExp = GetNeedExpForLevel(currentLevel);
            OnLevelChanged?.Invoke(currentLevel);
        }
    }

    private void LoadLevelXpTable()
    {
        levelNeedXpTable.Clear();

        TextAsset levelXpCsv = Resources.Load<TextAsset>(LevelXpResourcePath);
        if (levelXpCsv == null)
        {
            Debug.LogWarning("LevelXP.csv not found in Resources.");
            return;
        }

        string[] lines = levelXpCsv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');
            if (columns.Length < 2)
            {
                continue;
            }

            if (!int.TryParse(columns[0].Trim(), out int level) || level < 1)
            {
                continue;
            }

            if (!int.TryParse(columns[1].Trim(), out int parsedNeedExp) || parsedNeedExp <= 0)
            {
                continue;
            }

            levelNeedXpTable[level] = parsedNeedExp;
        }
    }

    private void OnValidate()
    {
        currentLevel = Mathf.Max(1, currentLevel);
        currentExp = Mathf.Max(0, currentExp);
        needExp = Mathf.Max(0, needExp);
    }
}
