using UnityEngine;

public static class GameplayPauseState
{
    private static int levelUpPauseCount;
    private static float cachedTimeScale = 1f;

    public static bool IsGameplayPaused => levelUpPauseCount > 0;

    public static void EnterLevelUpPause()
    {
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
        if (levelUpPauseCount == 0)
        {
            Time.timeScale = cachedTimeScale > 0f ? cachedTimeScale : 1f;
        }
    }
}
