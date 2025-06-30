using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;  

public class TimeSystem_7 : MonoBehaviour
{
    //仅用于测试（可删）
    //public int debugTargetYear = 1986;
    //public int debugTargetMonth = 5;
    public static TimeSystem_7 Instance { get; private set; }

    public TextMeshProUGUI timeText; // 显示游戏时间的TextMeshProUGUI
    public GameObject shopUI; // 商城UI
    public GameObject settlementUI; // 结算UI
    public TextMeshProUGUI settlementText; // 结算信息的TextMeshProUGUI
    public ShopSystem_7 shopSystem; // 引用ShopSystem脚本

    // UI组件
    public GameObject specialEventUI; // 特殊事件UI
    public TextMeshProUGUI specialEventText; // 特殊事件文字

    // 新增游戏结束UI
    public GameObject gameOverUI; // 游戏结束UI
    public TextMeshProUGUI gameOverText; // 游戏结束文字

    // 特殊日期和文字
    private Dictionary<string, string> specialEvents = new Dictionary<string, string>
    {
        { "1987-07", "<size=10>1987-07</size>\n<size=60>特大消息</size>\n中央决定大力扶持发展非公有制经济\n个体小贩不再是资本主义尾巴！" },
        { "1997-07", "<size=10>1997-7</size>\n<size=60>香港回归！</size>\n今日零时香港正式脱离英国政府回归祖国怀抱！我国国旗将于香港上空飘荡！" },
        { "2007-11", "<size=10>2007-11</size>\n<size=25>武汉长江大桥建成50周年</size>\n\n祝贺武汉长江大桥建成50周年！\n纪念50周年的天堑变通途!" },
        { "2008-05", "<size=10>2008-05</size>\n<size=60>汶川大地震</size>\n今日汶川发生8.0级特大地震\n国家启动一级响应\n举国同心抗震救灾！" },
        { "2010-01", "<size=10>2010-01</size>\n<size=30>5G技术研发成功</size>\n\n我国5G技术成功流入市场！华为、中兴等企业推进开启了万物互联新时代！" }
    };
    public List<string> newsEvents = new List<string>(); // 存储已经发生的新闻事件
    public int currentPage = 0; // 当前显示的新闻页码
    public int totalPages => newsEvents.Count; // 总页数

    public Image specialEventImage; // 特殊事件图片容器

    // 新增：特殊事件图片资源字典
    private Dictionary<string, Sprite> specialEventImages = new Dictionary<string, Sprite>();


    // 新增：对话系统引用
    [Header("对话系统")]
    public MultiTimeDialogueTrigger dialogueTrigger; // 对话触发器引用

    private float realTimeSeconds = 0f;
    private int gameTimeMonths = 0;
    private int startingYear = 1985;
    private int startingMonth = 1;


    private bool isCartExceedWorkerCapacity = false;// 新增标志变量

    // 新增：对话暂停相关变量
    private bool isDialoguePaused = false;
    private float pausedAtTime = 0f; // 记录暂停时的时间点
    private bool isDialogueMonthPaused = false; // 新增：标记是否因对话月份暂停
    private string currentDialogueMonth = ""; // 新增：当前对话月份

    // 新增：标记是否已经检查过当前月份
    private string lastCheckedMonth = "";
    private bool monthJustChanged = false;

