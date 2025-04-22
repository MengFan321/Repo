using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 用于处理UI组件

public class CalendarManager : MonoBehaviour
{
    public Image calendarImage;  // 日历图标
    public Sprite[] calendarPages;  // 存储不同日期的图像
    public float pageFlipInterval = 50f;  // 翻页的时间间隔（秒）

    private int currentPageIndex = 0;  // 当前页索引
    private float timePassed = 0f;  // 已经过的时间

    void Update()
    {
        // 每帧增加已过的时间
        timePassed += Time.deltaTime;

        // 如果已过时间超过翻页间隔，切换到下一页
        if (timePassed >= pageFlipInterval)
        {
            FlipCalendarPage();
            timePassed = 0f;  // 重置计时器
        }
    }

    void FlipCalendarPage()
    {
        currentPageIndex++;

        // 如果已经到最后一页，回到第一页
        if (currentPageIndex >= calendarPages.Length)
        {
            currentPageIndex = 0;
        }

        // 切换日历图标
        calendarImage.sprite = calendarPages[currentPageIndex];

        // 可以在这里添加翻页动画效果
    }
}
