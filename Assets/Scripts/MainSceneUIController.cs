using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneUIController : MonoBehaviour
{
    [Serializable]
    private class PopupBinding
    {
        public string popupName;
        public Button openButton;
        public GameObject popup;
        public Button closeButton;
    }

    [Serializable]
    private class StageRow
    {
        public string sceneName;
        public string stageName;
        public string stageImage;
    }

    [Header("Popup")]
    [SerializeField]
    private GameObject popupPanel;

    [SerializeField]
    private PopupBinding[] popups;

    [SerializeField]
    private bool closeOnBackdrop = true;

    [SerializeField]
    private Button backdropButton;

    [Header("Stage Cards")]
    [SerializeField]
    private string stageCsvPath = "Stage";

    [SerializeField]
    private Transform stageCardPanel;

    [SerializeField]
    private RectTransform stageCardContent;

    [SerializeField]
    private GameObject stageCardPrefab;

    [SerializeField]
    private bool clearExistingCards = true;

    [Header("Exit")]
    [SerializeField]
    private Button exitButton;

    private readonly List<GameObject> spawnedCards = new List<GameObject>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (!string.Equals(currentScene.name, "MainScene", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (FindObjectOfType<MainSceneUIController>() != null)
        {
            return;
        }

        GameObject bootstrapObject = new GameObject(nameof(MainSceneUIController));
        bootstrapObject.AddComponent<MainSceneUIController>();
    }

    private void Awake()
    {
        AutoResolveReferences();
        BindPopupButtons();
        BindExitButton();
        EnsurePanelStartsClosed();
        SetupStageCardContentLayout();
        BuildStageCards();
    }

    private void AutoResolveReferences()
    {
        if (popupPanel == null)
        {
            GameObject panel = GameObject.Find("Popup_Panel");
            if (panel != null)
            {
                popupPanel = panel;
            }
        }

        if ((popups == null || popups.Length == 0) && popupPanel != null)
        {
            popups = new[]
            {
                new PopupBinding
                {
                    popupName = "Stage",
                    openButton = FindButton("BTN_GameStart"),
                    popup = FindChild(popupPanel.transform, "Popup_Stage"),
                    closeButton = FindButtonInPopup("Popup_Stage")
                },
                new PopupBinding
                {
                    popupName = "Upgrade",
                    openButton = FindButton("BTN_Upgrade"),
                    popup = FindChild(popupPanel.transform, "Popup_Upgrade"),
                    closeButton = FindButtonInPopup("Popup_Upgrade")
                },
                new PopupBinding
                {
                    popupName = "Achivement",
                    openButton = FindButton("BTN_Achivement"),
                    popup = FindChild(popupPanel.transform, "Popup_Achivement"),
                    closeButton = FindButtonInPopup("Popup_Achivement")
                }
            };
        }

        if (backdropButton == null && popupPanel != null)
        {
            backdropButton = FindButtonUnder(popupPanel.transform, "Backdrop");
        }

        if (stageCardPanel == null)
        {
            GameObject panel = GameObject.Find("StageCardPanel");
            if (panel != null)
            {
                stageCardPanel = panel.transform;
            }
        }

        if (stageCardContent == null && stageCardPanel != null)
        {
            stageCardContent = stageCardPanel as RectTransform;
        }

        if (exitButton == null)
        {
            exitButton = FindButton("BTN_Exit");
        }
    }

    private static Button FindButton(string objectName)
    {
        GameObject found = GameObject.Find(objectName);
        return found != null ? found.GetComponent<Button>() : null;
    }

    private static GameObject FindChild(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }

        Transform found = root.Find(name);
        return found != null ? found.gameObject : null;
    }

    private Button FindButtonInPopup(string popupName)
    {
        if (popupPanel == null)
        {
            return null;
        }

        Transform popup = popupPanel.transform.Find(popupName);
        if (popup == null)
        {
            return null;
        }

        return FindButtonUnder(popup, "Xbutton");
    }

    private static Button FindButtonUnder(Transform root, string objectName)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (string.Equals(child.name, objectName, StringComparison.Ordinal))
            {
                return child.GetComponent<Button>();
            }
        }

        return null;
    }

    private void BindPopupButtons()
    {
        if (popups == null)
        {
            return;
        }

        foreach (PopupBinding binding in popups)
        {
            if (binding == null)
            {
                continue;
            }

            if (binding.openButton != null)
            {
                PopupBinding captured = binding;
                binding.openButton.onClick.AddListener(() => OpenPopup(captured));
            }

            Button closeButton = binding.closeButton;
            if (closeButton == null && binding.popup != null)
            {
                Transform closeTransform = binding.popup.transform.Find("Xbutton");
                if (closeTransform != null)
                {
                    closeButton = closeTransform.GetComponent<Button>();
                }
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseAllPopups);
            }
        }

        if (closeOnBackdrop && backdropButton != null)
        {
            backdropButton.onClick.AddListener(CloseAllPopups);
        }
    }

    private void BindExitButton()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(HandleExitButtonClicked);
        }
    }

    private void EnsurePanelStartsClosed()
    {
        SetPopupVisibility(null);
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    private void OpenPopup(PopupBinding target)
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
        }

        SetPopupVisibility(target?.popup);
    }

    private void CloseAllPopups()
    {
        SetPopupVisibility(null);
        if (popupPanel != null)
        {
            popupPanel.SetActive(false);
        }
    }

    private void SetPopupVisibility(GameObject visiblePopup)
    {
        if (popups == null)
        {
            return;
        }

        foreach (PopupBinding binding in popups)
        {
            if (binding?.popup == null)
            {
                continue;
            }

            bool shouldShow = visiblePopup != null && binding.popup == visiblePopup;
            binding.popup.SetActive(shouldShow);
        }
    }

    private void SetupStageCardContentLayout()
    {
        if (stageCardContent == null && stageCardPanel != null)
        {
            stageCardContent = stageCardPanel as RectTransform;
        }

        if (stageCardContent == null)
        {
            return;
        }

        VerticalLayoutGroup layoutGroup = stageCardContent.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup == null)
        {
            layoutGroup = stageCardContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 8f;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
        }

        ContentSizeFitter sizeFitter = stageCardContent.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = stageCardContent.gameObject.AddComponent<ContentSizeFitter>();
        }

        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    private void BuildStageCards()
    {
        if (stageCardPanel == null || stageCardPrefab == null)
        {
            return;
        }

        if (clearExistingCards)
        {
            for (int i = stageCardPanel.childCount - 1; i >= 0; i--)
            {
                Destroy(stageCardPanel.GetChild(i).gameObject);
            }

            spawnedCards.Clear();
        }

        foreach (StageRow row in ParseStageRows())
        {
            GameObject card = Instantiate(stageCardPrefab, stageCardPanel);
            card.name = $"StageCard_{row.sceneName}";
            ApplyStageCardView(card, row);
            BindStageCardClick(card, row.sceneName);
            spawnedCards.Add(card);
        }

        if (stageCardContent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(stageCardContent);
        }
    }

    private IEnumerable<StageRow> ParseStageRows()
    {
        TextAsset csv = Resources.Load<TextAsset>(stageCsvPath);
        if (csv == null)
        {
            Debug.LogWarning($"Stage csv not found at Resources/{stageCsvPath}.");
            yield break;
        }

        string[] lines = csv.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            yield break;
        }

        Dictionary<string, int> headers = BuildHeaderMap(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] cols = lines[i].Split(',');
            string sceneName = ReadColumn(cols, headers, "SceneName");
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                continue;
            }

            yield return new StageRow
            {
                sceneName = sceneName,
                stageName = ReadColumn(cols, headers, "StageName"),
                stageImage = ReadColumn(cols, headers, "StageImage")
            };
        }
    }

    private static Dictionary<string, int> BuildHeaderMap(string headerLine)
    {
        Dictionary<string, int> map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        string[] headerColumns = headerLine.Split(',');
        for (int i = 0; i < headerColumns.Length; i++)
        {
            string key = headerColumns[i].Trim();
            if (!string.IsNullOrEmpty(key) && !map.ContainsKey(key))
            {
                map.Add(key, i);
            }
        }

        return map;
    }

    private static string ReadColumn(string[] columns, Dictionary<string, int> headers, string key)
    {
        if (headers.TryGetValue(key, out int index) && index >= 0 && index < columns.Length)
        {
            return columns[index].Trim();
        }

        return string.Empty;
    }

    private void ApplyStageCardView(GameObject card, StageRow row)
    {
        Transform imageTransform = card.transform.Find("StageImage");
        if (imageTransform != null)
        {
            Image image = imageTransform.GetComponent<Image>();
            if (image != null)
            {
                image.sprite = LoadStageSprite(row.stageImage);
            }
        }

        Transform titleTransform = card.transform.Find("StageTitle");
        if (titleTransform != null)
        {
            TMP_Text tmpText = titleTransform.GetComponent<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = string.IsNullOrWhiteSpace(row.stageName) ? row.sceneName : row.stageName;
            }
            else
            {
                Text legacyText = titleTransform.GetComponent<Text>();
                if (legacyText != null)
                {
                    legacyText.text = string.IsNullOrWhiteSpace(row.stageName) ? row.sceneName : row.stageName;
                }
            }
        }
    }

    private Sprite LoadStageSprite(string stageImageKey)
    {
        if (string.IsNullOrWhiteSpace(stageImageKey))
        {
            return null;
        }

        Sprite sprite = Resources.Load<Sprite>($"Sprite/{stageImageKey}");
        if (sprite != null)
        {
            return sprite;
        }

        return Resources.Load<Sprite>(stageImageKey);
    }

    private static void BindStageCardClick(GameObject card, string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return;
        }

        Button button = card.GetComponent<Button>();
        if (button == null)
        {
            button = card.GetComponentInChildren<Button>();
        }

        if (button == null)
        {
            return;
        }

        button.onClick.AddListener(() => SceneManager.LoadScene(sceneName));
    }

    private void HandleExitButtonClicked()
    {
#if UNITY_EDITOR
        Debug.Log("Exit requested in editor. Application.Quit is skipped.");
#else
        Application.Quit();
#endif
    }
}
