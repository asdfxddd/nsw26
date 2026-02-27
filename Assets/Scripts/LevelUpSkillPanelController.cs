using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelUpSkillPanelController : MonoBehaviour
{
    private const string SkillUpgradePanelName = "SkillUpgradePanel";
    private const string CardContainerName = "CardContainer";

    [SerializeField]
    private GameObject skillUpgradePanel;

    [SerializeField]
    private Button[] cardButtons;

    private int pendingLevelUpCount;
    private bool isPanelOpen;

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

        Debug.Log($"Selected skill card index: {cardIndex}. Skill effect application will be implemented later.");

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

        pendingLevelUpCount--;
        isPanelOpen = true;
        SetPanelVisible(true);
        GameplayPauseState.EnterLevelUpPause();

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(cardButtons[0].gameObject);
        }
    }

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

        if (cardButtons != null && cardButtons.Length > 0)
        {
            return;
        }

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
}
