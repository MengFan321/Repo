using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelChanger : MonoBehaviour
{
    // ����ʱ��ϵͳ
    public TimeSystem_7 timeSystem;

    // ��Ҫ������ͼ��Panel
    public List<PanelImageChange> panelsToChange;

    // ���ڴ洢�ض���ݺ��·��Լ���Ӧ�ĵ�ͼ
    [System.Serializable]
    public class PanelImageChange
    {
        public GameObject panel; // ��Ҫ������ͼ��Panel
        public List<YearMonthImagePair> yearMonthImagePairs; // �ض���ݺ��·��Լ���Ӧ�ĵ�ͼ
    }

    [System.Serializable]
    public class YearMonthImagePair
    {
        public int year; // �ض����
        public int month; // �ض��·�
        public Sprite image; // ��Ӧ�ĵ�ͼ
    }

    private void Start()
    {
        // ȷ��ʱ��ϵͳ�Ѿ���ʼ��
        if (timeSystem == null)
        {
            Debug.LogError("TimeSystem_7 ����δ���ã�");
            return;
        }

        // ע��ʱ��ϵͳ�ı仯�¼�
        timeSystem.onTimeChanged += OnTimeChanged;

        // ��ʼ��鲢���õ�ͼ
        CheckAndChangePanelImages();
    }

    private void OnTimeChanged()
    {
        // ÿ��ʱ��仯ʱ����鲢������ͼ
        CheckAndChangePanelImages();
    }

    private void CheckAndChangePanelImages()
    {
        // ����������Ҫ������ͼ��Panel
        foreach (var panelChange in panelsToChange)
        {
            if (panelChange.panel == null)
            {
                Debug.LogWarning("Panel ����δ���ã�");
                continue;
            }

            // ��ȡ��ǰ��ݺ��·�
            int currentYear = timeSystem.CurrentGameYear;
            int currentMonth = timeSystem.CurrentGameMonth;

            // ������Panel��������ݺ��·��Լ���ͼ��
            foreach (var yearMonthImagePair in panelChange.yearMonthImagePairs)
            {
                if (currentYear == yearMonthImagePair.year && currentMonth == yearMonthImagePair.month)
                {
                    // �����ǰ��ݺ��·�ƥ�䣬������ͼ
                    Image panelImage = panelChange.panel.GetComponent<Image>();
                    if (panelImage != null)
                    {
                        panelImage.sprite = yearMonthImagePair.image;
                    }
                    else
                    {
                        Debug.LogWarning("Panel û�� Image �����");
                    }
                    break; // �ҵ����ʵĵ�ͼ���˳�ѭ��
                }
            }
        }
    }

    private void OnDestroy()
    {
        // ע��ʱ��ϵͳ�ı仯�¼�
        if (timeSystem != null)
        {
            timeSystem.onTimeChanged -= OnTimeChanged;
        }
    }
}
