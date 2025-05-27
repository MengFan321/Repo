using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeSystem_7 : MonoBehaviour
{
    //仅用于测试（可删）
    public int debugTargetYear = 1986;
    public int debugTargetMonth = 5;
    public static TimeSystem_7 Instance { get; private set; }
    //后续可删

    public TextMeshProUGUI timeText; // 显示游戏时间的TextMeshProUGUI
    public GameObject shopUI; // 商城UI
    public GameObject settlementUI; // 结算UI
    public TextMeshProUGUI settlementText; // 结算信息的TextMeshProUGUI
    public ShopSystem_7 shopSystem; // 引用ShopSystem脚本

    // 新增UI组件
    public GameObject specialEventUI; // 特殊事件UI
    public TextMeshProUGUI specialEventText; // 特殊事件文字

    // 新增：对话系统引用
    [Header("对话系统")]
    public MultiTimeDialogueTrigger dialogueTrigger; // 对话触发器引用

    private float realTimeSeconds = 0f;
    private int gameTimeMonths = 0;
    private int startingYear = 1985;
    private int startingMonth = 1;

    // 新增：对话暂停相关变量
    private bool isDialoguePaused = false;
    private float pausedAtTime = 0f; // 记录暂停时的时间点
    private bool isDialogueMonthPaused = false; // 新增：标记是否因对话月份暂停
    private string currentDialogueMonth = ""; // 新增：当前对话月份

    // 新增：标记是否已经检查过当前月份
    private string lastCheckedMonth = "";
    private bool monthJustChanged = false;

    // 特殊日期和文字
    private Dictionary<string, string> specialEvents = new Dictionary<string, string>
    {
        { "1987-05", "中共十三大提出基本路线是以经济建设为中心" },
        { "1997-07", "香港回归" },
        { "2007-9", "武汉长江大桥通车五十周年纪念" },
        { "2008-05", "汶川大地震" },
        { "2010-01", "5G技术研发" }
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // 防止场景中有多个 TimeSystem_7 导致冲突
        }
    }

    void Start()
    {
        // 自动查找MultiTimeDialogueTrigger（如果没有手动分配）
        if (dialogueTrigger == null)
        {
            dialogueTrigger = FindObjectOfType<MultiTimeDialogueTrigger>();
            if (dialogueTrigger == null)
            {
                Debug.LogWarning("⚠️ 未找到MultiTimeDialogueTrigger组件");
            }
        }

        // 初始化时检查第一个月份
        CheckInitialMonth();
    }

    // 新增：初始检查第一个月份
    void CheckInitialMonth()
    {
        int currentYear = startingYear + (startingMonth + gameTimeMonths - 1) / 12;
        int currentMonth = (startingMonth + gameTimeMonths - 1) % 12 + 1;
        string currentDateString = $"{currentYear:D4}-{currentMonth:D2}";

        lastCheckedMonth = currentDateString;

        // 检查初始月份是否有对话
        if (dialogueTrigger != null && dialogueTrigger.IsDialogueMonth(currentDateString))
        {
            PauseTimeForDialogueMonth(currentDateString);
        }
    }

    void Update()
    {
        // 获取当前时间信息
        int currentYear = startingYear + (startingMonth + gameTimeMonths - 1) / 12;
        int currentMonth = (startingMonth + gameTimeMonths - 1) % 12 + 1;
        string currentDateString = $"{currentYear:D4}-{currentMonth:D2}";

        // 检查月份是否变化
        if (currentDateString != lastCheckedMonth)
        {
            monthJustChanged = true;
            lastCheckedMonth = currentDateString;
            Debug.Log($"📅 月份变化: {lastCheckedMonth} -> {currentDateString}");
        }

        // 如果对话暂停中（包括对话月份暂停和实际对话暂停），不更新时间
        if (!isDialoguePaused && !isDialogueMonthPaused)
        {
            realTimeSeconds += Time.deltaTime;
        }

        // 每50秒结算一次，但需要先检查接下来的月份是否有对话
        if (realTimeSeconds >= 50f)
        {
            // 检查接下来的两个月是否有对话
            bool hasDialogueInNextMonths = false;
            int targetGameMonths = gameTimeMonths;

            // 逐月检查
            for (int i = 1; i <= 2; i++)
            {
                targetGameMonths = gameTimeMonths + i;
                int checkYear = startingYear + (startingMonth + targetGameMonths - 1) / 12;
                int checkMonth = (startingMonth + targetGameMonths - 1) % 12 + 1;
                string checkDateString = $"{checkYear:D4}-{checkMonth:D2}";

                Debug.Log($"🔍 检查月份 {i}: {checkDateString}");

                if (dialogueTrigger != null && dialogueTrigger.IsDialogueMonth(checkDateString))
                {
                    Debug.Log($"🎯 发现对话月份: {checkDateString}");
                    hasDialogueInNextMonths = true;
                    // 只前进到有对话的月份
                    gameTimeMonths = targetGameMonths;
                    realTimeSeconds = 0f;
                    shopSystem.AutoCheckout();

                    // 立即暂停
                    PauseTimeForDialogueMonth(checkDateString);
                    break;
                }
            }

            // 如果接下来两个月都没有对话，正常前进2个月
            if (!hasDialogueInNextMonths)
            {
                gameTimeMonths += 2;
                realTimeSeconds = 0f;
                shopSystem.AutoCheckout();

                int nextYear = startingYear + (startingMonth + gameTimeMonths - 1) / 12;
                int nextMonth = (startingMonth + gameTimeMonths - 1) % 12 + 1;
                Debug.Log($"⏰ 正常前进到: {nextYear:D4}-{nextMonth:D2}");
            }
        }

        // 显示商城UI和结算UI
        if (realTimeSeconds < 20f)
        {
            shopUI.SetActive(true);
            settlementUI.SetActive(false);
            specialEventUI.SetActive(false); // 隐藏特殊事件UI

            // 如果刚进入新月份且在商城阶段，检查是否需要显示对话气泡
            if (monthJustChanged && realTimeSeconds < 1f)
            {
                monthJustChanged = false;
                if (isDialogueMonthPaused && dialogueTrigger != null)
                {
                    // 延迟一帧激活气泡，确保UI更新
                    StartCoroutine(DelayedActivateBubble());
                }
            }
        }
        else if (realTimeSeconds >= 40f && realTimeSeconds < 50f)
        {
            shopUI.SetActive(false);
            settlementUI.SetActive(true);
            ShowSettlement();
            specialEventUI.SetActive(false); // 隐藏特殊事件UI
        }
        else
        {
            shopUI.SetActive(false);
            settlementUI.SetActive(false);
        }

        // 如果对话暂停中，时间显示变为红色提示
        if (isDialoguePaused || isDialogueMonthPaused)
        {
            timeText.color = Color.red;
        }
        else
        {
            timeText.color = Color.black;
        }

        timeText.text = $"{currentYear}-{currentMonth:D2}";

        // 检查是否达到特殊日期
        if (specialEvents.ContainsKey(currentDateString))
        {
            specialEventUI.SetActive(true); // 显示特殊事件UI
            specialEventText.color = Color.red; // 设置特殊事件文字颜色
            specialEventText.text = specialEvents[currentDateString]; // 显示特殊文字
        }
        else
        {
            specialEventUI.SetActive(false); // 隐藏特殊事件UI
        }

        //  按 T 键跳转到 Inspector 中设置的目标年月（仅用于测试，可删）
        if (Input.GetKeyDown(KeyCode.T))
        {
            gameTimeMonths = (debugTargetYear - startingYear) * 12 + (debugTargetMonth - startingMonth);
            realTimeSeconds = 0f; // 重置秒数
            Debug.Log($"⏩ 快速跳转到时间: {debugTargetYear}-{debugTargetMonth:D2}");

            // 检查跳转后的月份
            string jumpDateString = $"{debugTargetYear:D4}-{debugTargetMonth:D2}";
            if (dialogueTrigger != null && dialogueTrigger.IsDialogueMonth(jumpDateString))
            {
                PauseTimeForDialogueMonth(jumpDateString);
            }
        }
    }

    // 新增：延迟激活气泡的协程
    private IEnumerator DelayedActivateBubble()
    {
        yield return null; // 等待一帧

        if (isDialogueMonthPaused && !string.IsNullOrEmpty(currentDialogueMonth))
        {
            dialogueTrigger.OnTimeSystemPausedForDialogue(currentDialogueMonth);
        }
    }

    // 新增：因对话月份暂停时间系统
    private void PauseTimeForDialogueMonth(string dialogueMonth)
    {
        if (!isDialogueMonthPaused)
        {
            isDialogueMonthPaused = true;
            currentDialogueMonth = dialogueMonth;
            // 不再重置时间，保持当前时间
            pausedAtTime = realTimeSeconds;
            Debug.Log($"⏸️ 进入对话月份，时间系统已暂停: {dialogueMonth}，当前秒数: {realTimeSeconds:F2}");

            // 如果刚好在0秒附近，立即通知对话触发器
            if (realTimeSeconds < 1f)
            {
                StartCoroutine(DelayedActivateBubble());
            }
        }
    }

    // 新增：对话月份完成后恢复时间系统
    public void ResumeTimeFromDialogueMonth()
    {
        Debug.Log($"📞 ResumeTimeFromDialogueMonth 被调用");
        Debug.Log($"   当前对话月份暂停状态: {isDialogueMonthPaused}");
        Debug.Log($"   当前对话月份: {currentDialogueMonth}");

        if (isDialogueMonthPaused)
        {
            isDialogueMonthPaused = false;
            currentDialogueMonth = "";
            Debug.Log($"▶️ 对话月份完成，时间系统已恢复");

            // 验证恢复后的状态
            Debug.Log($"✅ 恢复后状态检查:");
            Debug.Log($"   对话暂停: {isDialoguePaused}");
            Debug.Log($"   对话月份暂停: {isDialogueMonthPaused}");
            Debug.Log($"   时间系统总体暂停: {IsTimeSystemPaused()}");
        }
    }

    // 新增：暂停时间系统（对话开始时调用）
    public void PauseTimeForDialogue()
    {
        if (!isDialoguePaused)
        {
            isDialoguePaused = true;
            pausedAtTime = realTimeSeconds;
            Debug.Log($"⏸️ 对话开始，时间系统已暂停，暂停时间点: {pausedAtTime:F2}秒");
        }
    }

    // 新增：恢复时间系统（对话结束时调用）
    public void ResumeTimeFromDialogue()
    {
        Debug.Log($"📞 ResumeTimeFromDialogue 被调用");
        Debug.Log($"   当前对话暂停状态: {isDialoguePaused}");
        Debug.Log($"   当前对话月份暂停状态: {isDialogueMonthPaused}");

        if (isDialoguePaused)
        {
            isDialoguePaused = false;
            Debug.Log($"▶️ 对话结束，时间系统已恢复，从 {pausedAtTime:F2}秒 继续");
        }

        // 不要在这里自动恢复对话月份暂停，让调用者显式控制
    }

    // 新增：检查是否在暂停状态（包括对话暂停和对话月份暂停）
    public bool IsDialoguePaused()
    {
        return isDialoguePaused;
    }

    // 新增：检查是否因对话月份暂停
    public bool IsDialogueMonthPaused()
    {
        return isDialogueMonthPaused;
    }

    // 新增：获取当前周期内的时间进度（用于调试）
    public float GetCurrentCycleProgress()
    {
        return realTimeSeconds / 50f;
    }

 
    private void ShowSettlement()
    {
        int totalSpent = shopSystem.TotalSpent;
        int totalSellPrice = shopSystem.CalculateTotalSellPrice();

        settlementText.text = $"结算：\n成本: {totalSpent}\n出售: {totalSellPrice}";
    }

    // 新增：用于npc对话
    public int CurrentYear
    {
        get
        {
            return startingYear + (startingMonth + gameTimeMonths - 1) / 12;
        }
    }

    // 用于npc对话
    public int CurrentMonth
    {
        get
        {
            return (startingMonth + gameTimeMonths - 1) % 12 + 1;
        }
    }

    // 当前年月字符串，如 "1997-07"
    public string CurrentDateString
    {
        get
        {
            return $"{CurrentYear:D4}-{CurrentMonth:D2}";
        }
    }

    // 新增：用于npc对话
    public bool IsTimeSystemPaused()
    {
        return isDialoguePaused || isDialogueMonthPaused; // 任一暂停状态都算暂停
    }

    // 新增：调试方法（用于npc对话)
    [ContextMenu("显示时间系统状态")]
    void ShowTimeSystemStatus()
    {
        Debug.Log("=== 时间系统状态 ===");
        Debug.Log($"当前时间: {CurrentDateString}");
        Debug.Log($"当前秒数: {realTimeSeconds:F2}");
        Debug.Log($"对话暂停: {isDialoguePaused}");
        Debug.Log($"对话月份暂停: {isDialogueMonthPaused}");
        Debug.Log($"当前对话月份: {currentDialogueMonth}");
        Debug.Log($"时间系统暂停: {IsTimeSystemPaused()}");
        Debug.Log($"对话触发器: {(dialogueTrigger != null ? "已连接" : "未连接")}");
    }

    [ContextMenu("强制恢复时间系统")]
    void ForceResumeTimeSystem()
    {
        if (isDialoguePaused || isDialogueMonthPaused)
        {
            isDialoguePaused = false;
            isDialogueMonthPaused = false;
            currentDialogueMonth = "";
            Debug.Log("🔧 已强制恢复时间系统");
        }
        else
        {
            Debug.Log("时间系统未暂停，无需恢复");
        }
    }
}