using System.Collections;
using UnityEngine;

// 씬이 바뀌어도 유지되며 게임 전체의 BGM을 한 곳에서 관리합니다.
[DefaultExecutionOrder(-1000)]
public sealed class AudioManager : MonoBehaviour
{
    private const string RuntimeObjectName = "AudioManager";

    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource bgmSource;
    [Range(0f, 1f)] [SerializeField] private float defaultVolume = 0.5f;

    private Coroutine transitionRoutine;
    private float requestedVolume;

    public AudioClip CurrentBgm => bgmSource != null ? bgmSource.clip : null;
    public bool IsPlaying => bgmSource != null && bgmSource.isPlaying;
    public float RequestedVolume => requestedVolume;

    // 씬에 AudioManager를 직접 배치하지 않아도 처음 요청할 때 자동 생성합니다.
    public static AudioManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        AudioManager existing = FindFirstObjectByType<AudioManager>();
        if (existing != null)
            return existing;

        GameObject managerObject = new(RuntimeObjectName);
        return managerObject.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        PrepareBgmSource();
    }

    private void PrepareBgmSource()
    {
        if (bgmSource == null)
            bgmSource = GetComponent<AudioSource>();
        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();

        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.spatialBlend = 0f;
        requestedVolume = Mathf.Clamp01(defaultVolume);
    }

    public void PlayBgm(
        AudioClip clip,
        float volume = -1f,
        float fadeDuration = 1f,
        bool restartIfSame = false)
    {
        if (clip == null)
        {
            Debug.LogWarning("재생할 BGM AudioClip이 없습니다.", this);
            return;
        }

        PrepareBgmSource();
        requestedVolume = volume < 0f
            ? Mathf.Clamp01(defaultVolume)
            : Mathf.Clamp01(volume);

        if (bgmSource.clip == clip && bgmSource.isPlaying && !restartIfSame)
        {
            StartVolumeChange(requestedVolume, fadeDuration);
            return;
        }

        StopTransition();
        transitionRoutine = StartCoroutine(
            ChangeBgmRoutine(clip, requestedVolume, Mathf.Max(0f, fadeDuration)));
    }

    public void StopBgm(float fadeDuration = 1f)
    {
        if (bgmSource == null || (!bgmSource.isPlaying && bgmSource.clip == null))
            return;

        StopTransition();
        transitionRoutine = StartCoroutine(
            StopBgmRoutine(Mathf.Max(0f, fadeDuration)));
    }

    public void SetBgmVolume(float volume, float fadeDuration = 0f)
    {
        requestedVolume = Mathf.Clamp01(volume);
        StartVolumeChange(requestedVolume, fadeDuration);
    }

    private IEnumerator ChangeBgmRoutine(
        AudioClip nextClip, float targetVolume, float fadeDuration)
    {
        float halfDuration = fadeDuration * 0.5f;

        if (bgmSource.isPlaying)
            yield return FadeVolume(bgmSource.volume, 0f, halfDuration);

        bgmSource.Stop();
        bgmSource.clip = nextClip;
        bgmSource.volume = fadeDuration > 0f ? 0f : targetVolume;
        bgmSource.Play();

        if (fadeDuration > 0f)
            yield return FadeVolume(0f, targetVolume, halfDuration);

        bgmSource.volume = targetVolume;
        transitionRoutine = null;
    }

    private IEnumerator StopBgmRoutine(float fadeDuration)
    {
        yield return FadeVolume(bgmSource.volume, 0f, fadeDuration);
        bgmSource.Stop();
        bgmSource.clip = null;
        transitionRoutine = null;
    }

    private void StartVolumeChange(float targetVolume, float fadeDuration)
    {
        if (bgmSource == null)
            return;

        StopTransition();
        if (fadeDuration <= 0f)
        {
            bgmSource.volume = targetVolume;
            return;
        }

        transitionRoutine = StartCoroutine(
            ChangeVolumeRoutine(targetVolume, fadeDuration));
    }

    private IEnumerator ChangeVolumeRoutine(float targetVolume, float duration)
    {
        yield return FadeVolume(bgmSource.volume, targetVolume, duration);
        bgmSource.volume = targetVolume;
        transitionRoutine = null;
    }

    private IEnumerator FadeVolume(float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            bgmSource.volume = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        bgmSource.volume = to;
    }

    private void StopTransition()
    {
        if (transitionRoutine == null)
            return;

        StopCoroutine(transitionRoutine);
        transitionRoutine = null;
    }
}
