using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct YearMonth
{
    public int year;   // 年
    public int month;  // 月（1-12）
}

[Serializable]
public class SwitchGroup
{
    [Tooltip("本组的名字，仅供区分")]
    public string name;
    [Tooltip("要轮换显示的物体列表，顺序即各阶段显示顺序")]
    public List<GameObject> objects;
    [Tooltip("切换阈值列表 (数量 = objects.Count - 1)，\n每个阈值定义从这一年月(含)起进入下一个阶段")]
    public List<YearMonth> thresholds;
}

public class MultiGroupSwitcher : MonoBehaviour
{
    public List<SwitchGroup> groups;

    private TimeSystem_7 timeSystem;

    void Start()
    {
        timeSystem = TimeSystem_7.Instance;
        if (timeSystem == null)
        {
            Debug.LogError("找不到 TimeSystem_7.Instance，请检查单例设置");
            enabled = false;
            return;
        }
        UpdateAllGroups();
    }

    void Update()
    {
        UpdateAllGroups();
    }

    private void UpdateAllGroups()
    {
        // 计算“月”为单位的累计值，方便比较
        int curTotal = (timeSystem.CurrentGameYear * 12) + (timeSystem.CurrentGameMonth - 1);

        foreach (var group in groups)
        {
            // 哪个阶段
            int stage = 0;
            for (int i = 0; i < group.thresholds.Count; i++)
            {
                var th = group.thresholds[i];
                int thTotal = th.year * 12 + (th.month - 1);
                if (curTotal >= thTotal) stage = i + 1;
            }
            stage = Mathf.Clamp(stage, 0, group.objects.Count - 1);

            // 激活对应阶段的物体，隐藏其余
            for (int j = 0; j < group.objects.Count; j++)
            {
                group.objects[j].SetActive(j == stage);
            }
        }
    }
}
