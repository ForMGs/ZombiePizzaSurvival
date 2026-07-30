using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 대화와 컷신처럼 여러 시스템이 동시에 게임 정지를 요청할 수 있도록 관리합니다.
/// 마지막 요청이 해제될 때만 원래 시간 배율로 돌아갑니다.
/// </summary>
public static class GameplayPause
{
    private static readonly HashSet<object> Owners = new();
    private static float previousTimeScale = 1f;

    public static bool IsPaused => Owners.Count > 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetState()
    {
        Owners.Clear();
        previousTimeScale = 1f;
        Time.timeScale = 1f;
    }

    public static void Pause(object owner)
    {
        if (owner == null || !Owners.Add(owner))
            return;

        if (Owners.Count == 1)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    public static void Resume(object owner)
    {
        if (owner == null || !Owners.Remove(owner))
            return;

        if (Owners.Count == 0)
            Time.timeScale = previousTimeScale;
    }
}
