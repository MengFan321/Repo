using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelChanger : MonoBehaviour
{
    // 引用时间系统
    public TimeSystem_7 timeSystem;

    // 需要更换底图的Panel
    public List<PanelImageChange> panelsToChange;

    // 用于存储特定年份和月份以及对应的底图
    [System.Serializable]
    public class PanelImageChange
    {
        public GameObject panel; // 需要更换底图的Panel
        public List<YearMonthImagePair> yearMonthImagePairs; // 特定年份和月份以及对应的底图
    }

    [System.Serializable]
    public class YearMonthImagePair
    {
        public int year; // 特定年份
        public int month; // 特定月份
        public Sprite image; // 对应的底图
    }

    private void Start()
    {
        // 确保时间系统已经初始化
        if (timeSystem == null)
        {
            Debug.LogError("TimeSystem_7 引用未设置！");
            return;
        }

        // 注册时间系统的变化事件
        timeSystem.onTimeChanged += OnTimeChanged;

        // 初始检查并设置底图
        CheckAndChangePanelImages();
    }

    private void OnTimeChanged()
    {
        // 每当时间变化时，检查并更换底图
        CheckAndChangePanelImages();
    }

    private void CheckAndChangePanelImages()
    {
        // 遍历所有需要更换底图的Panel
        foreach (var panelChange in panelsToChange)
        {
            if (panelChange.panel == null)
            {
                Debug.LogWarning("Panel 引用未设置！");
                continue;
            }

            // 获取当前年份和月份
            int currentYear = timeSystem.CurrentGameYear;
            int currentMonth = timeSystem.CurrentGameMonth;

            // 遍历该Panel的所有年份和月份以及底图对
            foreach (var yearMonthImagePair in panelChange.yearMonthImagePairs)
            {
                if (currentYear == yearMonthImagePair.year && currentMonth == yearMonthImagePair.month)
                {
                    // 如果当前年份和月份匹配，更换底图
                    Image panelImage = panelChange.panel.GetComponent<Image>();
                    if (panelImage != null)
                    {
                        panelImage.sprite = yearMonthImagePair.image;
                    }
                    else
                    {
                        Debug.LogWarning("Panel 没有 Image 组件！");
                    }
                    break; // 找到合适的底图后退出循环
                }
            }
        }
    }

    private void OnDestroy()
    {
        // 注销时间系统的变化事件
        if (timeSystem != null)
        {
            timeSystem.onTimeChanged -= OnTimeChanged;
        }
    }
}
