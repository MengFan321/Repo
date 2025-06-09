using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class DialogueTriggerInfo
{
    public string triggerDate;           // 触发时间 "yyyy-MM"
    public GameObject speechBubble;      // 对应气泡
    public Button bubbleButton;          // 气泡上的按钮
    public GameObject dialogueUI;        // 对话框UI
    public DialogueManager DialogueManager; // 你现有的对话管理器（挂在dialogueUI上）
    public string[] dialogueLines;       // 该时间点对应的多句对话内容

    [Header("调试信息")]
    public bool hasBeenTriggered = false; // 调试用：是否已触发过
    public bool isCompleted = false;     // 新增：是否已完成对话

    [Header("新增：唯一标识")]
    public string uniqueId;              // 新增：每个触发器的唯一标识

    [Header("NPC形象控制")]
    public GameObject npcPanelLeft;   // 左侧NPC形象
    public GameObject npcPanelRight;  // 右侧NPC形象

}

public class MultiTimeDialogueTrigger : MonoBehaviour
{
    [Header("对话触发器配置")]
    public DialogueTriggerInfo[] dialogueTriggers;

    [Header("调试设置")]
    public bool enableDebugMode = true;
    public bool logDetailedInfo = true;

    // ───── 新增：播放气泡音效的字段 ─────
    [Header("UI 音效")]
    public AudioSource uiAudioSource;    // 拖一个场景中带 AudioSource 的物体进来
    public AudioClip bubbleClip;         // 气泡弹出时要播放的音效（.wav/.mp3）
    public AudioClip bubbleClickClip;    // ── 新增：点击气泡时播放的音效


    private DialogueTriggerInfo currentActiveTrigger = null;
    private string lastCheckedDate = "";
    private bool isDialogueSystemBusy = false;

    // 防重复触发的安全机制
    private float lastTriggerTime = 0f;
    private const float MIN_TRIGGER_INTERVAL = 1f;

    // 触发器索引映射，避免引用混乱
    private Dictionary<string, DialogueTriggerInfo> triggerLookup = new Dictionary<string, DialogueTriggerInfo>();
    private Dictionary<DialogueTriggerInfo, int> triggerToIndex = new Dictionary<DialogueTriggerInfo, int>();

    void Start()
    {
        // —— ① 确保拿到 AudioSource —— 
        if (uiAudioSource == null)
            uiAudioSource = GetComponent<AudioSource>();

        // —— ② 预热音效，强制解码到内存 —— 
        if (bubbleClip != null)
            bubbleClip.LoadAudioData();
        if (bubbleClickClip != null)
            bubbleClickClip.LoadAudioData();

        // —— ③ 继续初始化对话系统 —— 
        InitializeDialogueSystem();
    }

    void InitializeDialogueSystem()
    {
        DebugLog("=== 初始化对话系统 ===");

        // 清空映射字典
        triggerLookup.Clear();
        triggerToIndex.Clear();

        for (int i = 0; i < dialogueTriggers.Length; i++)
        {
            var trigger = dialogueTriggers[i];

            // 生成唯一ID（如果没有设置）
            if (string.IsNullOrEmpty(trigger.uniqueId))
            {
                trigger.uniqueId = $"trigger_{i}_{trigger.triggerDate}";
            }

            // 验证配置完整性
            if (ValidateTriggerConfiguration(trigger, i))
            {
                SetupTriggerUI(trigger, i);

                // 建立映射关系
                string lookupKey = $"{trigger.triggerDate}_{i}"; // 组合键：日期+索引
                triggerLookup[lookupKey] = trigger;
                triggerToIndex[trigger] = i;

                DebugLog($"注册触发器[{i}] - 日期: {trigger.triggerDate}, ID: {trigger.uniqueId}");
            }
        }

        // 验证是否有重复的触发日期
        ValidateForDuplicateDates();

        DebugLog($"对话系统初始化完成，共配置 {dialogueTriggers.Length} 个触发器");
    }

    void ValidateForDuplicateDates()
    {
        Dictionary<string, List<int>> dateToIndices = new Dictionary<string, List<int>>();

        for (int i = 0; i < dialogueTriggers.Length; i++)
        {
            string date = dialogueTriggers[i].triggerDate;
            if (!dateToIndices.ContainsKey(date))
            {
                dateToIndices[date] = new List<int>();
            }
            dateToIndices[date].Add(i);
        }

        foreach (var kvp in dateToIndices)
        {
            if (kvp.Value.Count > 1)
            {
                string indices = string.Join(", ", kvp.Value);
                Debug.LogError($"⚠️ 重复日期警告: 日期 '{kvp.Key}' 在以下触发器中重复出现: 索引[{indices}]");
                Debug.LogError($"这将导致对话内容错乱！请检查配置。");
            }
        }
    }