    /// <summary>当前游戏年份（从 startingYear 算起）</summary>
    public int CurrentGameYear => startingYear + gameTimeMonths / 12;
    /// <summary>当前游戏月份（1–12）</summary>
    public int CurrentGameMonth => gameTimeMonths % 12 + 1;


    

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
        // 初始化特殊事件图片资源字典
        InitializeSpecialEventImages();
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
        if (realTimeSeconds >= 40f)
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
                    /***************************************/
                    // 根据当前时间选择商品
                    int nowYear = startingYear + (startingMonth + gameTimeMonths - 1) / 12;
                    int nowMonth = (startingMonth + gameTimeMonths - 1) % 12 + 1;
                    shopSystem.SelectItemsByTime(nowYear, nowMonth);
                    isCartExceedWorkerCapacity = false; // 重置标志
                    // 检查玩家资金是否不足70
                    CheckGameOver(nowYear, nowMonth);
                    /***************************************/
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
                /***************************************/
                // 根据当前时间选择商品
                int nowYear = startingYear + (startingMonth + gameTimeMonths - 1) / 12;
                int nowMonth = (startingMonth + gameTimeMonths - 1) % 12 + 1;
                shopSystem.SelectItemsByTime(nowYear, nowMonth);
                isCartExceedWorkerCapacity = false; // 重置标志
                // 检查玩家资金是否不足70
                CheckGameOver(nowYear, nowMonth);
                /***************************************/
                int nextYear = startingYear + (startingMonth + gameTimeMonths - 1) / 12;
                int nextMonth = (startingMonth + gameTimeMonths - 1) % 12 + 1;
                Debug.Log($"⏰ 正常前进到: {nextYear:D4}-{nextMonth:D2}");
            }
            // 通知时间变化
            NotifyTimeChanged();
        }

        // 显示商城UI和结算UI
        if (realTimeSeconds < 20f)
        {
            shopUI.SetActive(true);
            settlementUI.SetActive(false);
            //specialEventUI.SetActive(false); // 隐藏特殊事件UI

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
        else if (realTimeSeconds >= 30f && realTimeSeconds < 40f)
        {
            shopUI.SetActive(false);
            settlementUI.SetActive(true);
            ShowSettlement();
            //specialEventUI.SetActive(false); // 隐藏特殊事件UI
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

        timeText.text = $"{currentYear}\n{currentMonth:D2}";

        // 检查是否达到特殊日期
        if (specialEvents.ContainsKey(currentDateString))
        {
            specialEventUI.SetActive(true); // 显示特殊事件UI

            // 将新事件添加到列表中
            if (!newsEvents.Contains(specialEvents[currentDateString]))
            {
                newsEvents.Insert(0, specialEvents[currentDateString]); // 新事件插入到列表最前面
            }

            // 显示当前页的新闻事件
            specialEventText.text = newsEvents[currentPage];

            // 新增：显示对应图片
            if (specialEventImages.ContainsKey(currentDateString))
            {
                Sprite sprite = specialEventImages[currentDateString];
                if (sprite != null)
                {
                    specialEventImage.sprite = sprite;
                    specialEventImage.gameObject.SetActive(true); // 确保图片容器是激活的
                    Debug.Log($"图片 {currentDateString} 已正确显示");
                }
                else
                {
                    Debug.LogError($"图片 {currentDateString} 未正确加载");
                }
            }
            else
            {
                specialEventImage.gameObject.SetActive(false); // 如果没有图片，隐藏图片容器
            }
        }
        else
        {
            //specialEventUI.SetActive(false); // 隐藏特殊事件UI
            specialEventImage.gameObject.SetActive(false); // 隐藏图片容器
        }

        // 检查是否需要更新图片
        if (currentPage < newsEvents.Count)
        {
            string currentEventDate = GetDateFromNewsEvent(newsEvents[currentPage]);
            if (currentEventDate != null && specialEventImages.ContainsKey(currentEventDate))
            {
                Sprite sprite = specialEventImages[currentEventDate];
                if (sprite != null)
                {
                    specialEventImage.sprite = sprite;
                    specialEventImage.gameObject.SetActive(true); // 确保图片容器是激活的
                }
                else
                {
                    specialEventImage.gameObject.SetActive(false); // 如果没有图片，隐藏图片容器
                }
            }
            else
            {
                specialEventImage.gameObject.SetActive(false); // 如果没有图片，隐藏图片容器
            }
        }

        ////  按 T 键跳转到 Inspector 中设置的目标年月（仅用于测试，可删）
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    gameTimeMonths = (debugTargetYear - startingYear) * 12 + (debugTargetMonth - startingMonth);
        //    realTimeSeconds = 0f; // 重置秒数
        //    Debug.Log($"⏩ 快速跳转到时间: {debugTargetYear}-{debugTargetMonth:D2}");

        //    // 检查跳转后的月份
        //    string jumpDateString = $"{debugTargetYear:D4}-{debugTargetMonth:D2}";
        //    if (dialogueTrigger != null && dialogueTrigger.IsDialogueMonth(jumpDateString))
        //    {
        //        PauseTimeForDialogueMonth(jumpDateString);
        //    }
        //    // 通知时间变化
        //    NotifyTimeChanged();
        //}
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
        return realTimeSeconds / 40f;
    }


    private void ShowSettlement()
    {
        int totalSpent = shopSystem.TotalSpent;
        int totalSellPrice = shopSystem.CalculateTotalSellPrice();

        settlementText.text = $"结算：\n成本: {totalSpent}销售额: {totalSellPrice}";

        // 检查是否需要显示提醒
        if (isCartExceedWorkerCapacity)
        {
            // 添加提醒文本
            //settlementText.text += "\n\nWarning: You have purchased more items than your workers can process.";

            // 显示未售出的商品信息
            if (shopSystem.excessItems.Count > 0)
            {
                settlementText.text += "\n未售出商品：\n";
                foreach (var item in shopSystem.excessItems)
                {
                    settlementText.text += $"|{item.name} (成本: {item.cost}, 售价: {item.sellPrice})";
                }
            }

        }

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

    public void SetCartExceedWorkerCapacity(bool value)
    {
        isCartExceedWorkerCapacity = value;
    }
    private void CheckGameOver(int currentYear, int currentMonth)
    {
        if (shopSystem.playerMoney < 70)
        {
            gameOverUI.SetActive(true); // 显示游戏结束UI
            string gameOverMessage = "达成破产结局！\n";

            // 检查是否在前5个月内达成破产结局
            if (gameTimeMonths <= 10) // 前5个月（每2个月结算一次，5 * 2 = 10）
            {
                gameOverMessage += "恭喜你获得破产大王称号！\n很遗憾的通知您，由于您的本金已不足以开启下一关，您破产了。令我们没想到的是，您在五个月内实现了破产，嗯，经过我们的深思熟虑，我们决定授予您“破产大王”称号。";
            }

            gameOverText.text = gameOverMessage;

            // 停止游戏逻辑
            Time.timeScale = 0;
        }
    }

    public void NextPage()
    {
        if (currentPage < totalPages - 1)
        {
            currentPage++;
            if (specialEventUI.activeSelf)
            {
                specialEventText.text = newsEvents[currentPage];
            }
        }
        UpdateSpecialEventImage(); // 更新图片
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            if (specialEventUI.activeSelf)
            {
                specialEventText.text = newsEvents[currentPage];
            }
        }
        UpdateSpecialEventImage(); // 更新图片
    }

    public delegate void TimeChangedHandler();
    public event TimeChangedHandler onTimeChanged;

    private void NotifyTimeChanged()
    {
        onTimeChanged?.Invoke();
    }

    // 新增：初始化特殊事件图片资源字典
    private void InitializeSpecialEventImages()
    {
        specialEventImages.Add("1987-07", Resources.Load<Sprite>("SpecialEvents/1"));
        specialEventImages.Add("1997-07", Resources.Load<Sprite>("SpecialEvents/2"));
        specialEventImages.Add("2007-11", Resources.Load<Sprite>("SpecialEvents/3"));
        specialEventImages.Add("2008-05", Resources.Load<Sprite>("SpecialEvents/4"));
        specialEventImages.Add("2010-01", Resources.Load<Sprite>("SpecialEvents/5"));
    }
    private void UpdateSpecialEventImage()
    {
        // 获取当前新闻事件的日期
        string currentEventDate = GetDateFromNewsEvent(newsEvents[currentPage]);
        Debug.Log($"当前新闻事件日期: {currentEventDate}");

        // 检查是否有对应的图片
        if (specialEventImages.ContainsKey(currentEventDate))
        {
            Sprite sprite = specialEventImages[currentEventDate];
            if (sprite != null)
            {

                specialEventImage.gameObject.SetActive(true); // 确保图片容器是激活的
                Debug.Log($"加载并显示图片: {currentEventDate}");
                specialEventImage.sprite = sprite;
                // 强制刷新 UI
                Canvas.ForceUpdateCanvases();
            }
            else
            {
                Debug.LogError($"图片 {currentEventDate} 未正确加载");
                specialEventImage.gameObject.SetActive(false); // 如果没有图片，隐藏图片容器
            }
        }
        else
        {
            Debug.Log($"没有找到对应的图片: {currentEventDate}");
            specialEventImage.gameObject.SetActive(false); // 如果没有图片，隐藏图片容器
        }
    }
    private string GetDateFromNewsEvent(string newsEventText)
    {
        // 新的新闻事件文本格式是 "<size=10>1985-01</size>\n<size=60>特大消息</size>\n\n..."
        // 使用字符串操作来提取日期
        int startIndex = newsEventText.IndexOf("<size=10>") + "<size=10>".Length;
        int endIndex = newsEventText.IndexOf("</size>", startIndex);
        if (startIndex != -1 && endIndex != -1)
        {
            string date = newsEventText.Substring(startIndex, endIndex - startIndex).Trim();
            Debug.Log($"从新闻事件文本中提取的日期: {date}");
            return date;
        }
        else
        {
            Debug.LogError("新闻事件文本格式不匹配，无法提取日期");
            return null; // 如果格式不匹配，返回 null
        }
    }
}
