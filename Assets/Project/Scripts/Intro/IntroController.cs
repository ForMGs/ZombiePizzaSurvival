using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroController : MonoBehaviour
{
    // 손전등 경로점 하나에서 재생할 효과음 이벤트입니다.
    // 한 경로점에 이 이벤트를 여러 개 등록하여 서로 다른 타이밍에 재생할 수 있습니다.
    [Serializable]
    public class SpotlightSoundEvent
    {
        public AudioClip clip;
        [Tooltip("켜면 손전등 도착 시점, 끄면 이동 시작 시점을 기준으로 Delay를 계산합니다.")]
        public bool playOnArrival = true;
        [Min(0f)] public float delay;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(-3f, 3f)] public float pitch = 1f;
        [Tooltip("켜면 다음 손전등 경로점으로 넘어갈 때까지 반복 재생합니다.")]
        public bool loop;
    }

    // 한 컷 안에서 손전등이 방문할 경로점 하나를 나타냅니다.
    // 경로점마다 위치, 이동시간, 머무르는 시간, 빛의 모양을 따로 설정할 수 있습니다.
    [Serializable]
    public class SpotlightStep
    {
        [Tooltip("화면 좌하단 (0,0), 우상단 (1,1) 기준의 목표 위치")]
        public Vector2 position = new(0.5f, 0.5f);
        [Tooltip("이전 경로점에서 이 위치까지 이동하는 시간")]
        [Min(0f)] public float moveDuration = 0.35f;
        [Tooltip("이 위치를 비추며 머무르는 시간")]
        [Min(0f)] public float holdDuration = 1f;
        [Tooltip("화면 너비/높이에 대한 손전등 크기")]
        public Vector2 size = new(0.32f, 0.42f);
        [Tooltip("0은 단단한 경계, 1은 매우 부드러운 경계")]
        [Range(0.001f, 1f)] public float softness = 0.35f;
        [Tooltip("주변을 어둡게 덮는 정도")]
        [Range(0f, 1f)] public float darkness = 0.92f;
        public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("경로점 줌")]
        [Tooltip("켜면 줌 중심이 이동 중인 손전등 위치를 그대로 따라갑니다.")]
        public bool zoomFollowsSpotlight = true;
        [Tooltip("따라가기를 끈 경우에 사용할 별도의 줌 중심 위치")]
        public Vector2 zoomFocusPosition = new(0.5f, 0.5f);
        [Tooltip("이 경로점에 도착했을 때의 이미지 확대 배율")]
        [Min(0.01f)] public float zoom = 1.15f;

        [Header("경로점 자막")]
        [TextArea(1, 4)] public string subtitle;
        [Tooltip("손전등이 이 경로점에 도착한 뒤 자막이 나타날 때까지의 시간")]
        [Min(0f)] public float subtitleDelay = 0.1f;
        [Tooltip("0이면 다음 경로점으로 넘어갈 때까지 표시")]
        [Min(0f)] public float subtitleDuration;
        [Min(0f)] public float subtitleFadeDuration = 0.15f;

        [Header("경로점 효과음")]
        [Tooltip("이 경로점에서 재생할 효과음을 원하는 만큼 추가할 수 있습니다.")]
        public List<SpotlightSoundEvent> soundEvents = new();

        // 이 경로점에 소비되는 전체 시간입니다.
        public float TotalDuration => moveDuration + holdDuration;
    }

    // 인트로 이미지 한 장과 그 이미지에서 실행할 모든 연출 설정입니다.
    [Serializable]
    public class IntroCut
    {
        [Header("이미지")]
        public Sprite image;
        [Tooltip("페이드 시간을 제외하고 이 컷이 재생되는 시간")]
        [Min(0f)] public float duration = 2.5f;
        [Min(0f)] public float fadeInDuration = 0.25f;
        [Min(0f)] public float fadeOutDuration = 0.25f;

        [Header("손전등")]
        [Tooltip("화면 좌하단 (0,0), 우상단 (1,1) 기준의 손전등 목표 위치")]
        public Vector2 spotlightPosition = new(0.5f, 0.5f);
        [Tooltip("화면 너비/높이에 대한 손전등 크기")]
        public Vector2 spotlightSize = new(0.32f, 0.42f);
        [Tooltip("0은 단단한 경계, 1은 매우 부드러운 경계")]
        [Range(0.001f, 1f)] public float spotlightSoftness = 0.35f;
        [Tooltip("주변을 어둡게 덮는 정도")]
        [Range(0f, 1f)] public float darkness = 0.92f;
        [Tooltip("이전 컷의 위치에서 이 컷의 위치까지 손전등이 이동하는 시간")]
        [Min(0f)] public float spotlightMoveDuration = 0.35f;
        [Tooltip("비어 있으면 위의 단일 손전등 설정을 사용합니다. 경로점을 추가하면 위치를 여러 번 이동합니다.")]
        public List<SpotlightStep> spotlightPath = new();

        [Header("줌")]
        [Tooltip("화면 좌하단 (0,0), 우상단 (1,1) 기준의 줌 중심")]
        public Vector2 zoomFocusPosition = new(0.5f, 0.5f);
        [Min(0.01f)] public float zoomStart = 1f;
        [Min(0.01f)] public float zoomEnd = 1.15f;
        public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("자막")]
        [TextArea(1, 4)] public string subtitle;
        [Tooltip("컷이 시작된 뒤 자막이 나타날 때까지의 시간")]
        [Min(0f)] public float subtitleDelay = 0.2f;
        [Tooltip("0이면 컷이 끝날 때까지 표시")]
        [Min(0f)] public float subtitleDuration;
        [Min(0f)] public float subtitleFadeDuration = 0.15f;

        [Header("선택 요소")]
        public Sprite overlayImage;
        public GameObject effectPrefab;
        [Min(0f)] public float effectDelay;
        public AudioClip soundEffect;
        [Min(0f)] public float soundDelay;
        [Range(0f, 1f)] public float soundVolume = 1f;

        // 손전등 경로가 있으면 모든 경로점의 시간을 합산합니다.
        // 경로가 비어 있으면 기존 단일 이동 방식의 duration을 사용합니다.
        public float ContentDuration
        {
            get
            {
                if (spotlightPath == null || spotlightPath.Count == 0)
                    return duration;

                float total = 0f;
                foreach (SpotlightStep step in spotlightPath)
                {
                    if (step != null)
                        total += step.TotalDuration;
                }

                return total;
            }
        }

        // 페이드 시간까지 포함한 이 컷의 실제 재생시간입니다.
        public float TotalDuration => fadeInDuration + ContentDuration + fadeOutDuration;
    }

    [Header("화면")]
    [SerializeField] private Image cutImage;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image overlayImage;
    [SerializeField] private Transform effectParent;
    [SerializeField] private AudioSource audioSource;

    [Header("손전등")]
    [Tooltip("비워두면 실행 시 자동 생성됩니다.")]
    [SerializeField] private Image spotlightOverlay;
    [SerializeField] private Color spotlightDarkColor = Color.black;
    [SerializeField] private Vector2 initialSpotlightPosition = new(0.5f, 0.5f);

    [Header("자막")]
    [Tooltip("비워두면 실행 시 자동 생성됩니다.")]
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_FontAsset subtitleFont;
    [SerializeField, Min(1f)] private float subtitleFontSize = 42f;
    [SerializeField] private Color subtitleColor = Color.white;
    [SerializeField, Min(0f)] private float subtitleBottomMargin = 60f;

    [Header("컷 설정")]
    [SerializeField] private IntroCut[] cuts;

    [Header("이동 및 입력")]
    [SerializeField] private string nextSceneName = "MainScene";
    [SerializeField] private bool allowSkip = true;
    [SerializeField, Min(0.01f)] private float playbackSpeed = 1f;

    // 매 프레임 문자열로 셰이더 속성을 검색하지 않도록 ID를 미리 저장합니다.
    private static readonly int SpotlightPositionId = Shader.PropertyToID("_SpotlightPosition");
    private static readonly int SpotlightSizeId = Shader.PropertyToID("_SpotlightSize");
    private static readonly int SpotlightSoftnessId = Shader.PropertyToID("_SpotlightSoftness");
    private static readonly int DarknessId = Shader.PropertyToID("_Darkness");

    // 실행 중에 사용하는 내부 상태입니다.
    private bool isFinished;
    private RectTransform cutRect;
    private Vector2 baseAnchoredPosition;
    private Vector3 baseScale;
    private Vector2 currentSpotlightPosition;
    private Material spotlightMaterial;
    private CanvasGroup subtitleCanvasGroup;

    // 커스텀 Editor가 컷 설정과 미리보기 영역을 읽을 때 사용합니다.
    public IntroCut[] Cuts => cuts;
    public RectTransform PreviewRect =>
        cutImage != null ? cutImage.rectTransform.parent as RectTransform : null;

    // Inspector에 표시할 예상 전체 재생시간입니다.
    // 이미지가 연결되지 않은 컷은 실제 재생에서도 건너뛰므로 합산하지 않습니다.
    public float EstimatedTotalDuration
    {
        get
        {
            float total = 0f;
            if (cuts != null)
            {
                foreach (IntroCut cut in cuts)
                {
                    if (cut != null && cut.image != null)
                        total += cut.TotalDuration;
                }
            }

            return total / Mathf.Max(0.01f, playbackSpeed);
        }
    }

    // 필요한 UI를 준비하고 등록된 컷을 순서대로 재생합니다.
    private IEnumerator Start()
    {
        if (cutImage == null || canvasGroup == null)
        {
            Debug.LogError("IntroController: Cut Image와 Canvas Group을 연결해 주세요.", this);
            LoadNextScene();
            yield break;
        }

        cutRect = cutImage.rectTransform;
        baseAnchoredPosition = cutRect.anchoredPosition;
        baseScale = cutRect.localScale;
        currentSpotlightPosition = Clamp01(initialSpotlightPosition);

        PrepareOverlayImage();
        PrepareSpotlightOverlay();
        PrepareSubtitleText();
        PreloadConfiguredAudio();

        if (cuts != null)
        {
            foreach (IntroCut cut in cuts)
            {
                if (cut == null || cut.image == null)
                    continue;

                yield return PlayCut(cut);
                if (isFinished)
                    yield break;
            }
        }

        LoadNextScene();
    }

    // 이미지 하나의 페이드, 손전등, 줌, 자막, 사운드 연출을 재생합니다.
    private IEnumerator PlayCut(IntroCut cut)
    {
        cutImage.sprite = cut.image;
        ResetImageTransform(cut);
        SetOverlay(cut.overlayImage);
        HideSubtitleImmediate();

        // 자막, 효과음, 프리팹 효과는 이미지 연출과 동시에 실행합니다.
        GameObject spawnedEffect = null;
        Coroutine effectRoutine = null;
        Coroutine soundRoutine = null;
        Coroutine subtitleRoutine = null;

        if (cut.effectPrefab != null)
            effectRoutine = StartCoroutine(PlayEffectAfterDelay(cut, effect => spawnedEffect = effect));
        if (cut.soundEffect != null)
            soundRoutine = StartCoroutine(PlaySoundAfterDelay(cut));
        // 경로점별 자막이 없을 때만 기존 컷 전체 자막을 사용합니다.
        if (!HasStepSubtitles(cut) && !string.IsNullOrWhiteSpace(cut.subtitle))
            subtitleRoutine = StartCoroutine(PlaySubtitle(cut));

        canvasGroup.alpha = 0f;
        yield return FadeCanvas(0f, 1f, cut.fadeInDuration);
        yield return AnimateCut(cut);
        yield return FadeCanvas(1f, 0f, cut.fadeOutDuration);

        // 다중 경로를 사용했다면 마지막 경로점이 이미 현재 위치로 저장되어 있습니다.
        if (cut.spotlightPath == null || cut.spotlightPath.Count == 0)
            currentSpotlightPosition = Clamp01(cut.spotlightPosition);
        StopOptionalCoroutine(effectRoutine);
        StopOptionalCoroutine(soundRoutine);
        StopOptionalCoroutine(subtitleRoutine);
        HideSubtitleImmediate();

        if (spawnedEffect != null)
            Destroy(spawnedEffect);
    }

    // 경로 목록의 유무에 따라 다중 이동 또는 기존 단일 이동을 선택합니다.
    private IEnumerator AnimateCut(IntroCut cut)
    {
        if (cut.spotlightPath != null && cut.spotlightPath.Count > 0)
        {
            yield return AnimateSpotlightPath(cut);
            yield break;
        }

        // 경로가 없을 때 사용하는 기존 단일 손전등 이동 방식입니다.
        float elapsed = 0f;
        float duration = Mathf.Max(0f, cut.duration);
        Vector2 spotlightStart = currentSpotlightPosition;
        Vector2 spotlightTarget = Clamp01(cut.spotlightPosition);

        ApplySpotlight(cut, spotlightStart);
        ApplyZoom(cut, 0f);

        while (elapsed < duration)
        {
            elapsed += ScaledUnscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / Mathf.Max(duration, 0.0001f));
            float moveProgress = cut.spotlightMoveDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / cut.spotlightMoveDuration);

            ApplySpotlight(cut, Vector2.Lerp(spotlightStart, spotlightTarget, Smooth(moveProgress)));
            ApplyZoom(cut, progress);
            yield return null;
        }

        ApplySpotlight(cut, spotlightTarget);
        ApplyZoom(cut, 1f);
    }

    // 한 컷 안의 모든 손전등 경로점을 순서대로 방문합니다.
    // 손전등 이동 중에는 줌 중심과 확대 배율도 다음 경로점 설정으로 함께 이동합니다.
    // 각 경로점에서 moveDuration 동안 이동하고 holdDuration 동안 머무릅니다.
    private IEnumerator AnimateSpotlightPath(IntroCut cut)
    {
        Vector2 startPosition = currentSpotlightPosition;
        Vector2 startZoomFocus = Clamp01(cut.zoomFocusPosition);
        float startZoom = Mathf.Max(0.01f, cut.zoomStart);

        foreach (SpotlightStep step in cut.spotlightPath)
        {
            if (step == null)
                continue;

            // 이 경로점의 이동이 시작되는 시점부터 전용 자막 타이머도 시작합니다.
            Coroutine stepSubtitleRoutine = null;
            if (!string.IsNullOrWhiteSpace(step.subtitle))
                stepSubtitleRoutine = StartCoroutine(PlayStepSubtitle(step));

            // 여러 효과음의 지연 타이머를 동시에 시작합니다.
            List<Coroutine> stepSoundRoutines = new();
            List<AudioSource> stepSoundSources = new();
            if (step.soundEvents != null)
            {
                foreach (SpotlightSoundEvent soundEvent in step.soundEvents)
                {
                    if (soundEvent == null || soundEvent.clip == null)
                        continue;

                    Coroutine soundRoutine = StartCoroutine(
                        PlayStepSound(step, soundEvent, source => stepSoundSources.Add(source)));
                    stepSoundRoutines.Add(soundRoutine);
                }
            }

            Vector2 targetPosition = Clamp01(step.position);
            // 이전 버전의 씬에는 새 줌 필드가 저장되어 있지 않아 zoom이 0입니다.
            // 이 경우에는 손전등 추적을 기본값으로 사용하고 컷의 Zoom End를 적용합니다.
            bool followsSpotlight = step.zoomFollowsSpotlight || step.zoom <= 0f;
            Vector2 targetZoomFocus = followsSpotlight
                ? targetPosition
                : Clamp01(step.zoomFocusPosition);
            float targetZoom = step.zoom > 0f
                ? step.zoom
                : Mathf.Max(0.01f, cut.zoomEnd);
            float moveElapsed = 0f;
            float moveDuration = Mathf.Max(0f, step.moveDuration);

            // 이동시간이 0이면 중간 애니메이션 없이 목표 위치로 즉시 이동합니다.
            if (moveDuration <= 0f)
            {
                ApplySpotlight(step, targetPosition);
                ApplyZoomAt(targetZoomFocus, targetZoom);
            }
            else
            {
                while (moveElapsed < moveDuration)
                {
                    float delta = ScaledUnscaledDeltaTime;
                    moveElapsed += delta;
                    float progress = Mathf.Clamp01(moveElapsed / moveDuration);
                    // Move Curve를 적용하여 일정 속도뿐 아니라 감속/가속 이동도 지원합니다.
                    float curvedProgress = step.moveCurve == null || step.moveCurve.length == 0
                        ? Smooth(progress)
                        : step.moveCurve.Evaluate(progress);

                    Vector2 movingSpotlightPosition =
                        Vector2.Lerp(startPosition, targetPosition, curvedProgress);
                    Vector2 movingZoomFocus = followsSpotlight
                        ? movingSpotlightPosition
                        : Vector2.Lerp(startZoomFocus, targetZoomFocus, curvedProgress);
                    float movingZoom = Mathf.Lerp(startZoom, targetZoom, curvedProgress);

                    ApplySpotlight(step, movingSpotlightPosition);
                    ApplyZoomAt(movingZoomFocus, movingZoom);
                    yield return null;
                }
            }

            ApplySpotlight(step, targetPosition);
            ApplyZoomAt(targetZoomFocus, targetZoom);
            startPosition = targetPosition;
            startZoomFocus = targetZoomFocus;
            startZoom = targetZoom;
            currentSpotlightPosition = targetPosition;

            // 목표 지점에 도착한 뒤 설정한 시간만큼 빛을 고정합니다.
            float holdElapsed = 0f;
            float holdDuration = Mathf.Max(0f, step.holdDuration);
            while (holdElapsed < holdDuration)
            {
                float delta = ScaledUnscaledDeltaTime;
                holdElapsed += delta;
                ApplySpotlight(step, targetPosition);
                ApplyZoomAt(targetZoomFocus, targetZoom);
                yield return null;
            }

            // 다음 경로점 자막과 겹치지 않도록 현재 자막을 정리합니다.
            StopOptionalCoroutine(stepSubtitleRoutine);
            HideSubtitleImmediate();

            // 아직 지연 중인 효과음은 다음 경로점에서 뒤늦게 재생되지 않도록 취소합니다.
            foreach (Coroutine soundRoutine in stepSoundRoutines)
                StopOptionalCoroutine(soundRoutine);

            // 반복 효과음만 경로점 종료 시 정지합니다.
            // 단발 효과음은 다음 경로점으로 넘어가도 원래 길이만큼 계속 재생됩니다.
            foreach (AudioSource source in stepSoundSources)
            {
                if (source != null && source.loop)
                {
                    source.Stop();
                    Destroy(source.gameObject);
                }
            }
        }
    }

    // 기존 단일 손전등 설정을 셰이더에 전달합니다.
    private void ApplySpotlight(IntroCut cut, Vector2 position)
    {
        // 단일 컷도 경로점과 같은 셰이더를 사용하므로 공통 메서드에 값만 전달합니다.
        ApplySpotlight(
            position,
            cut.spotlightSize,
            cut.spotlightSoftness,
            cut.darkness);
    }

    // 다중 경로의 현재 경로점 설정을 셰이더에 전달합니다.
    private void ApplySpotlight(SpotlightStep step, Vector2 position)
    {
        // SpotlightStep의 설정 형식만 다르고 실제 Material 갱신 과정은 단일 컷과 같습니다.
        ApplySpotlight(position, step.size, step.softness, step.darkness);
    }

    // 컷과 경로점이 공통으로 사용하는 손전등 셰이더 설정입니다.
    private void ApplySpotlight(Vector2 position, Vector2 size, float softness, float darkness)
    {
        if (spotlightMaterial == null)
            return;

        Vector2 clampedSize = new(
            Mathf.Max(0.001f, size.x),
            Mathf.Max(0.001f, size.y));

        // 크기와 부드러움이 0이면 셰이더 계산이 불안정해질 수 있어 최소값을 보장합니다.
        spotlightMaterial.SetVector(SpotlightPositionId, position);
        spotlightMaterial.SetVector(SpotlightSizeId, clampedSize);
        spotlightMaterial.SetFloat(SpotlightSoftnessId, Mathf.Max(0.001f, softness));
        spotlightMaterial.SetFloat(DarknessId, darkness);
        spotlightOverlay.color = spotlightDarkColor;
    }

    // 지정한 중심을 화면 가운데로 끌어오면서 이미지를 확대/축소합니다.
    // 손전등의 위치와 줌 중심은 서로 독립적으로 설정할 수 있습니다.
    private void ApplyZoom(IntroCut cut, float progress)
    {
        float curvedProgress = cut.zoomCurve == null ? progress : cut.zoomCurve.Evaluate(progress);
        float zoom = Mathf.Lerp(cut.zoomStart, cut.zoomEnd, curvedProgress);
        ApplyZoomAt(Clamp01(cut.zoomFocusPosition), zoom);
    }

    // 특정 화면 위치를 중심으로 지정한 배율만큼 이미지를 확대합니다.
    // 다중 손전등 경로에서는 매 프레임 이동 중인 빛의 위치를 이 메서드에 전달합니다.
    private void ApplyZoomAt(Vector2 focus, float zoom)
    {
        zoom = Mathf.Max(0.01f, zoom);
        Vector2 size = cutRect.rect.size;
        Vector2 focusOffset = new(0.5f - focus.x, 0.5f - focus.y);

        cutRect.localScale = baseScale * zoom;
        cutRect.anchoredPosition = baseAnchoredPosition
            + Vector2.Scale(focusOffset, size) * (zoom - 1f);
    }

    // 설정한 지연 후 자막을 페이드 인하고 표시시간이 지나면 페이드 아웃합니다.
    private IEnumerator PlaySubtitle(IntroCut cut)
    {
        yield return WaitScaled(cut.subtitleDelay);
        if (isFinished)
            yield break;

        subtitleText.text = cut.subtitle;
        subtitleText.gameObject.SetActive(true);
        yield return FadeSubtitle(0f, 1f, cut.subtitleFadeDuration);

        // 표시시간이 0이면 현재 컷이 끝날 때까지 자동으로 유지합니다.
        float visibleDuration = cut.subtitleDuration > 0f
            ? cut.subtitleDuration
            : Mathf.Max(0f, cut.ContentDuration - cut.subtitleDelay - cut.subtitleFadeDuration * 2f);
        yield return WaitScaled(visibleDuration);
        yield return FadeSubtitle(1f, 0f, cut.subtitleFadeDuration);
        subtitleText.gameObject.SetActive(false);
    }

    // 손전등 경로점 하나에 연결된 자막을 해당 경로점의 재생시간 안에서 표시합니다.
    private IEnumerator PlayStepSubtitle(SpotlightStep step)
    {
        // 이동 중에는 자막을 띄우지 않고, 손전등이 목표 위치에 도착한 뒤 표시합니다.
        yield return WaitScaled(step.moveDuration + step.subtitleDelay);
        if (isFinished)
            yield break;

        subtitleText.text = step.subtitle;
        subtitleText.gameObject.SetActive(true);
        yield return FadeSubtitle(0f, 1f, step.subtitleFadeDuration);

        // 표시시간이 0이면 손전등이 이 위치에 머무르는 동안 유지합니다.
        float visibleDuration = step.subtitleDuration > 0f
            ? step.subtitleDuration
            : Mathf.Max(0f, step.holdDuration - step.subtitleDelay - step.subtitleFadeDuration * 2f);
        yield return WaitScaled(visibleDuration);
        yield return FadeSubtitle(1f, 0f, step.subtitleFadeDuration);
        subtitleText.gameObject.SetActive(false);
    }

    // 하나라도 경로점 자막이 설정되어 있는지 확인합니다.
    private static bool HasStepSubtitles(IntroCut cut)
    {
        if (cut.spotlightPath == null)
            return false;

        foreach (SpotlightStep step in cut.spotlightPath)
        {
            if (step != null && !string.IsNullOrWhiteSpace(step.subtitle))
                return true;
        }

        return false;
    }

    // 경로점에 등록된 효과음 하나를 설정한 기준 시점과 지연시간에 맞춰 재생합니다.
    private IEnumerator PlayStepSound(
        SpotlightStep step,
        SpotlightSoundEvent soundEvent,
        Action<AudioSource> onStarted)
    {
        float startDelay = soundEvent.playOnArrival ? step.moveDuration : 0f;
        yield return WaitScaled(startDelay + soundEvent.delay);

        if (isFinished || soundEvent.clip == null)
            yield break;

        // Preload Audio Data가 꺼진 클립도 재생 시점에 확실히 준비되도록 보장합니다.
        if (soundEvent.clip.loadState == AudioDataLoadState.Unloaded)
            soundEvent.clip.LoadAudioData();

        while (soundEvent.clip.loadState == AudioDataLoadState.Loading)
            yield return null;

        if (soundEvent.clip.loadState == AudioDataLoadState.Failed)
        {
            Debug.LogWarning($"IntroController: 효과음을 불러오지 못했습니다. ({soundEvent.clip.name})", this);
            yield break;
        }

        // 효과음마다 별도의 AudioSource를 만들어 피치와 반복 여부를 독립적으로 제어합니다.
        GameObject soundObject = new($"Intro Sound - {soundEvent.clip.name}", typeof(AudioSource));
        soundObject.transform.SetParent(transform, false);

        AudioSource source = soundObject.GetComponent<AudioSource>();
        source.clip = soundEvent.clip;
        source.volume = soundEvent.volume;
        source.pitch = Mathf.Approximately(soundEvent.pitch, 0f) ? 0.01f : soundEvent.pitch;
        source.loop = soundEvent.loop;
        source.playOnAwake = false;
        source.spatialBlend = 0f;

        // 기존 AudioSource에 믹서 그룹이 연결되어 있으면 경로점 효과음에도 동일하게 적용합니다.
        if (audioSource != null)
            source.outputAudioMixerGroup = audioSource.outputAudioMixerGroup;

        source.Play();
        onStarted?.Invoke(source);

        if (!source.loop)
        {
            float playbackDuration = soundEvent.clip.length / Mathf.Max(0.01f, Mathf.Abs(source.pitch));
            Destroy(soundObject, playbackDuration + 0.1f);
        }
    }

    // 인트로가 시작될 때 모든 효과음의 오디오 데이터를 미리 요청합니다.
    // 실제 재생 시점의 디스크 로딩 지연으로 소리가 누락되는 것을 방지합니다.
    private void PreloadConfiguredAudio()
    {
        if (cuts == null)
            return;

        foreach (IntroCut cut in cuts)
        {
            if (cut == null)
                continue;

            PreloadAudioClip(cut.soundEffect);

            if (cut.spotlightPath == null)
                continue;

            foreach (SpotlightStep step in cut.spotlightPath)
            {
                if (step?.soundEvents == null)
                    continue;

                foreach (SpotlightSoundEvent soundEvent in step.soundEvents)
                {
                    if (soundEvent != null)
                        PreloadAudioClip(soundEvent.clip);
                }
            }
        }
    }

    private static void PreloadAudioClip(AudioClip clip)
    {
        // 이미 로딩 중이거나 완료된 클립에는 중복 요청을 보내지 않습니다.
        // 실제 재생 코루틴은 Loading 상태가 끝날 때까지 별도로 기다립니다.
        if (clip != null && clip.loadState == AudioDataLoadState.Unloaded)
            clip.LoadAudioData();
    }

    // 설정한 지연 후 선택한 이펙트 프리팹을 생성합니다.
    private IEnumerator PlayEffectAfterDelay(IntroCut cut, Action<GameObject> onSpawned)
    {
        yield return WaitScaled(cut.effectDelay);
        if (!isFinished && cut.effectPrefab != null)
        {
            GameObject effect = Instantiate(
                cut.effectPrefab,
                effectParent != null ? effectParent : transform);
            onSpawned?.Invoke(effect);
        }
    }

    // 설정한 지연 후 컷 효과음을 재생합니다.
    private IEnumerator PlaySoundAfterDelay(IntroCut cut)
    {
        yield return WaitScaled(cut.soundDelay);
        if (isFinished || cut.soundEffect == null)
            yield break;

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.PlayOneShot(cut.soundEffect, cut.soundVolume);
    }

    // Time.timeScale이 0이어도 인트로가 재생되도록 unscaledDeltaTime을 사용합니다.
    // playbackSpeed는 인트로만 별도로 빠르게 또는 느리게 재생합니다.
    private IEnumerator WaitScaled(float duration)
    {
        float elapsed = 0f;
        while (elapsed < Mathf.Max(0f, duration))
        {
            elapsed += ScaledUnscaledDeltaTime;
            yield return null;
        }
    }

    // 컷 이미지 전체의 투명도를 변경합니다.
    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        // Fade가 시간 계산을 담당하고 이 콜백은 Canvas 값 적용만 담당합니다.
        yield return Fade(from, to, duration, value => canvasGroup.alpha = value);
    }

    // 자막만 독립적으로 페이드합니다.
    private IEnumerator FadeSubtitle(float from, float to, float duration)
    {
        // 컷 이미지와 같은 보간 규칙을 사용하되 자막 CanvasGroup만 변경합니다.
        yield return Fade(from, to, duration, value => subtitleCanvasGroup.alpha = value);
    }

    // Canvas와 자막에서 공통으로 사용하는 알파 보간입니다.
    private IEnumerator Fade(float from, float to, float duration, Action<float> applyAlpha)
    {
        // 지속 시간이 0이면 한 프레임도 기다리지 않고 최종 알파를 즉시 적용합니다.
        if (duration <= 0f)
        {
            applyAlpha(to);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // timeScale과 무관하게 동작하며 playbackSpeed만 반영된 시간을 사용합니다.
            elapsed += ScaledUnscaledDeltaTime;
            applyAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        // 마지막 프레임의 오차와 관계없이 요청한 최종값을 정확히 보장합니다.
        applyAlpha(to);
    }

    // 선택 이미지 위에 추가 이미지를 겹칠 수 있는 UI를 준비합니다.
    private void PrepareOverlayImage()
    {
        if (overlayImage != null)
            return;

        GameObject overlayObject = CreateFullScreenImage("Intro Overlay Image");
        overlayImage = overlayObject.GetComponent<Image>();
        overlayImage.raycastTarget = false;
        overlayObject.SetActive(false);
    }

    // 화면 전체를 덮는 손전등 마스크 UI와 전용 Material을 준비합니다.
    // Inspector에서 연결하지 않아도 실행 시 자동으로 생성됩니다.
    private void PrepareSpotlightOverlay()
    {
        if (spotlightOverlay == null)
        {
            GameObject spotlightObject = CreateFullScreenImage("Intro Spotlight Overlay");
            spotlightOverlay = spotlightObject.GetComponent<Image>();
        }

        Shader shader = Shader.Find("UI/IntroSpotlight");
        if (shader == null)
        {
            Debug.LogError("IntroController: UI/IntroSpotlight 셰이더를 찾을 수 없습니다.", this);
            spotlightOverlay.gameObject.SetActive(false);
            return;
        }

        spotlightMaterial = new Material(shader)
        {
            name = "Intro Spotlight (Runtime)",
            hideFlags = HideFlags.DontSave
        };
        spotlightOverlay.material = spotlightMaterial;
        spotlightOverlay.color = spotlightDarkColor;
        spotlightOverlay.raycastTarget = false;
        spotlightOverlay.transform.SetAsLastSibling();
    }

    // Cut Image와 같은 부모 아래에 화면 전체 크기의 UI Image를 생성합니다.
    private GameObject CreateFullScreenImage(string objectName)
    {
        GameObject imageObject = new(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(cutImage.transform.parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return imageObject;
    }

    // 컷에 오버레이 이미지가 있을 때만 오버레이 UI를 표시합니다.
    private void SetOverlay(Sprite sprite)
    {
        if (overlayImage == null)
            return;

        overlayImage.sprite = sprite;
        overlayImage.gameObject.SetActive(sprite != null);
    }

    // 자막 TextMeshPro와 CanvasGroup을 준비하고 화면 아래쪽에 배치합니다.
    private void PrepareSubtitleText()
    {
        if (subtitleText == null)
        {
            GameObject subtitleObject = new(
                "Intro Subtitle",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI),
                typeof(CanvasGroup));
            subtitleObject.transform.SetParent(cutImage.transform.parent, false);
            subtitleText = subtitleObject.GetComponent<TextMeshProUGUI>();
        }

        subtitleCanvasGroup = subtitleText.GetComponent<CanvasGroup>();
        if (subtitleCanvasGroup == null)
            subtitleCanvasGroup = subtitleText.gameObject.AddComponent<CanvasGroup>();

        RectTransform rect = subtitleText.rectTransform;
        rect.anchorMin = new Vector2(0.08f, 0f);
        rect.anchorMax = new Vector2(0.92f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, subtitleBottomMargin);
        rect.sizeDelta = new Vector2(0f, 150f);

        if (subtitleFont != null)
            subtitleText.font = subtitleFont;
        subtitleText.fontSize = subtitleFontSize;
        subtitleText.color = subtitleColor;
        subtitleText.alignment = TextAlignmentOptions.Bottom;
        subtitleText.textWrappingMode = TextWrappingModes.Normal;
        subtitleText.raycastTarget = false;
        subtitleText.transform.SetAsLastSibling();
        HideSubtitleImmediate();
    }

    // 컷 전환 또는 스킵 시 이전 자막이 남지 않도록 즉시 숨깁니다.
    private void HideSubtitleImmediate()
    {
        if (subtitleText == null)
            return;

        subtitleText.text = string.Empty;
        subtitleText.gameObject.SetActive(false);
        if (subtitleCanvasGroup != null)
            subtitleCanvasGroup.alpha = 0f;
    }

    // 새 컷이 시작될 때 이전 컷의 위치와 확대 상태를 초기화합니다.
    private void ResetImageTransform(IntroCut cut)
    {
        cutRect.anchoredPosition = baseAnchoredPosition;
        cutRect.localScale = baseScale * Mathf.Max(0.01f, cut.zoomStart);
    }

    // 생성되지 않은 선택 코루틴도 안전하게 정리하기 위한 도우미입니다.
    private void StopOptionalCoroutine(Coroutine routine)
    {
        if (routine != null)
            StopCoroutine(routine);
    }

    // 스킵이 허용된 경우 키보드 또는 마우스 입력으로 다음 씬으로 이동합니다.
    private void Update()
    {
        if (!allowSkip || isFinished)
            return;

        // 입력 종류를 판정하는 코드와 실제 씬 전환 책임을 분리합니다.
        if (WasSkipRequested())
            LoadNextScene();
    }

    private static bool WasSkipRequested()
    {
        // 키보드 진행 키와 화면 클릭을 모두 동일한 스킵 요청으로 취급합니다.
        return Input.GetKeyDown(KeyCode.Space)
            || Input.GetKeyDown(KeyCode.Return)
            || Input.GetMouseButtonDown(0);
    }

    // 중복 호출을 막고 설정된 다음 씬을 한 번만 불러옵니다.
    private void LoadNextScene()
    {
        if (isFinished)
            return;

        isFinished = true;
        SceneManager.LoadScene(nextSceneName);
    }

    // 실행 중 생성한 Material이 메모리에 남지 않도록 정리합니다.
    private void OnDestroy()
    {
        if (spotlightMaterial != null)
            Destroy(spotlightMaterial);
    }

    // 인트로 전용 재생속도가 적용된 프레임 시간입니다.
    private float ScaledUnscaledDeltaTime => Time.unscaledDeltaTime * Mathf.Max(0.01f, playbackSpeed);

    // 화면 밖의 정규화 좌표가 들어오지 않도록 0~1 범위로 제한합니다.
    private static Vector2 Clamp01(Vector2 value)
    {
        return new Vector2(Mathf.Clamp01(value.x), Mathf.Clamp01(value.y));
    }

    // 이동의 시작과 끝을 부드럽게 만드는 Smoothstep 계산입니다.
    private static float Smooth(float value)
    {
        return value * value * (3f - 2f * value);
    }
}