    bool ValidateTriggerConfiguration(DialogueTriggerInfo trigger, int index)
    {
        bool isValid = true;
        string triggerInfo = $"触发器[{index}] ({trigger.triggerDate})";

        if (string.IsNullOrEmpty(trigger.triggerDate))
        {
            Debug.LogError($"{triggerInfo}: 触发日期为空！");
            isValid = false;
        }

        if (trigger.speechBubble == null)
        {
            Debug.LogError($"{triggerInfo}: 气泡GameObject未分配！");
            isValid = false;
        }

        if (trigger.bubbleButton == null)
        {
            Debug.LogError($"{triggerInfo}: 气泡按钮未分配！");
            isValid = false;
        }

        if (trigger.dialogueUI == null)
        {
            Debug.LogError($"{triggerInfo}: 对话UI未分配！");
            isValid = false;
        }

        if (trigger.DialogueManager == null)
        {
            Debug.LogError($"{triggerInfo}: DialogueManager未分配！");
            isValid = false;
        }

        if (trigger.dialogueLines == null || trigger.dialogueLines.Length == 0)
        {
            Debug.LogError($"{triggerInfo}: 对话内容为空！");
            isValid = false;
        }

        return isValid;
    }

    void SetupTriggerUI(DialogueTriggerInfo trigger, int index)
    {
        // 初始化UI状态
        if (trigger.speechBubble != null)
            trigger.speechBubble.SetActive(false);
        if (trigger.dialogueUI != null)
            trigger.dialogueUI.SetActive(false);
        // ✅ 新增：隐藏 NPC 面板
        if (trigger.npcPanelLeft != null)
            trigger.npcPanelLeft.SetActive(false);
        if (trigger.npcPanelRight != null)
            trigger.npcPanelRight.SetActive(false);

        // 设置按钮监听
        if (trigger.bubbleButton != null)
        {
            // 清除之前的监听器避免重复绑定
            trigger.bubbleButton.onClick.RemoveAllListeners();

            // 创建本地副本，确保闭包捕获正确的值
            DialogueTriggerInfo localTrigger = trigger;
            int localIndex = index;

            trigger.bubbleButton.onClick.AddListener(() => {
                DebugLog($"按钮点击事件触发 - 捕获的索引: {localIndex}, 触发器日期: {localTrigger.triggerDate}");
                OnBubbleClicked(localTrigger, localIndex);
            });

            DebugLog($"触发器[{index}] 按钮监听器设置完成");
        }
    }

    void Update()
    {
        // 原有的时间检查逻辑保留，但不再主动暂停时间系统
        CheckTimeBasedTriggers();
    }

    void CheckTimeBasedTriggers()
    {
        // 检查时间系统可用性
        if (TimeSystem_7.Instance == null)
        {
            if (enableDebugMode && Time.frameCount % 300 == 0) // 每5秒提醒一次
            {
                Debug.LogWarning("TimeSystem_7.Instance 为空，无法检查时间触发");
            }
            return;
        }

        string currentDate = TimeSystem_7.Instance.CurrentDateString;

        // 避免重复处理相同时间
        if (currentDate == lastCheckedDate)
            return;

        DebugLog($"时间变化检测: {lastCheckedDate} -> {currentDate}");
        lastCheckedDate = currentDate;

        // 注意：这里不再主动暂停时间系统，而是等待TimeSystem的通知
    }

    // 新增：供TimeSystem调用，检查指定日期是否有对话
    public bool IsDialogueMonth(string dateString)
    {
        for (int i = 0; i < dialogueTriggers.Length; i++)
        {
            if (dialogueTriggers[i].triggerDate == dateString && !dialogueTriggers[i].isCompleted)
            {
                return true;
            }
        }
        return false;
    }

    // 新增：供TimeSystem调用，通知时间系统已为对话暂停
    public void OnTimeSystemPausedForDialogue(string dateString)
    {
        DebugLog($"🔔 收到时间系统暂停通知，日期: {dateString}");

        // 查找对应的触发器并激活
        DialogueTriggerInfo trigger = FindTriggerByDate(dateString);
        if (trigger != null && !trigger.isCompleted)
        {
            ActivateDialogueTrigger(trigger);
        }
        else
        {
            Debug.LogWarning($"⚠️ 未找到日期 {dateString} 对应的未完成触发器");
        }
    }

