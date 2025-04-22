using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject gameOverPanel;  // 结算界面
    public Text scoreText;  // 显示得分的文本
    public float gameDuration = 50f;  // 游戏结算的时间间隔
    private float timeElapsed = 0f;  // 已经过的时间
    public Image calendarImage;  // 日历图标
    public Sprite[] calendarPages;  // 存储不同日期的图像
    private int currentPageIndex = 0;  // 当前页索引
    
    void Start()
    {
        // 初始化时隐藏结算界面
        gameOverPanel.SetActive(false);
        Time.timeScale = 1f;  // 确保游戏开始时是正常的速度
    }

    void Update()
    {
        // 累积已过去的时间
        timeElapsed += Time.deltaTime;

        // 每经过 `gameDuration` 秒，触发结算
        if (timeElapsed >= gameDuration)
        {
            TriggerGameOver();
            timeElapsed = 0f;  // 重置计时器
        }
    }

    // 触发结算逻辑
    void TriggerGameOver()
    {
        // 暂停游戏
        Time.timeScale = 0f;  // 设置时间缩放为0，暂停游戏

        // 显示结算面板
        gameOverPanel.SetActive(true);

        // 更新结算信息（例如得分）
        scoreText.text = "Score: " + GetCurrentScore();  // 假设你有一个方法来获取当前得分
    }

    // 继续游戏的方法（当玩家点击结算面板上的"继续"按钮时）
    public void ContinueGame()
    {
        // 隐藏结算面板
        gameOverPanel.SetActive(false);

        // 恢复游戏
        Time.timeScale = 1f;  // 恢复正常游戏速度

        // 调用翻页方法
        FlipCalendarPage();  // 翻到下一页
    }

    // 获取当前得分的假设方法（你可以替换成实际的得分计算方法）
    int GetCurrentScore()
    {
        return 100;  // 示例，返回一个固定分数
    }

    void FlipCalendarPage()
    {
        if (calendarPages == null || calendarPages.Length == 0)
        {
            Debug.LogError("calendarPages is empty or not assigned!");
            return;
        }

        // 翻到下一页
        currentPageIndex++;
        if (currentPageIndex >= calendarPages.Length)
        {
            currentPageIndex = 0;  // 回到第一页
        }

        // 更新日历图标
        calendarImage.sprite = calendarPages[currentPageIndex];
    }
}

