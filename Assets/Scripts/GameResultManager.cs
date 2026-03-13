using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameResultManager : MonoBehaviour
{
    [Header("Result UI")]
    [SerializeField]
    private GameObject hudResult;

    [SerializeField]
    private GameObject success;

    [SerializeField]
    private GameObject fail;

    [SerializeField]
    private Button btnHome;

    [Header("Scene")]
    [SerializeField]
    private string mainSceneName = "MainScene";

    private bool hasResolvedResult;

    private void Awake()
    {
        GameplayPauseState.ResetPauseState();
        EnsureReferences();
        HideResultHud();

        if (btnHome != null)
        {
            btnHome.onClick.RemoveListener(HandleHomeButtonClicked);
            btnHome.onClick.AddListener(HandleHomeButtonClicked);
        }
    }

    private void OnEnable()
    {
        StageSurvivalTimerController.OnStageResultTriggered += HandleStageResultTriggered;
    }

    private void OnDisable()
    {
        StageSurvivalTimerController.OnStageResultTriggered -= HandleStageResultTriggered;

        if (btnHome != null)
        {
            btnHome.onClick.RemoveListener(HandleHomeButtonClicked);
        }
    }

    private void EnsureReferences()
    {
        if (hudResult == null)
        {
            GameObject hudResultObject = GameObject.Find("HUD_Result");
            if (hudResultObject != null)
            {
                hudResult = hudResultObject;
            }
        }

        if (success == null && hudResult != null)
        {
            Transform successTransform = hudResult.transform.Find("Success");
            if (successTransform != null)
            {
                success = successTransform.gameObject;
            }
        }

        if (fail == null && hudResult != null)
        {
            Transform failTransform = hudResult.transform.Find("Fail");
            if (failTransform != null)
            {
                fail = failTransform.gameObject;
            }
        }

        if (btnHome == null && hudResult != null)
        {
            Transform buttonTransform = hudResult.transform.Find("BTN_Home");
            if (buttonTransform != null)
            {
                btnHome = buttonTransform.GetComponent<Button>();
            }
        }
    }

    private void HideResultHud()
    {
        if (hudResult != null)
        {
            hudResult.SetActive(false);
        }

        if (success != null)
        {
            success.SetActive(false);
        }

        if (fail != null)
        {
            fail.SetActive(false);
        }
    }

    private void HandleStageResultTriggered(StageSurvivalTimerController.StageResult stageResult)
    {
        if (hasResolvedResult || stageResult == StageSurvivalTimerController.StageResult.None)
        {
            return;
        }

        hasResolvedResult = true;
        GameplayPauseState.EnterResultPause();

        if (hudResult != null)
        {
            hudResult.SetActive(true);
        }

        bool isSuccess = stageResult == StageSurvivalTimerController.StageResult.Success;
        if (success != null)
        {
            success.SetActive(isSuccess);
        }

        if (fail != null)
        {
            fail.SetActive(!isSuccess);
        }

        if (btnHome != null)
        {
            btnHome.gameObject.SetActive(true);
            btnHome.interactable = true;
        }
    }

    private void HandleHomeButtonClicked()
    {
        GameplayPauseState.ResetPauseState();
        SceneManager.LoadScene(mainSceneName);
    }
}
