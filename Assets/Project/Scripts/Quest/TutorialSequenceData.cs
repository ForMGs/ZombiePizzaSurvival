using System;
using System.Collections.Generic;
using UnityEngine;

public enum TutorialSequenceStepType
{
    Quest,
    Cutscene,
    Delay
}

[Serializable]
public sealed class TutorialSequenceStep
{
    [SerializeField] private TutorialSequenceStepType type;

    [Header("Quest")]
    [Tooltip("Quest 단계에서 실행할 QuestData입니다.")]
    [SerializeField] private QuestData quest;
    [Tooltip("단계가 시작되면 퀘스트를 자동으로 수락합니다.")]
    [SerializeField] private bool autoAccept = true;

    [Header("Cutscene")]
    [Tooltip("TutorialSequenceRunner의 Cutscenes 목록에 등록한 ID와 같아야 합니다.")]
    [SerializeField] private string cutsceneId;

    [Header("Delay")]
    [Tooltip("Delay 단계에서 다음 단계로 넘어가기 전까지 기다릴 시간입니다.")]
    [Min(0f)] [SerializeField] private float delaySeconds;

    public TutorialSequenceStepType Type => type;
    public QuestData Quest => quest;
    public bool AutoAccept => autoAccept;
    public string CutsceneId => cutsceneId;
    public float DelaySeconds => delaySeconds;
}

[CreateAssetMenu(
    fileName = "New Tutorial Sequence",
    menuName = "Game/Tutorial Sequence")]
public sealed class TutorialSequenceData : ScriptableObject
{
    [Tooltip("저장 데이터를 구분하는 고유 ID입니다. 다른 시퀀스와 중복되면 안 됩니다.")]
    [SerializeField] private string sequenceId;
    [SerializeField] private List<TutorialSequenceStep> steps = new();
    [Tooltip("현재 시퀀스가 끝난 뒤 자동으로 실행할 다음 시퀀스입니다.")]
    [SerializeField] private TutorialSequenceData nextSequence;

    public string SequenceId =>
        string.IsNullOrWhiteSpace(sequenceId) ? name : sequenceId;
    public IReadOnlyList<TutorialSequenceStep> Steps => steps;
    public TutorialSequenceData NextSequence => nextSequence;
}
