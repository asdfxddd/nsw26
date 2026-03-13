using UnityEngine;

public static class GameplayPauseState
{
    private static int levelUpPauseCount;
    private static bool isResultPaused;
    private static float cachedTimeScale = 1f;

    public static bool IsGameplayPaused => levelUpPauseCount > 0 || isResultPaused;

    public static void EnterLevelUpPause()
    {
        if (isResultPaused)
        {
            levelUpPauseCount++;
            return;
        }

        if (levelUpPauseCount == 0)
        {
            cachedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        levelUpPauseCount++;
    }

    public static void ExitLevelUpPause()
    {
        if (levelUpPauseCount <= 0)
        {
            return;
        }

        levelUpPauseCount--;
        if (levelUpPauseCount == 0 && !isResultPaused)
        {
            Time.timeScale = cachedTimeScale > 0f ? cachedTimeScale : 1f;
        }
    }

    public static void EnterResultPause()
    {
        if (isResultPaused)
        {
            return;
        }

        isResultPaused = true;
        Time.timeScale = 0f;
    }

    public static void ResetPauseState(float defaultTimeScale = 1f)
    {
        levelUpPauseCount = 0;
        isResultPaused = false;
        cachedTimeScale = defaultTimeScale > 0f ? defaultTimeScale : 1f;
        Time.timeScale = cachedTimeScale;
    }
}
