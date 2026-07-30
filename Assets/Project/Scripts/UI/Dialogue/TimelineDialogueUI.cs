using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TimelineDialogueUI : MonoBehaviour
{
    [Header("대화창 내부 UI")]
    [SerializeField]
    private TMP_Text speakerNameText;

    [SerializeField]
    private TMP_Text dialogueText;

    [SerializeField]
    private Image portraitImage;

    [SerializeField]
    private Image nextMark;

    [Header("글자 출력 설정")]
    [SerializeField]
    [Min(0f)]
    private float typingInterval = 0.03f;

    // 현재 출력 중인 전체 문장
    private string currentFullText;

    // 현재 한 글자씩 출력 중인지 확인
    private bool isTyping;

    // 실행 중인 글자 출력 코루틴
    private Coroutine typingCoroutine;

    public bool IsTyping => isTyping;

    private void Awake()
    {
        if (nextMark != null)
        {
            nextMark.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 대화창에 새로운 대사를 표시합니다.
    /// </summary>
    public void ShowLine(
        string speakerName,
        string dialogue,
        Sprite portrait)
    {
        gameObject.SetActive(true);

        if (speakerNameText != null)
        {
            speakerNameText.text = speakerName;
        }

        if (portraitImage != null)
        {
            portraitImage.sprite = portrait;

            // 초상화가 없으면 Image 오브젝트를 숨깁니다.
            portraitImage.gameObject.SetActive(portrait != null);
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(
            TypeDialogue(dialogue));
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
            nextMark.gameObject.SetActive(false);
        }

        foreach (char character in currentFullText)
        {
            if (dialogueText != null)
            {
                dialogueText.text += character;
            }

            // Timeline이나 게임 시간이 멈춰도
            // 대사 글자는 계속 출력되도록 실시간 대기를 사용합니다.
            yield return new WaitForSecondsRealtime(typingInterval);
        }

        isTyping = false;
        typingCoroutine = null;

        if (nextMark != null)
        {
            nextMark.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 출력 중인 문장을 즉시 완성합니다.
    /// </summary>
    public void CompleteTyping()
    {
        if (!isTyping)
        {
            return;
        }

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null)
        {
            dialogueText.text = currentFullText;
        }

        isTyping = false;

        if (nextMark != null)
        {
            nextMark.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 대화창을 숨깁니다.
    /// </summary>
    public void Hide()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;

        if (nextMark != null)
        {
            nextMark.gameObject.SetActive(false);
        }

        gameObject.SetActive(false);
    }
}