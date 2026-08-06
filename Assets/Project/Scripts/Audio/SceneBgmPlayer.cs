using UnityEngine;

// 이 컴포넌트를 씬에 배치하고 BGM Clip을 연결하면 씬 시작 시 전역 BGM을 재생합니다.
public sealed class SceneBgmPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip bgmClip;
    [Range(0f, 1f)] [SerializeField] private float volume = 0.5f;
    [Min(0f)] [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private bool restartIfSame;

    private void Start()
    {
        if (bgmClip == null)
        {
            Debug.LogWarning("SceneBgmPlayer에 BGM Clip이 연결되지 않았습니다.", this);
            return;
        }

        AudioManager.GetOrCreate().PlayBgm(
            bgmClip,
            volume,
            fadeDuration,
            restartIfSame);
    }
}

