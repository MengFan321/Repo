using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeSystem : MonoBehaviour
{
    public TextMeshProUGUI timeText; // ��ʾ��Ϸʱ���Text���
    public GameObject shopUI; // �̳�UI

    private float realTimeSeconds = 0f;
    private int gameTimeMonths = 0;
    private int startingYear = 1985;
    private int startingMonth = 1;
    private bool isShopOpen = false; // �̳��Ƿ񿪷�

    void Update()
    {
        // ������ʵʱ�������
        realTimeSeconds += Time.deltaTime;

        // ÿ��50����ʵʱ�䣬��Ϸʱ������һ����
        if (realTimeSeconds >= 50f)
        {
            realTimeSeconds -= 50f;
            gameTimeMonths++;
            isShopOpen = false; // �ر��̳�
        }

        // ÿ���µ�ǰ20�뿪���̳�
        if (realTimeSeconds < 20f)
        {
            isShopOpen = true;
            shopUI.SetActive(true); // ��ʾ�̳�UI
        }
        else
        {
            isShopOpen = false;
            shopUI.SetActive(false); // �����̳�UI
        }

        // ���㵱ǰ����ݺ��·�
        int currentYear = startingYear + (startingMonth + gameTimeMonths - 1) / 12;
        int currentMonth = (startingMonth + gameTimeMonths - 1) % 12 + 1;

        // ��ʾ��Ϸʱ��
        timeText.text = currentYear + "-" + currentMonth.ToString("D2");
    }

    // ����Ƿ���Բ���
    public bool CanOperate()
    {
        if (isShopOpen)
        {
            return true;
        }
        else
        {
            Debug.Log("�����Բ�����");
            return false;
        }
    }
}


