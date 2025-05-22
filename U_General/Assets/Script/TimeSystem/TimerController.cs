using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour
{
    public TextMeshProUGUI timerText; // 用于显示倒计时的Text组件
    private float totalTime; // 当前倒计时的总时间
    private bool isTimerRunning = false; // 标记计时器是否正在运行

    void Start()
    {
        // 启动倒计时
        StartTimer();
    }

    // 启动倒计时
    public void StartTimer()
    {
        if (!isTimerRunning)
        {
            StartCoroutine(TimerLoop());
        }
    }

    // 停止倒计时
    public void StopTimer()
    {
        if (isTimerRunning)
        {
            StopAllCoroutines();
            isTimerRunning = false;
        }
    }

    // 循环倒计时的Coroutine
    private IEnumerator TimerLoop()
    {
        isTimerRunning = true;

        while (true)
        {
            // 第一个20秒倒计时
            totalTime = 20f;
            while (totalTime > 0f)
            {
                timerText.color = Color.black;
                timerText.text = totalTime.ToString("F0"); // 显示倒计时
                totalTime -= Time.deltaTime;
                yield return null;
            }

            // 第二个20秒倒计时
            totalTime = 20f;
            while (totalTime > 0f)
            {
                timerText.text = totalTime.ToString("F0"); // 显示倒计时
                totalTime -= Time.deltaTime;
                yield return null;
            }

            // 10秒倒计时
            totalTime = 10f;
            while (totalTime > 0f)
            {
                timerText.text = totalTime.ToString("F0"); // 显示倒计时
                totalTime -= Time.deltaTime;
                yield return null;
            }
        }
    }
}
