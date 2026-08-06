using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public sealed class TutorialSequenceRunner : MonoBehaviour
{
    private const string SaveKeyPrefix = "ZombiePizzaSurvival.TutorialSequence.v1.";

    [Serializable]
    private sealed class CutsceneBinding
    {
        [Tooltip("SequenceData의 Cutscene Id와 같아야 합니다.")]
        [SerializeField] private string cutsceneId;
        [SerializeField] private PlayableDirector director;
        [Tooltip("이 컷신이 재생되는 동안 씬 BGM을 정지합니다.")]
        [SerializeField] private bool stopBgmDuringCutscene;

        public string CutsceneId => cutsceneId;
        public PlayableDirector Director => director;
        public bool StopBgmDuringCutscene => stopBgmDuringCutscene;
    }

    [Header("Sequence")]
    [Tooltip("가장 먼저 실행할 시퀀스입니다. 이후에는 각 SequenceData의 Next Sequence를 따라갑니다.")]
    [SerializeField] private TutorialSequenceData sequence;
    [SerializeField] private QuestManager questManager;

    [Header("Scene Cutscenes")]
    [Tooltip("SequenceData의 Cutscene Id와 씬의 PlayableDirector를 연결합니다.")]
    [SerializeField] private List<CutsceneBinding> cutscenes = new();

    [Header("Save")]
    [Tooltip("활성화하면 현재 단계 번호를 PlayerPrefs에 저장하고 다음 실행 때 복원합니다.")]
    [SerializeField] private bool saveProgress = true;

    private int currentStepIndex;
    private TutorialSequenceData currentSequence;
    private readonly HashSet<TutorialSequenceData> visitedSequences = new();
    private Coroutine runningStep;
    private PlayableDirector activeDirector;
    private DirectorUpdateMode activeDirectorPreviousUpdateMode;
    private bool activeCutsceneFinished;
    private bool hasStarted;
    private bool isAdvancing;
    private bool allSequencesFinished;
    private AudioClip pausedBgmClip;
    private float pausedBgmVolume;
    private bool shouldResumeBgm;

    public int CurrentStepIndex => currentStepIndex;
    public TutorialSequenceData CurrentSequence => currentSequence;
    public bool IsFinished => currentSequence != null &&
                              currentStepIndex >= currentSequence.Steps.Count;

    private void Awake()
    {
        if (questManager == null)
            questManager = FindFirstObjectByType<QuestManager>();

        currentSequence = sequence;
        if (currentSequence != null)
            visitedSequences.Add(currentSequence);
        LoadProgress();
    }

    private void OnEnable()
    {
        if (questManager != null)
            questManager.QuestChanged += OnQuestChanged;

        // Start가 한 번 실행된 뒤 오브젝트가 다시 활성화되면 현재 단계를 재개합니다.
        if (hasStarted)
            RunCurrentStep();
    }

    private void Start()
    {
        hasStarted = true;
        RunCurrentStep();
    }

    private void OnDisable()
    {
        if (questManager != null)
            questManager.QuestChanged -= OnQuestChanged;

        StopRunningStep();
    }

    private void RunCurrentStep()
    {
        if (!isActiveAndEnabled || runningStep != null || allSequencesFinished)
            return;
        if (!ValidateSequence())
            return;
        if (IsFinished)
        {
            CompleteCurrentSequence();
            return;
        }

        TutorialSequenceStep step = currentSequence.Steps[currentStepIndex];
        if (step == null)
        {
            Debug.LogWarning(
                $"튜토리얼 단계가 비어 있어 건너뜁니다: {currentStepIndex}", this);
            MoveNext();
            return;
        }

        Debug.Log(
            $"튜토리얼 단계 시작: {currentSequence.SequenceId} / " +
            $"{currentStepIndex} ({step.Type})",
            this);

        switch (step.Type)
        {
            case TutorialSequenceStepType.Quest:
                StartQuestStep(step);
                break;
            case TutorialSequenceStepType.Cutscene:
                StartCutsceneStep(step);
                break;
            case TutorialSequenceStepType.Delay:
                runningStep = StartCoroutine(PlayDelayStep(step.DelaySeconds));
                break;
            default:
                Debug.LogWarning($"지원하지 않는 튜토리얼 단계입니다: {step.Type}", this);
                MoveNext();
                break;
        }
    }

    private void StartQuestStep(TutorialSequenceStep step)
    {
        if (questManager == null)
        {
            Debug.LogError("TutorialSequenceRunner에 QuestManager가 없습니다.", this);
            return;
        }

        QuestData quest = step.Quest;
        if (quest == null)
        {
            Debug.LogWarning(
                $"QuestData가 없는 단계를 건너뜁니다: {currentStepIndex}", this);
            MoveNext();
            return;
        }

        questManager.RegisterQuest(quest);

        // 이전 플레이에서 이미 완료한 퀘스트라면 다시 수락하지 않고 다음 단계로 갑니다.
        if (questManager.IsCompleted(quest.questId))
        {
            MoveNext();
            return;
        }

        if (questManager.FindActiveQuest(quest.questId) != null)
            return;

        if (!step.AutoAccept)
        {
            Debug.Log(
                $"퀘스트 수락 대기: {quest.questId}. 퀘스트 메뉴에서 수락해야 합니다.",
                quest);
            return;
        }

        if (!questManager.AcceptQuest(quest))
        {
            Debug.LogWarning(
                $"튜토리얼 퀘스트를 자동 수락할 수 없습니다: {quest.questId}. " +
                "QuestData의 등장 조건을 확인하세요.",
                quest);
        }
    }

    private void OnQuestChanged(QuestRuntime runtime)
    {
        if (runtime == null || runtime.State != QuestState.Completed ||
            !ValidateSequence() || IsFinished)
            return;

        TutorialSequenceStep step = currentSequence.Steps[currentStepIndex];
        if (step == null || step.Type != TutorialSequenceStepType.Quest ||
            step.Quest != runtime.Data)
            return;

        MoveNext();
    }

    private void StartCutsceneStep(TutorialSequenceStep step)
    {
        CutsceneBinding binding = FindCutsceneBinding(step.CutsceneId);
        activeDirector = binding?.Director;
        if (activeDirector == null)
        {
            Debug.LogWarning(
                $"컷신 연결을 찾을 수 없어 단계를 건너뜁니다: '{step.CutsceneId}'",
                this);
            MoveNext();
            return;
        }

        activeCutsceneFinished = false;
        activeDirectorPreviousUpdateMode = activeDirector.timeUpdateMode;
        activeDirector.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
        activeDirector.stopped += OnCutsceneStopped;
        StopBgmForCutscene(binding);
        GameplayPause.Pause(this);
        activeDirector.Play();
        runningStep = StartCoroutine(WaitForCutscene());
    }

    private void OnCutsceneStopped(PlayableDirector director)
    {
        if (director == activeDirector)
            activeCutsceneFinished = true;
    }

    private IEnumerator WaitForCutscene()
    {
        yield return new WaitUntil(() => activeCutsceneFinished);

        UnsubscribeActiveDirector();
        runningStep = null;
        MoveNext();
    }

    private IEnumerator PlayDelayStep(float seconds)
    {
        if (seconds > 0f)
            yield return new WaitForSeconds(seconds);

        runningStep = null;
        MoveNext();
    }

    private CutsceneBinding FindCutsceneBinding(string cutsceneId)
    {
        if (string.IsNullOrWhiteSpace(cutsceneId))
            return null;

        foreach (CutsceneBinding binding in cutscenes)
        {
            if (binding != null &&
                string.Equals(binding.CutsceneId, cutsceneId, StringComparison.Ordinal))
                return binding;
        }

        return null;
    }

    private void StopBgmForCutscene(CutsceneBinding binding)
    {
        shouldResumeBgm = false;
        if (binding == null || !binding.StopBgmDuringCutscene || AudioManager.Instance == null)
            return;

        AudioManager audioManager = AudioManager.Instance;
        pausedBgmClip = audioManager.CurrentBgm;
        pausedBgmVolume = audioManager.RequestedVolume;
        shouldResumeBgm = pausedBgmClip != null && audioManager.IsPlaying;

        if (shouldResumeBgm)
            audioManager.StopBgm(0.3f);
    }

    private void ResumeBgmAfterCutscene()
    {
        if (!shouldResumeBgm || pausedBgmClip == null)
            return;

        AudioManager.GetOrCreate().PlayBgm(pausedBgmClip, pausedBgmVolume, 0.3f);
        shouldResumeBgm = false;
        pausedBgmClip = null;
    }

    private void MoveNext()
    {
        // 퀘스트 수락 직후 바로 완료되는 경우에도 중복 이동하지 않게 막습니다.
        if (isAdvancing)
            return;

        isAdvancing = true;
        currentStepIndex++;
        SaveProgress();
        isAdvancing = false;
        RunCurrentStep();
    }

    private void CompleteCurrentSequence()
    {
        Debug.Log($"튜토리얼 시퀀스 완료: {currentSequence.SequenceId}", this);

        TutorialSequenceData nextSequence = currentSequence.NextSequence;
        if (nextSequence == null)
        {
            allSequencesFinished = true;
            Debug.Log("연결된 모든 튜토리얼 시퀀스를 완료했습니다.", this);
            return;
        }

        // A -> B -> A처럼 잘못 연결하면 무한 반복되므로 실행을 중단합니다.
        if (!visitedSequences.Add(nextSequence))
        {
            allSequencesFinished = true;
            Debug.LogError(
                $"튜토리얼 시퀀스 순환 연결이 감지되었습니다: " +
                $"{currentSequence.SequenceId} -> {nextSequence.SequenceId}",
                nextSequence);
            return;
        }

        currentSequence = nextSequence;
        currentStepIndex = 0;
        LoadProgress();

        Debug.Log($"다음 튜토리얼 시퀀스로 이동: {currentSequence.SequenceId}", this);
        RunCurrentStep();
    }

    private bool ValidateSequence()
    {
        if (currentSequence != null)
            return true;

        Debug.LogError("TutorialSequenceRunner에 시작 SequenceData가 없습니다.", this);
        return false;
    }

    private void StopRunningStep()
    {
        if (runningStep != null)
        {
            StopCoroutine(runningStep);
            runningStep = null;
        }

        UnsubscribeActiveDirector();
    }

    private void UnsubscribeActiveDirector()
    {
        if (activeDirector != null)
        {
            activeDirector.stopped -= OnCutsceneStopped;
            activeDirector.timeUpdateMode = activeDirectorPreviousUpdateMode;
        }

        activeDirector = null;
        activeCutsceneFinished = false;
        ResumeBgmAfterCutscene();
        GameplayPause.Resume(this);
    }

    private string GetSaveKey()
    {
        return currentSequence == null
            ? string.Empty
            : SaveKeyPrefix + currentSequence.SequenceId;
    }

    private void SaveProgress()
    {
        if (!saveProgress || currentSequence == null)
            return;

        PlayerPrefs.SetInt(GetSaveKey(), currentStepIndex);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        if (!saveProgress || currentSequence == null)
        {
            currentStepIndex = 0;
            return;
        }

        currentStepIndex = Mathf.Clamp(
            PlayerPrefs.GetInt(GetSaveKey(), 0),
            0,
            currentSequence.Steps.Count);
    }

    [ContextMenu("Reset Tutorial Sequence Progress")]
    public void ResetProgress()
    {
        // 시작 시퀀스부터 Next Sequence로 연결된 모든 단계 저장을 초기화합니다.
        HashSet<TutorialSequenceData> resetSequences = new();
        TutorialSequenceData target = sequence;
        while (target != null && resetSequences.Add(target))
        {
            PlayerPrefs.DeleteKey(SaveKeyPrefix + target.SequenceId);
            target = target.NextSequence;
        }
        PlayerPrefs.Save();

        currentSequence = sequence;
        currentStepIndex = 0;
        allSequencesFinished = false;
        visitedSequences.Clear();
        if (currentSequence != null)
            visitedSequences.Add(currentSequence);
        StopRunningStep();

        if (Application.isPlaying && isActiveAndEnabled)
            RunCurrentStep();
    }
}
