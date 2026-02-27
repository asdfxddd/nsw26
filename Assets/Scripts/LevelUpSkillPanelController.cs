using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelUpSkillPanelController : MonoBehaviour
{
    private const string SkillUpgradePanelName = "SkillUpgradePanel";
    private const string CardContainerName = "CardContainer";
    private const string LevelUpCardResourcePath = "LevelUpCard";

    [SerializeField]
    private GameObject skillUpgradePanel;

    [SerializeField]
    private Button[] cardButtons;

    private readonly List<LevelUpCardData> allCards = new List<LevelUpCardData>();
    private readonly HashSet<int> selectedCardIds = new HashSet<int>();
    private readonly Dictionary<int, LevelUpCardData> cardLookup = new Dictionary<int, LevelUpCardData>();
    private readonly Dictionary<Button, CardView> cardViewsByButton = new Dictionary<Button, CardView>();

    private int pendingLevelUpCount;
    private bool isPanelOpen;
    private List<LevelUpCardData> currentlyPresentedCards = new List<LevelUpCardData>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindFirstObjectByType<LevelUpSkillPanelController>() != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject(nameof(LevelUpSkillPanelController));
        controllerObject.AddComponent<LevelUpSkillPanelController>();
    }

    private void Awake()
    {
        LoadLevelUpCardTable();
        ResolveReferences();
        SetPanelVisible(false);
        RegisterCardClickHandlers();
    }

    private void OnEnable()
    {
        PlayerExperience.Instance.OnLevelChanged += HandleLevelChanged;
    }

    private void OnDisable()
    {
        if (!PlayerExperience.HasInstance)
        {
            return;
        }

        PlayerExperience.Instance.OnLevelChanged -= HandleLevelChanged;
    }

    private void OnDestroy()
    {
        if (isPanelOpen)
        {
            isPanelOpen = false;
            GameplayPauseState.ExitLevelUpPause();
        }
    }

    private void HandleLevelChanged(int _)
    {
        pendingLevelUpCount++;
        TryOpenNextPanel();
    }

    private void RegisterCardClickHandlers()
    {
        if (cardButtons == null)
        {
            return;
        }

        for (int i = 0; i < cardButtons.Length; i++)
        {
            Button button = cardButtons[i];
            if (button == null)
            {
                continue;
            }

            int cardIndex = i;
            button.onClick.AddListener(() => OnCardSelected(cardIndex));
        }
    }

    private void OnCardSelected(int cardIndex)
    {
        if (!isPanelOpen)
        {
            return;
        }

        if (cardIndex < 0 || cardIndex >= currentlyPresentedCards.Count)
        {
            return;
        }

        LevelUpCardData selectedCard = currentlyPresentedCards[cardIndex];
        selectedCardIds.Add(selectedCard.Id);
        ApplyCardEffect(selectedCard);

        isPanelOpen = false;
        SetPanelVisible(false);
        GameplayPauseState.ExitLevelUpPause();

        TryOpenNextPanel();
    }

    private void TryOpenNextPanel()
    {
        if (isPanelOpen || pendingLevelUpCount <= 0)
        {
            return;
        }

        ResolveReferences();
        if (skillUpgradePanel == null || cardButtons == null || cardButtons.Length == 0)
        {
            Debug.LogWarning("Skill upgrade panel or card buttons are not configured.");
            return;
        }

        List<LevelUpCardData> candidates = BuildCandidates();
        if (candidates.Count == 0)
        {
            pendingLevelUpCount--;
            Debug.LogWarning("No available level-up cards remain. Skipping level-up choice panel.");
            TryOpenNextPanel();
            return;
        }

        currentlyPresentedCards = PickCardsByWeight(candidates, Mathf.Min(cardButtons.Length, candidates.Count));
        BindCardsToUi(currentlyPresentedCards);

        pendingLevelUpCount--;
        isPanelOpen = true;
        SetPanelVisible(true);
        GameplayPauseState.EnterLevelUpPause();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(cardButtons[0].gameObject);
        }
    }

    private List<LevelUpCardData> BuildCandidates()
    {
        List<LevelUpCardData> candidates = new List<LevelUpCardData>();

        for (int i = 0; i < allCards.Count; i++)
        {
            LevelUpCardData card = allCards[i];
            if (selectedCardIds.Contains(card.Id))
            {
                continue;
            }

            if (card.RequiredId.HasValue && !selectedCardIds.Contains(card.RequiredId.Value))
            {
                continue;
            }

            candidates.Add(card);
        }

        return candidates;
    }

    private List<LevelUpCardData> PickCardsByWeight(List<LevelUpCardData> candidates, int count)
    {
        List<LevelUpCardData> pool = new List<LevelUpCardData>(candidates);
        List<LevelUpCardData> picked = new List<LevelUpCardData>(count);

        for (int n = 0; n < count && pool.Count > 0; n++)
        {
            int totalWeight = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                totalWeight += Mathf.Max(0, pool[i].Ratio);
            }

            int selectedIndex = 0;
            if (totalWeight > 0)
            {
                int roll = UnityEngine.Random.Range(0, totalWeight);
                int cumulative = 0;

                for (int i = 0; i < pool.Count; i++)
                {
                    cumulative += Mathf.Max(0, pool[i].Ratio);
                    if (roll < cumulative)
                    {
                        selectedIndex = i;
                        break;
                    }
                }
            }
            else
            {
                selectedIndex = UnityEngine.Random.Range(0, pool.Count);
            }

            picked.Add(pool[selectedIndex]);
            pool.RemoveAt(selectedIndex);
        }

        return picked;
    }

    private void BindCardsToUi(List<LevelUpCardData> cards)
    {
        for (int i = 0; i < cardButtons.Length; i++)
        {
            Button button = cardButtons[i];
            if (button == null)
            {
                continue;
            }

            bool hasCard = i < cards.Count;
            button.gameObject.SetActive(hasCard);

            if (!hasCard)
            {
                continue;
            }

            LevelUpCardData card = cards[i];
            CardView view = GetOrCreateCardView(button);

            if (view.IconImage != null)
            {
                Sprite icon = LoadIconSprite(card.Icon);
                if (icon != null)
                {
                    view.IconImage.sprite = icon;
                    view.IconImage.enabled = true;
                }
                else
                {
                    Debug.LogWarning($"Level-up icon not found: {card.Icon}");
                }
            }

            if (view.DescriptionText != null)
            {
                view.DescriptionText.text = card.Desc;
            }
        }
    }

    private void ApplyCardEffect(LevelUpCardData card)
    {
        string valueLog = card.Value.HasValue ? card.Value.Value.ToString() : "(none)";
        Debug.Log($"Apply level-up effect => ID:{card.Id}, Effect:{card.Effect}, Value:{valueLog}");
    }

    private Sprite LoadIconSprite(string iconName)
    {
        if (string.IsNullOrWhiteSpace(iconName))
        {
            return null;
        }

        string trimmedIconName = iconName.Trim();
        string normalizedIconName = NormalizeIconName(trimmedIconName);

        Sprite icon = Resources.Load<Sprite>(trimmedIconName);
        if (icon != null)
        {
            return icon;
        }

        icon = Resources.Load<Sprite>($"Sprites/{trimmedIconName}");
        if (icon != null)
        {
            return icon;
        }

        Sprite[] resourcesSprites = Resources.LoadAll<Sprite>("Sprites");
        for (int i = 0; i < resourcesSprites.Length; i++)
        {
            Sprite candidate = resourcesSprites[i];
            if (candidate == null)
            {
                continue;
            }

            if (NormalizeIconName(candidate.name) == normalizedIconName)
            {
                return candidate;
            }
        }

#if UNITY_EDITOR
        icon = LoadSpriteFromAssetsFolder(trimmedIconName, normalizedIconName);
        if (icon != null)
        {
            return icon;
        }
#endif

        return null;
    }

    private static string NormalizeIconName(string iconName)
    {
        return iconName.Replace("_", string.Empty).Replace("-", string.Empty).ToLowerInvariant();
    }

