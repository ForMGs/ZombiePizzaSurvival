using System;
using System.Collections.Generic;

[Serializable]
public sealed class QuestRuntime
{
    public QuestData Data { get; }
    public QuestState State { get; internal set; }
    public float RemainingTime { get; internal set; }
    public IReadOnlyList<QuestObjectiveProgress> Objectives => objectives;

    private readonly List<QuestObjectiveProgress> objectives = new();

    public QuestRuntime(QuestData data)
    {
        Data = data;
        State = QuestState.Available;
        RemainingTime = data != null ? data.timeLimitSeconds : 0f;

        if (data?.objectives == null)
            return;

        foreach (QuestObjective objective in data.objectives)
        {
            if (objective != null)
                objectives.Add(new QuestObjectiveProgress(objective));
        }
    }

    public bool HasObjectives => objectives.Count > 0;

    public bool AreAllObjectivesComplete()
    {
        return objectives.Count > 0 && objectives.TrueForAll(objective => objective.IsComplete);
    }
}

[Serializable]
public sealed class QuestObjectiveProgress
{
    public QuestObjective Objective { get; }
    public int CurrentAmount { get; private set; }
    public int RequiredAmount => Objective.RequiredAmount;
    public bool IsComplete => CurrentAmount >= RequiredAmount;

    public QuestObjectiveProgress(QuestObjective objective)
    {
        Objective = objective;
    }

    public bool AddProgress(int amount)
    {
        if (amount <= 0 || IsComplete)
            return false;

        CurrentAmount = Math.Min(RequiredAmount, CurrentAmount + amount);
        return true;
    }

    public bool SetComplete()
    {
        return AddProgress(RequiredAmount);
    }
}
