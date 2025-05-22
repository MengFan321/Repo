using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerController : MonoBehaviour
{
    public TextMeshProUGUI timerText; // ������ʾ����ʱ��Text���
    private float totalTime; // ��ǰ����ʱ����ʱ��
    private bool isTimerRunning = false; // ��Ǽ�ʱ���Ƿ���������

    void Start()
    {
        // ��������ʱ
        StartTimer();
    }

    // ��������ʱ
    public void StartTimer()
    {
        if (!isTimerRunning)
        {
            StartCoroutine(TimerLoop());
        }
    }

    // ֹͣ����ʱ
    public void StopTimer()
    {
        if (isTimerRunning)
        {
            StopAllCoroutines();
            isTimerRunning = false;
        }
    }

    // ѭ������ʱ��Coroutine
    private IEnumerator TimerLoop()
    {
        isTimerRunning = true;

        while (true)
        {
            // ��һ��20�뵹��ʱ
            totalTime = 20f;
            while (totalTime > 0f)
            {
                timerText.color = Color.black;
                timerText.text = totalTime.ToString("F0"); // ��ʾ����ʱ
                totalTime -= Time.deltaTime;
                yield return null;
            }

            // �ڶ���20�뵹��ʱ
            totalTime = 20f;
            while (totalTime > 0f)
            {
                timerText.text = totalTime.ToString("F0"); // ��ʾ����ʱ
                totalTime -= Time.deltaTime;
                yield return null;
            }

            // 10�뵹��ʱ
            totalTime = 10f;
            while (totalTime > 0f)
            {
                timerText.text = totalTime.ToString("F0"); // ��ʾ����ʱ
                totalTime -= Time.deltaTime;
                yield return null;
            }
        }
    }
}
