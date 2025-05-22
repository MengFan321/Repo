using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TimeSystem_7 : MonoBehaviour
{
    public TextMeshProUGUI timeText; // 显示游戏时间的TextMeshProUGUI
    public GameObject shopUI; // 商城UI
    public GameObject settlementUI; // 结算UI
    public TextMeshProUGUI settlementText; // 结算信息的TextMeshProUGUI
    public ShopSystem_7 shopSystem; // 引用ShopSystem脚本

    // 新增UI组件
    public GameObject specialEventUI; // 特殊事件UI
    public TextMeshProUGUI specialEventText; // 特殊事件文字

    private float realTimeSeconds = 0f;
    private int gameTimeMonths = 0;
    private int startingYear = 1985;
    private int startingMonth = 1;

    // 特殊日期和文字
    private Dictionary<string, string> specialEvents = new Dictionary<string, string>
    {
        { "1987-05", "中共十三大提出基本路线是以经济建设为中心" },
        { "1997-07", "香港回归" },
        { "2007-9", "武汉长江大桥通车五十周年纪念" },
        { "2008-05", "汶川大地震" },
        { "2010-01", "5G技术研发" }
    };

    void Update()
    {
        realTimeSeconds += Time.deltaTime;

        // 每50秒结算一次
        if (realTimeSeconds >= 50f)
        {
            realTimeSeconds -= 50f; // 重置时间
            gameTimeMonths += 2;
            shopSystem.AutoCheckout(); // 自动结算购物车
        }

        // 显示商城UI和结算UI
        if (realTimeSeconds < 20f)
        {
            shopUI.SetActive(true);
            settlementUI.SetActive(false);
            specialEventUI.SetActive(false); // 隐藏特殊事件UI
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

        // 更新时间显示
        int currentYear = startingYear + (startingMonth + gameTimeMonths - 1) / 12;
        int currentMonth = (startingMonth + gameTimeMonths - 1) % 12 + 1;
        timeText.color = Color.black;
        timeText.text = $"{currentYear}-{currentMonth:D2}";

        // 检查是否达到特殊日期
        string currentDateString = $"{currentYear:D4}-{currentMonth:D2}";
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
    }

    private void ShowSettlement()
    {
        int totalSpent = shopSystem.TotalSpent;
        int totalSellPrice = shopSystem.CalculateTotalSellPrice();

        
        settlementText.text = $"结算：\n成本: {totalSpent}\n出售: {totalSellPrice}";
    }
   
   
}
