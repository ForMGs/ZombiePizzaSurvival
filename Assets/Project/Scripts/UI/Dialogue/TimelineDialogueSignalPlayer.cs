using UnityEngine;
using UnityEngine.Playables;

public sealed class TimelineDialogueSignalPlayer : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueData dialogue;
    [SerializeField] private PlayableDirector director;

    private void Reset()
    {
        director = GetComponent<PlayableDirector>();
        dialogueManager = FindFirstObjectByType<DialogueManager>();
    }

    /// <summary>
    /// Timeline의 Signal Receiver에서 호출합니다.
    /// </summary>
    public void Play()
    {
        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager가 연결되지 않았습니다.", this);
            return;
        }

        if (dialogue == null)
        {
            Debug.LogError("DialogueData가 연결되지 않았습니다.", this);
            return;
        }

        dialogueManager.PlayDialogue(dialogue, director);
    }
}
