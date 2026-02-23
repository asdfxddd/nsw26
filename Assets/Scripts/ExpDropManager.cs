using System;
using System.Collections.Generic;
using UnityEngine;

public class ExpDropManager : MonoBehaviour
{
    private const string LevelXpResourcePath = "LevelXP";

    private static ExpDropManager instance;

    [SerializeField]
    private float magnetRange = 2.5f;

    [SerializeField]
    private float absorbMoveSpeed = 10f;

    [SerializeField]
    private Transform playerTransform;

    [SerializeField]
    private int totalExp;

    [SerializeField]
    private int currentLevel = 1;

    [SerializeField]
    private int expIntoCurrentLevel;

    [SerializeField]
    private int needXpForNextLevel;

    private readonly Dictionary<int, int> levelNeedXpTable = new Dictionary<int, int>();

    public static ExpDropManager Instance
    {
        get
        {
            if (instance == null)
            {
                ExpDropManager existing = FindFirstObjectByType<ExpDropManager>();
                if (existing != null)
                {
                    instance = existing;
                }
                else
                {
                    GameObject managerObject = new GameObject(nameof(ExpDropManager));
                    instance = managerObject.AddComponent<ExpDropManager>();
                }
            }

            return instance;
        }
    }

    public float MagnetRange => magnetRange;

    public float AbsorbMoveSpeed => absorbMoveSpeed;

    public int TotalExp => totalExp;

    public int CurrentLevel => currentLevel;

    public int ExpIntoCurrentLevel => expIntoCurrentLevel;

    public int NeedXpForNextLevel => needXpForNextLevel;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        LoadLevelXpTable();
        RecalculateLevelState();
    }

    public bool TryGetPlayerPosition(out Vector3 playerPosition)
    {
        EnsurePlayerTransform();
        if (playerTransform == null)
        {
            playerPosition = default;
            return false;
        }

        playerPosition = playerTransform.position;
        return true;
    }

    public void AddExp(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        totalExp += amount;
        RecalculateLevelState();
    }

    private void EnsurePlayerTransform()
    {
        if (playerTransform != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
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

            if (!int.TryParse(columns[1].Trim(), out int needXp) || needXp <= 0)
            {
                continue;
            }

            levelNeedXpTable[level] = needXp;
        }
    }

    private void RecalculateLevelState()
    {
        int remainingExp = Mathf.Max(0, totalExp);
        int calculatedLevel = 1;

        while (levelNeedXpTable.TryGetValue(calculatedLevel, out int levelNeedXp) && remainingExp >= levelNeedXp)
        {
            remainingExp -= levelNeedXp;
            calculatedLevel++;
        }

        currentLevel = calculatedLevel;
        expIntoCurrentLevel = remainingExp;
        needXpForNextLevel = levelNeedXpTable.TryGetValue(currentLevel, out int needXp) ? needXp : 0;
    }

    private void OnValidate()
    {
        magnetRange = Mathf.Max(0f, magnetRange);
        absorbMoveSpeed = Mathf.Max(0.01f, absorbMoveSpeed);
        totalExp = Mathf.Max(0, totalExp);
        currentLevel = Mathf.Max(1, currentLevel);
        expIntoCurrentLevel = Mathf.Max(0, expIntoCurrentLevel);
        needXpForNextLevel = Mathf.Max(0, needXpForNextLevel);
    }
}
