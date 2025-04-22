using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeSystem : MonoBehaviour
{
    public TextMeshProUGUI timeText; // 显示游戏时间的Text组件
    public GameObject shopUI; // 商城UI

    private float realTimeSeconds = 0f;
    private int gameTimeMonths = 0;
    private int startingYear = 1985;
    private int startingMonth = 1;
    private bool isShopOpen = false; // 商城是否开放

    void Update()
    {
        // 计算现实时间的流逝
        realTimeSeconds += Time.deltaTime;

        // 每过50秒现实时间，游戏时间增加一个月
        if (realTimeSeconds >= 50f)
        {
            realTimeSeconds -= 50f;
            gameTimeMonths++;
            isShopOpen = false; // 关闭商城
        }

        // 每个月的前20秒开放商城
        if (realTimeSeconds < 20f)
        {
            isShopOpen = true;
            shopUI.SetActive(true); // 显示商城UI
        }
        else
        {
            isShopOpen = false;
            shopUI.SetActive(false); // 隐藏商城UI
        }

        // 计算当前的年份和月份
        int currentYear = startingYear + (startingMonth + gameTimeMonths - 1) / 12;
        int currentMonth = (startingMonth + gameTimeMonths - 1) % 12 + 1;

        // 显示游戏时间
        timeText.text = currentYear + "-" + currentMonth.ToString("D2");
    }

    // 检查是否可以操作
    public bool CanOperate()
    {
        if (isShopOpen)
        {
            return true;
        }
        else
        {
            Debug.Log("不可以操作！");
            return false;
        }
    }
}


