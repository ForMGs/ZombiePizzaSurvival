using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public sealed class TimeLineDialogueController : MonoBehaviour
{
    [SerializeField] 
    private sealed class DialogueLine
    {
        [Header("캐릭터 이름")]
        public string speakerName;

        [Header("캐릭터 초상화")]
        public Sprite portrait;

        [Header("실제 대사")]
        public string dialogue;
    };
    
    [Header("Timeline")]
    [SerializeField]
    private PlayableDirector director;

    [Header("대화창 UI")]
    [SerializeField]
    private GameObject dialoguePanel;

    [SerializeField]
    private TMP_Text speakerNameText;

    [SerializeField]
    private TMP_Text dialogueText;

    [SerializeField]
    private Image portraitImage;

    [SerializeField]
    private GameObject nextMark;

    [Header("대사 목록")]
    [SerializeField]
    private DialogueLine[] lines;

    [Header("한 글자가 출력되는 간격")]
    [SerializeField]
    [Min(0f)]
    private float typingInterval = 0.03f;

    // 현재 표시 중인 대사의 배열 번호입니다.
    private int currentLineIndex;

    // 현재 대화가 진행 중인지 나타냅니다.
    private bool isDialogueActive;

    // 현재 글자가 한 글자씩 출력 중인지 나타냅니다.
    private bool isTyping;

    // 현재 대사의 전체 문장을 저장합니다.
    private string currentFullText;

    // 글자 출력 코루틴을 저장합니다.
    private Coroutine typingCoroutine;


    private void Awake()
    {
        // Inspector에서 Director를 연결하지 않았다면
        // 현재 오브젝트에 있는 PlayableDirector를 찾습니다.
        if (director == null)
        {
            director = GetComponent<PlayableDirector>();
        }

        // 게임 시작 시 대화창을 숨깁니다.
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (nextMark != null)
        {
            nextMark.SetActive(false);
        }
    }

    private void Update()
    {
        // 대화가 진행 중이 아니면 입력을 확인하지 않습니다.
        if (!isDialogueActive)
        {
            return;
        }

        // 스페이스바 또는 마우스 왼쪽 버튼으로 대사를 넘깁니다.
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetMouseButtonDown(0))
        {
            AdvanceDialogue();
        }
    }

    // Timeline의 Signal에서 이 함수를 호출합니다.
    public void StartDialogue()
    {
        // 이미 대화 중이면 중복 실행하지 않습니다.
        if (isDialogueActive)
        {
            return;
        }

        // 대사가 없으면 실행하지 않습니다.
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("등록된 대사가 없습니다.");
            return;
        }

        isDialogueActive = true;
        currentLineIndex = 0;

        // 대화가 진행되는 동안 Timeline을 일시정지합니다.
        if (director != null)
        {
            director.Pause();
        }

        // 대화창을 표시합니다.
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        ShowCurrentLine();
    }

    private void AdvanceDialogue()
    {
        // 아직 글자가 출력 중이라면
        // 다음 대사로 넘어가지 않고 현재 문장 전체를 표시합니다.
        if (isTyping)
        {
            CompleteTypingImmediately();
            return;
        }

        // 다음 대사로 이동합니다.
        currentLineIndex++;

        // 마지막 대사까지 모두 표시했다면 대화를 종료합니다.
        if (currentLineIndex >= lines.Length)
        {
            EndDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        DialogueLine currentLine = lines[currentLineIndex];

        // 말하는 사람의 이름을 표시합니다.
        if (speakerNameText != null)
        {
            speakerNameText.text = currentLine.speakerName;
        }

        // 초상화가 등록되어 있다면 표시합니다.
        if (portraitImage != null)
        {
            portraitImage.sprite = currentLine.portrait;
            portraitImage.gameObject.SetActive(
                currentLine.portrait != null);
        }

        // 이전 글자 출력 코루틴이 남아 있다면 정지합니다.
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        // 현재 대사를 한 글자씩 출력합니다.
        typingCoroutine = StartCoroutine(
            TypeDialogue(currentLine.dialogue));
    }

    private IEnumerator TypeDialogue(string dialogue)
    {
        isTyping = true;
        currentFullText = dialogue ?? string.Empty;

        if (dialogueText != null)
        {
            dialogueText.text = string.Empty;
        }

        if (nextMark != null)
        {
            nextMark.SetActive(false);
        }

        // 전체 문장을 한 글자씩 순서대로 표시합니다.
        foreach (char character in currentFullText)
        {
            if (dialogueText != null)
            {
                dialogueText.text += character;
            }

            // Timeline이 정지되어도 글자는 출력되어야 하므로
            // WaitForSecondsRealtime을 사용합니다.
            yield return new WaitForSecondsRealtime(typingInterval);
        }

        isTyping = false;
        typingCoroutine = null;

        // 글자 출력이 끝나면 다음 대사 표시를 활성화합니다.
        if (nextMark != null)
        {
            nextMark.SetActive(true);
        }
    }
    private void CompleteTypingImmediately()
    {
        // 진행 중인 글자 출력 코루틴을 중지합니다.
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // 현재 대사의 전체 내용을 바로 표시합니다.
        if (dialogueText != null)
        {
            dialogueText.text = currentFullText;
        }

        isTyping = false;

        if (nextMark != null)
        {
            nextMark.SetActive(true);
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;

        // 대화창을 숨깁니다.
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (nextMark != null)
        {
            nextMark.SetActive(false);
        }

        // 일시정지했던 Timeline을 다시 재생합니다.
        if (director != null)
        {
            director.Resume();
        }
    }

}