using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Dialogue",
    menuName = "Game/Dialogue Data")]
public sealed class DialogueData : ScriptableObject
{
    [Serializable]
    public sealed class Line
    {
        [Header("말하는 사람")]
        public string speakerName;

        [Header("초상화")]
        public Sprite portrait;

        [Header("대사")]
        [TextArea(2, 5)]
        public string dialogue;
    }

    [SerializeField] private List<Line> lines = new();

    public IReadOnlyList<Line> Lines => lines;
}
