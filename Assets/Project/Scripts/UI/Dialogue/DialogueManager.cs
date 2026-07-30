using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public sealed class DialogueManager : MonoBehaviour
{
    [Serializable]
    private sealed class DialogueLine
    {
        [Header("말하는 사람")]
        public string speakerName;

        [Header("초상화")]
        public Sprite portrait;

        [Header("대사")]
        [TextArea(2, 5)]
        public string dialogue;
    }

    [Header("대화창 프리팹")]
    [SerializeField]
    private TimelineDialogueUI dialogueUIPrefab;

    [Header("대화창을 생성할 Canvas")]
    [SerializeField]
    private Transform dialogueUIParent;

    [Header("현재 Timeline")]
    [SerializeField]
    private PlayableDirector director;

    [Header("대사 목록")]
    [SerializeField]
    private DialogueLine[] lines;

    // 생성된 대화창 인스턴스
    private TimelineDialogueUI dialogueUIInstance;

    private int currentLineIndex;
    private bool isDialogueActive;
    private IReadOnlyList<DialogueData.Line> activeDataLines;
    private PlayableDirector activeDirector;

    private void OnDisable()
    {
        GameplayPause.Resume(this);
    }

    private void Awake()
    {
        CreateDialogueUI();
    }

    private void Update()
    {
        if (!isDialogueActive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetMouseButtonDown(0))
        {
            AdvanceDialogue();
        }
    }

    /// <summary>
    /// 대화창 프리팹을 한 번 생성합니다.
    /// </summary>
    private void CreateDialogueUI()
    {
        if (dialogueUIPrefab == null)
        {
            Debug.LogError(
                "Dialogue UI Prefab이 연결되지 않았습니다.");
            return;
        }

        // 프리팹을 Canvas의 자식으로 생성합니다.
        dialogueUIInstance = Instantiate(
            dialogueUIPrefab,
            dialogueUIParent);

        // 처음에는 대화창을 숨겨 놓습니다.
        dialogueUIInstance.Hide();
    }

    /// <summary>
    /// Timeline의 Signal Receiver에서 호출합니다.
    /// </summary>
    public void StartDialogue()
    {
        if (isDialogueActive)
        {
            return;
        }

        if (dialogueUIInstance == null)
        {
            CreateDialogueUI();
        }

        if (dialogueUIInstance == null)
        {
            return;
        }

        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("등록된 대사가 없습니다.");
            return;
        }

        isDialogueActive = true;
        currentLineIndex = 0;
        activeDataLines = null;
        activeDirector = director;
        GameplayPause.Pause(this);

        // 대화가 진행되는 동안 Timeline 정지
        if (activeDirector != null)
        {
            activeDirector.Pause();
        }

        ShowCurrentLine();
    }

    public void PlayDialogue(
        DialogueData dialogueData,
        PlayableDirector sourceDirector = null)
    {
        if (isDialogueActive)
        {
            Debug.LogWarning("다른 대화가 이미 진행 중입니다.", this);
            return;
        }

        if (dialogueData == null ||
            dialogueData.Lines == null ||
            dialogueData.Lines.Count == 0)
        {
            Debug.LogWarning("재생할 DialogueData에 대사가 없습니다.", dialogueData);
            return;
        }

        if (dialogueUIInstance == null)
            CreateDialogueUI();

        if (dialogueUIInstance == null)
            return;

        isDialogueActive = true;
        currentLineIndex = 0;
        activeDataLines = dialogueData.Lines;
        activeDirector = sourceDirector != null ? sourceDirector : director;
        GameplayPause.Pause(this);

        if (activeDirector != null)
            activeDirector.Pause();

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (activeDataLines != null)
        {
            DialogueData.Line dataLine = activeDataLines[currentLineIndex];
            dialogueUIInstance.ShowLine(
                dataLine.speakerName,
                dataLine.dialogue,
                dataLine.portrait);
            return;
        }

        DialogueLine currentLine =
            lines[currentLineIndex];

        dialogueUIInstance.ShowLine(
            currentLine.speakerName,
            currentLine.dialogue,
            currentLine.portrait);
    }

    private void AdvanceDialogue()
    {
        // 글자가 아직 출력되고 있다면
        // 현재 문장을 즉시 완성합니다.
        if (dialogueUIInstance.IsTyping)
        {
            dialogueUIInstance.CompleteTyping();
            return;
        }

        currentLineIndex++;

        // 모든 대사를 출력했다면 종료합니다.
        int lineCount = activeDataLines != null
            ? activeDataLines.Count
            : lines.Length;

        if (currentLineIndex >= lineCount)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void EndDialogue()
    {
        isDialogueActive = false;

        if (dialogueUIInstance != null)
        {
            dialogueUIInstance.Hide();
        }

        // 멈췄던 Timeline을 다시 진행합니다.
        if (activeDirector != null)
        {
            activeDirector.Resume();
        }

        activeDataLines = null;
        activeDirector = null;
        GameplayPause.Resume(this);
    }
}