#if UNITY_EDITOR
    private static Sprite LoadSpriteFromAssetsFolder(string iconName, string normalizedIconName)
    {
        string[] searchRoots = { "Assets/Sprites" };

        string[] exactGuids = AssetDatabase.FindAssets($"{iconName} t:Sprite", searchRoots);
        for (int i = 0; i < exactGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(exactGuids[i]);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null)
            {
                return sprite;
            }
        }

        string[] allGuids = AssetDatabase.FindAssets("t:Sprite", searchRoots);
        for (int i = 0; i < allGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(allGuids[i]);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                continue;
            }

            if (NormalizeIconName(sprite.name) == normalizedIconName)
            {
                return sprite;
            }
        }

        return null;
    }
#endif

    private void SetPanelVisible(bool visible)
    {
        if (skillUpgradePanel == null)
        {
            return;
        }

        skillUpgradePanel.SetActive(visible);

        if (visible && skillUpgradePanel.TryGetComponent<RectTransform>(out RectTransform rectTransform))
        {
            rectTransform.localScale = Vector3.one;
        }
    }

    private void ResolveReferences()
    {
        if (skillUpgradePanel == null)
        {
            GameObject panel = GameObject.Find(SkillUpgradePanelName);
            if (panel != null)
            {
                skillUpgradePanel = panel;
            }
        }

        if (cardButtons == null || cardButtons.Length == 0)
        {
            Transform containerTransform = null;

            if (skillUpgradePanel != null)
            {
                Transform foundContainer = skillUpgradePanel.transform.Find(CardContainerName);
                if (foundContainer != null)
                {
                    containerTransform = foundContainer;
                }
            }

            if (containerTransform == null)
            {
                GameObject containerObject = GameObject.Find(CardContainerName);
                if (containerObject != null)
                {
                    containerTransform = containerObject.transform;
                }
            }

            if (containerTransform != null)
            {
                cardButtons = containerTransform.GetComponentsInChildren<Button>(true);
            }
        }

        cardViewsByButton.Clear();
        if (cardButtons == null)
        {
            return;
        }

        for (int i = 0; i < cardButtons.Length; i++)
        {
            if (cardButtons[i] != null)
            {
                cardViewsByButton[cardButtons[i]] = BuildCardView(cardButtons[i]);
            }
        }
    }

    private CardView GetOrCreateCardView(Button button)
    {
        if (cardViewsByButton.TryGetValue(button, out CardView view))
        {
            return view;
        }

        view = BuildCardView(button);
        cardViewsByButton[button] = view;
        return view;
    }

    private static CardView BuildCardView(Button button)
    {
        Transform root = button.transform;
        Transform iconTransform = root.Find("Image_Icon");
        Transform descriptionTransform = root.Find("Text_Description") ?? root.Find("Text_Desc");

        return new CardView(
            iconTransform != null ? iconTransform.GetComponent<Image>() : null,
            descriptionTransform != null ? descriptionTransform.GetComponent<TMP_Text>() : null);
    }

    private void LoadLevelUpCardTable()
    {
        allCards.Clear();
        cardLookup.Clear();

        TextAsset csvAsset = Resources.Load<TextAsset>(LevelUpCardResourcePath);
        if (csvAsset == null)
        {
            Debug.LogWarning("LevelUpCard.csv not found in Resources.");
            return;
        }

        string[] lines = csvAsset.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < lines.Length; i++)
        {
            string[] columns = lines[i].Split(',');
            if (columns.Length < 7)
            {
                continue;
            }

            if (!int.TryParse(columns[0].Trim(), out int id) || id <= 0)
            {
                continue;
            }

            if (!int.TryParse(columns[1].Trim(), out int ratio) || ratio < 0)
            {
                continue;
            }

            int? requiredId = null;
            string requiredRaw = columns[2].Trim();
            if (!string.IsNullOrEmpty(requiredRaw))
            {
                if (int.TryParse(requiredRaw, out int parsedRequiredId) && parsedRequiredId > 0)
                {
                    requiredId = parsedRequiredId;
                }
                else
                {
                    continue;
                }
            }

            string icon = columns[3].Trim();
            string desc = columns[4].Trim();
            string effect = columns[5].Trim();

            int? value = null;
            string valueRaw = columns[6].Trim();
            if (!string.IsNullOrEmpty(valueRaw))
            {
                if (int.TryParse(valueRaw, out int parsedValue))
                {
                    value = parsedValue;
                }
                else
                {
                    continue;
                }
            }

            LevelUpCardData card = new LevelUpCardData(id, ratio, requiredId, icon, desc, effect, value);
            allCards.Add(card);
            cardLookup[id] = card;
        }
    }

    private readonly struct LevelUpCardData
    {
        public LevelUpCardData(int id, int ratio, int? requiredId, string icon, string desc, string effect, int? value)
        {
            Id = id;
            Ratio = ratio;
            RequiredId = requiredId;
            Icon = icon;
            Desc = desc;
            Effect = effect;
            Value = value;
        }

        public int Id { get; }

        public int Ratio { get; }

        public int? RequiredId { get; }

        public string Icon { get; }

        public string Desc { get; }

        public string Effect { get; }

        public int? Value { get; }
    }

    private readonly struct CardView
    {
        public CardView(Image iconImage, TMP_Text descriptionText)
        {
            IconImage = iconImage;
            DescriptionText = descriptionText;
        }

        public Image IconImage { get; }

        public TMP_Text DescriptionText { get; }
    }
}
