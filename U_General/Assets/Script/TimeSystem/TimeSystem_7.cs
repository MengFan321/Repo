using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class TimeSystem_7 : MonoBehaviour
{
    public TextMeshProUGUI timeText; // ��ʾ��Ϸʱ���TextMeshProUGUI
    public GameObject shopUI; // �̳�UI
    public GameObject settlementUI; // ����UI
    public TextMeshProUGUI settlementText; // ������Ϣ��TextMeshProUGUI
    public ShopSystem_7 shopSystem; // ����ShopSystem�ű�

    // ����UI���
    public GameObject specialEventUI; // �����¼�UI
    public TextMeshProUGUI specialEventText; // �����¼�����

    private float realTimeSeconds = 0f;
    private int gameTimeMonths = 0;
    private int startingYear = 1985;
    private int startingMonth = 1;

    // �������ں�����
    private Dictionary<string, string> specialEvents = new Dictionary<string, string>
    {
        { "1987-05", "�й�ʮ�����������·�����Ծ��ý���Ϊ����" },
        { "1997-07", "��ۻع�" },
        { "2007-9", "�人��������ͨ����ʮ�������" },
        { "2008-05", "�봨�����" },
        { "2010-01", "5G�����з�" }
    };

    void Update()
    {
        realTimeSeconds += Time.deltaTime;

        // ÿ50�����һ��
        if (realTimeSeconds >= 50f)
        {
            realTimeSeconds -= 50f; // ����ʱ��
            gameTimeMonths += 2;
            shopSystem.AutoCheckout(); // �Զ����㹺�ﳵ
        }

        // ��ʾ�̳�UI�ͽ���UI
        if (realTimeSeconds < 20f)
        {
            shopUI.SetActive(true);
            settlementUI.SetActive(false);
            specialEventUI.SetActive(false); // ���������¼�UI
        }
        else if (realTimeSeconds >= 40f && realTimeSeconds < 50f)
        {
            shopUI.SetActive(false);
            settlementUI.SetActive(true);
            ShowSettlement();
            specialEventUI.SetActive(false); // ���������¼�UI
        }
        else
        {
            shopUI.SetActive(false);
            settlementUI.SetActive(false);
        }

        // ����ʱ����ʾ
        int currentYear = startingYear + (startingMonth + gameTimeMonths - 1) / 12;
        int currentMonth = (startingMonth + gameTimeMonths - 1) % 12 + 1;
        timeText.color = Color.black;
        timeText.text = $"{currentYear}-{currentMonth:D2}";

        // ����Ƿ�ﵽ��������
        string currentDateString = $"{currentYear:D4}-{currentMonth:D2}";
        if (specialEvents.ContainsKey(currentDateString))
        {
            specialEventUI.SetActive(true); // ��ʾ�����¼�UI
            specialEventText.color = Color.red; // ���������¼�������ɫ
            specialEventText.text = specialEvents[currentDateString]; // ��ʾ��������
        }
        else
        {
            specialEventUI.SetActive(false); // ���������¼�UI
        }
    }

    private void ShowSettlement()
    {
        int totalSpent = shopSystem.TotalSpent;
        int totalSellPrice = shopSystem.CalculateTotalSellPrice();

        
        settlementText.text = $"���㣺\n�ɱ�: {totalSpent}\n����: {totalSellPrice}";
    }
   
   
}