    // 新增：根据日期查找触发器
    private DialogueTriggerInfo FindTriggerByDate(string dateString)
    {
        for (int i = 0; i < dialogueTriggers.Length; i++)
        {
            if (dialogueTriggers[i].triggerDate == dateString)
            {
                return dialogueTriggers[i];
            }
        }
        return null;
    }

    // 修改：激活对话触发器（原ActivateNewTrigger的简化版）
    void ActivateDialogueTrigger(DialogueTriggerInfo trigger)
    {
        try
        {
            // 关闭当前活跃的触发器
            DeactivateAllNpcPanels();

            DeactivateCurrentTrigger();

            if (trigger.speechBubble != null)
            {
                //1.播放音效
                if (uiAudioSource != null && bubbleClip != null)
                {
                    uiAudioSource.PlayOneShot(bubbleClip);
                }

                // 2. 弹出气泡
                trigger.speechBubble.SetActive(true);
                trigger.hasBeenTriggered = true;
                // ───【新增部分】同时把对话框也打开（但不开始播放文字）
                if (trigger.dialogueUI != null)
                {
                    trigger.dialogueUI.SetActive(true);
                }

                DebugLog($"🔼 已激活气泡& 对话框: ID[{trigger.uniqueId}], 日期[{trigger.triggerDate}], 对话行数[{trigger.dialogueLines?.Length ?? 0}]");

                // 3. 输出调试：预览第一行文本
                if (trigger.dialogueLines != null && trigger.dialogueLines.Length > 0)
                {
                    DebugLog($"📝 对话内容预览: \"{trigger.dialogueLines[0]}\" (共{trigger.dialogueLines.Length}行)");
                }

                // 4. 强制刷新 UI
                StartCoroutine(ForceRefreshUI(trigger.speechBubble));

                // 5. 显示左右 NPC 形象
                if (trigger.npcPanelLeft != null)
                    trigger.npcPanelLeft.SetActive(true);
                if (trigger.npcPanelRight != null)
                    trigger.npcPanelRight.SetActive(true);

                currentActiveTrigger = trigger;
            }
            else
            {
                Debug.LogError($"触发器 {trigger.uniqueId} 的气泡GameObject为空！");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"激活触发器 {trigger.uniqueId} 时出错: {e.Message}");
        }
    }

    void DeactivateCurrentTrigger()
    {
        if (currentActiveTrigger == null) return;

        try
        {
            if (currentActiveTrigger.speechBubble != null)
                currentActiveTrigger.speechBubble.SetActive(false);
            if (currentActiveTrigger.dialogueUI != null)
                currentActiveTrigger.dialogueUI.SetActive(false);

            if (currentActiveTrigger.npcPanelLeft != null)
                currentActiveTrigger.npcPanelLeft.SetActive(false);
            if (currentActiveTrigger.npcPanelRight != null)
                currentActiveTrigger.npcPanelRight.SetActive(false);

            DebugLog($"🔽 已关闭触发器: {currentActiveTrigger.uniqueId} ({currentActiveTrigger.triggerDate})");
        }
        catch (Exception e)
        {
            Debug.LogError($"关闭触发器时出错: {e.Message}");
        }
    }

