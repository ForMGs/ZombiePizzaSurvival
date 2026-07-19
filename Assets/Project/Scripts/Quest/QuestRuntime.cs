using System;

[Serializable]
public sealed class QuestRuntime
{
    public QuestData Data { get; }
    public QuestState State { get; internal set; }
    public float RemainingTime { get; internal set; }

    public QuestRuntime(QuestData data)
    {
        Data = data;
        State = QuestState.Available;
        RemainingTime = data != null ? data.timeLimitSeconds : 0f;
    }
}
