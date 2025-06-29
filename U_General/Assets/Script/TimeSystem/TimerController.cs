using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour
{
    public TextMeshProUGUI timerText; // 用于显示倒计时的Text组件
    private float totalTime; // 当前阶段的剩余时间
    private bool isTimerRunning = false; // 是否正在计时
    private bool isPaused = false; // 是否暂停计时（例如对话期间）
    private bool isDialoguePaused = false; // 专门用于对话暂停的标记

    [Header("计时器设置")]
    public float stage1Duration = 20f; // 第一阶段时长
    public float stage2Duration = 10f; // 第二阶段时长  
    public float stage3Duration = 10f; // 第三阶段时长

    [Header("颜色设置")]
    public Color stage1Color = Color.black;
    public Color stage2Color = Color.white;
    public Color stage3Color = Color.red; // 添加第三阶段颜色

    void Start()
    {
        // 启动计时器
        StartTimer();
    }

    // 启动计时器
    public void StartTimer()
    {
        if (!isTimerRunning)
        {
            StartCoroutine(TimerLoop());
        }
    }

    // 停止计时器
    public void StopTimer()
    {
        if (isTimerRunning)
        {
            StopAllCoroutines();
            isTimerRunning = false;
        }
    }

    // 计时器循环协程
    private IEnumerator TimerLoop()
    {
        isTimerRunning = true;

        while (true)
        {
            // 第一阶段：倒计时（黑色）
            yield return StartCoroutine(CountdownStage(stage1Duration, stage1Color));

            // 第二阶段：倒计时（白色）
            yield return StartCoroutine(CountdownStage(stage2Duration, stage2Color));

            // 第三阶段：倒计时（红色或其他颜色）
            yield return StartCoroutine(CountdownStage(stage3Duration, stage3Color));
        }
    }

    // 单个阶段的倒计时协程
    private IEnumerator CountdownStage(float duration, Color textColor)
    {
        totalTime = duration;
        timerText.color = textColor;

        while (totalTime > 0f)
        {
            // 检查是否因为对话而暂停
            while (isPaused || isDialoguePaused)
            {
                yield return null; // 暂停时等待
            }

            // 检查TimeSystem是否暂停（包括对话月份暂停和对话暂停）
            if (TimeSystem_7.Instance != null && TimeSystem_7.Instance.IsTimeSystemPaused())
            {
                yield return null;
                continue;
            }

            // 显示倒计时
            timerText.text = Mathf.Ceil(totalTime).ToString("F0");

            // 减少时间
            totalTime -= Time.deltaTime;

            yield return null;
        }

        // 确保显示0
        timerText.text = "0";
    }

    // 暂停计时器
    public void PauseTimer()
    {
        isPaused = true;
        Debug.Log("计时器已暂停");
    }

    // 恢复计时器
    public void ResumeTimer()
    {
        isPaused = false;
        Debug.Log("计时器已恢复");
    }

    // 检查是否暂停
    public bool IsPaused()
    {
        return isPaused;
    }

    // 检查是否运行中
    public bool IsRunning()
    {
        return isTimerRunning;
    }

    // 获取当前剩余时间
    public float GetCurrentTime()
    {
        return totalTime;
    }

    // 重置计时器
    public void ResetTimer()
    {
        StopTimer();
        StartTimer();
    }

    // 专门用于对话的暂停方法
    public void PauseForDialogue()
    {
        isDialoguePaused = true;
        Debug.Log("计时器因对话暂停");
    }

    // 专门用于对话的恢复方法
    public void ResumeFromDialogue()
    {
        isDialoguePaused = false;
        Debug.Log("计时器从对话中恢复");
    }

    // 检查是否因对话暂停
    public bool IsDialoguePaused()
    {
        return isDialoguePaused;
    }

    // 新增：检查计时器是否因为任何原因暂停（包括对话暂停、手动暂停、时间系统暂停）
    public bool IsAnyPaused()
    {
        bool timeSystemPaused = TimeSystem_7.Instance != null && TimeSystem_7.Instance.IsTimeSystemPaused();
        return isPaused || isDialoguePaused || timeSystemPaused;
    }

    // 新增：调试方法 - 显示当前状态
    [ContextMenu("显示计时器状态")]
    void ShowTimerStatus()
    {
        Debug.Log("=== 计时器状态 ===");
        Debug.Log($"计时器运行中: {isTimerRunning}");
        Debug.Log($"手动暂停: {isPaused}");
        Debug.Log($"对话暂停: {isDialoguePaused}");
        Debug.Log($"时间系统暂停: {(TimeSystem_7.Instance?.IsTimeSystemPaused() ?? false)}");
        Debug.Log($"任何暂停: {IsAnyPaused()}");
        Debug.Log($"当前剩余时间: {totalTime:F2}秒");
    }
}