    IEnumerator ForceRefreshUI(GameObject uiElement)
    {
        // 强制刷新UI显示的技巧
        yield return null; // 等待一帧

        if (uiElement != null)
        {
            uiElement.SetActive(false);
            yield return null;
            uiElement.SetActive(true);

            // 强制Canvas重绘
            Canvas parentCanvas = uiElement.GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                Canvas.ForceUpdateCanvases();
            }
        }
    }

    void OnBubbleClicked(DialogueTriggerInfo trigger, int triggerIndex)
    {
        DebugLog($"🔥 气泡点击事件开始");

        // ───【新增】播放“点击气泡”音效 ───
        if (uiAudioSource != null && bubbleClickClip != null)
        {
            uiAudioSource.PlayOneShot(bubbleClickClip);
        }

        DebugLog($"   传入的触发器ID: {trigger?.uniqueId ?? "NULL"}");
        DebugLog($"   传入的索引: {triggerIndex}");
        DebugLog($"   当前活跃触发器ID: {currentActiveTrigger?.uniqueId ?? "NULL"}");

        // 验证触发器一致性
        if (trigger != currentActiveTrigger)
        {
            Debug.LogWarning($"⚠️ 触发器不一致！点击的触发器({trigger?.uniqueId})与当前活跃触发器({currentActiveTrigger?.uniqueId})不匹配");
        }

        // 防止快速重复点击
        if (Time.time - lastTriggerTime < MIN_TRIGGER_INTERVAL)
        {
            DebugLog("点击过于频繁，忽略本次点击");
            return;
        }
        lastTriggerTime = Time.time;

        // 检查对话系统状态
        if (isDialogueSystemBusy)
        {
            DebugLog("对话系统忙碌中，忽略点击");
            return;
        }

        // 验证触发器有效性
        if (trigger == null)
        {
            Debug.LogError("❌ 触发器为空！");
            return;
        }

        StartCoroutine(HandleBubbleClickCoroutine(trigger, triggerIndex));
    }

    IEnumerator HandleBubbleClickCoroutine(DialogueTriggerInfo trigger, int triggerIndex)
    {
        isDialogueSystemBusy = true;

        DebugLog($"=== 开始处理对话点击 ===");
        DebugLog($"触发器ID: {trigger.uniqueId}");
        DebugLog($"触发器索引: {triggerIndex}");
        DebugLog($"触发日期: {trigger.triggerDate}");

        // 隐藏气泡
        if (trigger.speechBubble != null)
        {
            trigger.speechBubble.SetActive(false);
            DebugLog("气泡已隐藏");
        }

        yield return new WaitForEndOfFrame(); // 等待UI更新

        // 显示对话UI
        if (trigger.dialogueUI != null)
        {
            trigger.dialogueUI.SetActive(true);
            DebugLog("对话UI已显示");

            // 确保DialogueManager正确初始化
            yield return InitializeDialogueManager(trigger);
        }
        else
        {
            Debug.LogError($"触发器[{triggerIndex}] 的对话UI为空！");
        }

        isDialogueSystemBusy = false;
        DebugLog($"=== 对话点击处理完成 ===");
    }

    IEnumerator InitializeDialogueManager(DialogueTriggerInfo trigger)
    {
        if (trigger.DialogueManager == null)
        {
            Debug.LogError("DialogueManager为空！");
            yield break;
        }

        // 等待DialogueManager初始化
        yield return new WaitForEndOfFrame();

        bool success = false;

        try
        {
            DebugLog($"正在重置DialogueManager... (触发器: {trigger.uniqueId})");
            trigger.DialogueManager.ResetDialogue();
            success = true;
        }
        catch (Exception e)
        {
            Debug.LogError($"重置DialogueManager时出错: {e.Message}");
        }

        if (!success) yield break;

        yield return new WaitForEndOfFrame();

        try
        {
            // 详细验证对话内容
            if (trigger.dialogueLines != null && trigger.dialogueLines.Length > 0)
            {
                DebugLog($"📖 即将启动对话 (触发器: {trigger.uniqueId}):");
                for (int i = 0; i < trigger.dialogueLines.Length; i++)
                {
                    DebugLog($"   第{i + 1}行: \"{trigger.dialogueLines[i]}\"");
                }

                trigger.DialogueManager.StartDialogue(trigger.dialogueLines);
                DebugLog($"✅ 对话启动完成！(触发器: {trigger.uniqueId})");

                // 监听对话完成事件
                StartCoroutine(WaitForDialogueCompletion(trigger));
            }
            else
            {
                Debug.LogError($"❌ 触发器 {trigger.uniqueId} 的对话内容为空或未配置！");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"启动对话时出错: {e.Message}");
        }
    }

    // 新增：等待对话完成的协程
    IEnumerator WaitForDialogueCompletion(DialogueTriggerInfo trigger)
    {
        DebugLog($"🔄 开始等待对话完成: {trigger.uniqueId}");

        // 等待对话开始
        yield return new WaitForSeconds(0.5f);

        // 等待对话完成
        while (trigger.DialogueManager != null && trigger.DialogueManager.IsDialogueActive())
        {
            yield return new WaitForSeconds(0.1f);
        }

        // 额外等待确保对话UI已关闭
        yield return new WaitForSeconds(0.2f);

        // 对话完成，标记为已完成
        trigger.isCompleted = true;
        DebugLog($"🎉 对话完成: {trigger.uniqueId} ({trigger.triggerDate})");

        // 隐藏对话UI（确保已关闭）
        if (trigger.dialogueUI != null)
        {
            trigger.dialogueUI.SetActive(false);
        }

        // 通知TimeSystem恢复时间系统
        if (TimeSystem_7.Instance != null)
        {
            DebugLog($"🔔 准备通知TimeSystem恢复时间系统");

            // 先恢复对话暂停
            TimeSystem_7.Instance.ResumeTimeFromDialogue();

            // 再恢复对话月份暂停（如果有的话）
            if (TimeSystem_7.Instance.IsDialogueMonthPaused())
            {
                TimeSystem_7.Instance.ResumeTimeFromDialogueMonth();
            }

            // 验证恢复状态
            bool stillPaused = TimeSystem_7.Instance.IsTimeSystemPaused();
            DebugLog($"✅ 时间系统恢复通知已发送，当前暂停状态: {stillPaused}");

            if (stillPaused)
            {
                Debug.LogWarning("⚠️ 时间系统恢复失败，尝试强制恢复");
                // 可以在这里添加强制恢复逻辑
            }
        }
        else
        {
            Debug.LogError("❌ TimeSystem_7.Instance 为空，无法恢复时间系统");
        }

        // 清理当前活跃触发器
        if (currentActiveTrigger == trigger)
        {
            currentActiveTrigger = null;
            DebugLog($"🧹 已清理当前活跃触发器");
        }
    }

    void DebugLog(string message)
    {
        if (!enableDebugMode) return;

        string timestamp = Time.time.ToString("F2");
        string logMessage = $"[对话系统-{timestamp}s] {message}";

        if (logDetailedInfo)
        {
            Debug.Log(logMessage);
        }
    }

    // 调试工具
    [ContextMenu("显示当前状态")]
    void ShowCurrentStatus()
    {
        Debug.Log("=== 当前对话系统状态 ===");
        Debug.Log($"当前时间: {(TimeSystem_7.Instance?.CurrentDateString ?? "时间系统未初始化")}");
        Debug.Log($"当前活跃触发器: {(currentActiveTrigger?.uniqueId ?? "无")} ({currentActiveTrigger?.triggerDate ?? ""})");
        Debug.Log($"对话系统忙碌: {isDialogueSystemBusy}");
        Debug.Log($"配置的触发器数量: {dialogueTriggers?.Length ?? 0}");

        for (int i = 0; i < (dialogueTriggers?.Length ?? 0); i++)
        {
            var trigger = dialogueTriggers[i];
            Debug.Log($"  触发器[{i}]: ID[{trigger.uniqueId}], 日期[{trigger.triggerDate}], " +
                     $"已触发[{trigger.hasBeenTriggered}], 已完成[{trigger.isCompleted}], " +
                     $"气泡激活[{trigger.speechBubble?.activeInHierarchy ?? false}]");
        }
    }

    [ContextMenu("强制触发当前时间的对话")]
    void ForceCurrentDialogue()
    {
        if (TimeSystem_7.Instance != null)
        {
            string currentDate = TimeSystem_7.Instance.CurrentDateString;
            var trigger = FindTriggerByDate(currentDate);
            if (trigger != null && !trigger.isCompleted)
            {
                int index = triggerToIndex.ContainsKey(trigger) ? triggerToIndex[trigger] : -1;
                DebugLog($"🔧 强制触发对话: 触发器ID[{trigger.uniqueId}], 索引[{index}]");
                OnBubbleClicked(trigger, index);
            }
            else
            {
                Debug.Log($"当前时间 {currentDate} 没有匹配的未完成对话");
            }
        }
    }

    [ContextMenu("重置所有对话完成状态")]
    void ResetAllDialogueCompletionStatus()
    {
        for (int i = 0; i < dialogueTriggers.Length; i++)
        {
            dialogueTriggers[i].isCompleted = false;
            dialogueTriggers[i].hasBeenTriggered = false;
        }
        Debug.Log("🔄 已重置所有对话完成状态");
    }
    void DeactivateAllNpcPanels()
    {
        foreach (var trigger in dialogueTriggers)
        {
            if (trigger.npcPanelLeft != null)
                trigger.npcPanelLeft.SetActive(false);
            if (trigger.npcPanelRight != null)
                trigger.npcPanelRight.SetActive(false);
        }
    }

}