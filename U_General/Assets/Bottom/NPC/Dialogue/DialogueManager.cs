using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("UI 组件")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;

    [Header("打字机设置")]
    public float typingSpeed = 0.05f;

    [Header("自动播放设置")]
    public bool autoPlay = true;
    public float autoNextDelay = 2f;

    [Header("计时器控制")]
    public TimerController timerController; // TimerController引用

    private string[] dialogueLines;
    private int currentLineIndex = 0;
    private Coroutine typingCoroutine;
    private Coroutine autoNextCoroutine;
    private bool isTyping = false;
    private bool lineFullyDisplayed = false;
    private bool isDialogueActive = false;

    // 标记是否已暂停计时器（保留计时器控制，移除时间系统控制）
    private bool hasTimerPaused = false;

    void Start()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // 自动查找TimerController（如果没有手动分配的话）
        if (timerController == null)
        {
            timerController = FindObjectOfType<TimerController>();
            if (timerController == null)
            {
                Debug.LogWarning("⚠️ 未找到TimerController组件");
            }
        }
    }

    public void StartDialogue(string[] lines)
    {
        if (lines == null || lines.Length == 0)
        {
            Debug.LogWarning("⚠️ 对话内容为空");
            return;
        }

        Debug.Log($"🎭 开始对话");

        dialogueLines = lines;
        currentLineIndex = 0;
        isDialogueActive = true;

        // 只暂停计时器，不再管理时间系统（时间系统由TimeSystem_7自己管理）
        if (timerController != null && !timerController.IsDialoguePaused())
        {
            timerController.PauseForDialogue();
            hasTimerPaused = true;
            Debug.Log("🎭 对话开始，计时器已暂停");
        }

        dialoguePanel.SetActive(true);
        nextButton.gameObject.SetActive(true);

        ShowLine();
    }

    public void ResetDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (autoNextCoroutine != null) StopCoroutine(autoNextCoroutine);

        dialogueText.text = "";
        currentLineIndex = 0;
        isTyping = false;
        lineFullyDisplayed = false;
        isDialogueActive = false;

        // 重置计时器暂停标记（但不恢复计时器，因为可能是中途重置）
        hasTimerPaused = false;
    }

    void ShowLine()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (autoNextCoroutine != null) StopCoroutine(autoNextCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(dialogueLines[currentLineIndex]));
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        lineFullyDisplayed = false;
        dialogueText.text = "";

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        lineFullyDisplayed = true;

        if (autoPlay)
        {
            autoNextCoroutine = StartCoroutine(AutoPlayNextLine());
        }
    }

    IEnumerator AutoPlayNextLine()
    {
        yield return new WaitForSeconds(autoNextDelay);

        if (!isTyping && lineFullyDisplayed)
        {
            GoToNextLine();
        }
    }

    public void OnNextButtonClicked()
    {
        if (!isDialogueActive || isTyping) return;

        if (lineFullyDisplayed)
        {
            GoToNextLine();
        }
    }

    void GoToNextLine()
    {
        currentLineIndex++;
        if (currentLineIndex < dialogueLines.Length)
        {
            ShowLine();
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        nextButton.gameObject.SetActive(false);
        isDialogueActive = false;

        Debug.Log($"🎭 对话结束");

        // 只恢复计时器，时间系统由MultiTimeDialogueTrigger和TimeSystem_7协调管理
        if (timerController != null && hasTimerPaused)
        {
            // 延迟一帧恢复计时器，确保其他系统先处理
            StartCoroutine(DelayedResumeTimer());
        }
    }

    // 强制结束对话的方法（用于紧急情况）
    public void ForceEndDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (autoNextCoroutine != null) StopCoroutine(autoNextCoroutine);

        EndDialogue();
    }

    // 检查对话是否正在进行中
    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    // 当对象被销毁时，确保恢复计时器
    void OnDestroy()
    {
        // 只处理计时器，不再处理时间系统
        if (timerController != null && hasTimerPaused)
        {
            timerController.ResumeFromDialogue();
            Debug.Log("🎭 DialogueManager被销毁，强制恢复计时器");
        }
    }

    private IEnumerator DelayedResumeTimer()
    {
        yield return null; // 等待一帧

        if (timerController != null && timerController.IsDialoguePaused())
        {
            timerController.ResumeFromDialogue();
            hasTimerPaused = false;
            Debug.Log("🎭 对话结束，计时器已恢复");
        }
    }
}