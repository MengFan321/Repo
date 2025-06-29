using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct YearMonth
{
    public int year;
    public int month; // 1~12
}

[Serializable]
public class AlertSettings
{
    [Tooltip("感叹号相对目标物体的偏移位置")]
    public Vector3 offset = new Vector3(0, 2, 0);

    [Tooltip("感叹号显示时间（秒）")]
    public float displayTime = 3f;
}

[Serializable]
public class SwitchGroup
{
    [Tooltip("本组的名字，仅供区分")]
    public string name;

    [Tooltip("要轮换显示的物体列表，顺序即各阶段显示顺序")]
    public List<GameObject> objects;

    [Tooltip("切换阈值列表 (数量 = objects.Count - 1)，每个阈值定义从这一年月(含)起进入下一个阶段")]
    public List<YearMonth> thresholds;

    [Tooltip("每个阶段的感叹号提示配置（数量 = objects.Count）")]
    public List<AlertSettings> alertSettings;
}

public class MultiGroupSwitcher : MonoBehaviour
{
    public List<SwitchGroup> groups;

    [Header("感叹号预制体和音效")]
    public GameObject alertPrefab;   // 注意：必须是Project面板里的Prefab
    public AudioClip alertSound;

    private TimeSystem_7 timeSystem;
    private Dictionary<string, int> lastStages = new Dictionary<string, int>(); // 用组名做Key更稳

    void Start()
    {
        timeSystem = TimeSystem_7.Instance;
        if (timeSystem == null)
        {
            Debug.LogError("找不到 TimeSystem_7.Instance，请检查单例设置");
            enabled = false;
            return;
        }

        // 初始化lastStages为当前阶段，不触发提示
        int curTotal = (timeSystem.CurrentGameYear * 12) + (timeSystem.CurrentGameMonth - 1);
        foreach (var group in groups)
        {
            int stage = 0;
            for (int i = 0; i < group.thresholds.Count; i++)
            {
                var th = group.thresholds[i];
                int thTotal = th.year * 12 + (th.month - 1);
                if (curTotal >= thTotal) stage = i + 1;
            }
            stage = Mathf.Clamp(stage, 0, group.objects.Count - 1);
            lastStages[group.name] = stage;
        }

        UpdateAllGroups(); // 首帧刷新显示
    }

    void Update()
    {
        UpdateAllGroups();
    }

    private void UpdateAllGroups()
    {
        int curTotal = (timeSystem.CurrentGameYear * 12) + (timeSystem.CurrentGameMonth - 1);

        foreach (var group in groups)
        {
            int stage = 0;
            for (int i = 0; i < group.thresholds.Count; i++)
            {
                var th = group.thresholds[i];
                int thTotal = th.year * 12 + (th.month - 1);
                if (curTotal >= thTotal) stage = i + 1;
            }
            stage = Mathf.Clamp(stage, 0, group.objects.Count - 1);

            // 检查阶段是否变化
            if (lastStages.TryGetValue(group.name, out int lastStage))
            {
                // 检查阶段是否变化
                if (stage != lastStage)
                {
                    // —— 先打开/关闭所有建筑 —— 
                    for (int j = 0; j < group.objects.Count; j++)
                        group.objects[j].SetActive(j == stage);

                    // —— 再去弹感叹号 —— 
                    GameObject target = group.objects[stage];
                    // 这时候 target.activeSelf 一定是 true
                    if (target != null && alertPrefab != null)
                    {
                        AlertSettings s = (group.alertSettings != null && stage < group.alertSettings.Count)
                            ? group.alertSettings[stage]
                            : new AlertSettings();

                        if (s.displayTime > 0f)
                        {
                            Vector3 spawnPos = target.transform.position + s.offset;
                            Debug.Log($"即将生成感叹号，位置：{spawnPos}");
                            GameObject alert = Instantiate(alertPrefab, spawnPos, Quaternion.identity);
                            Debug.Log($"感叹号实例已生成，实例名：{alert.name} 位置：{alert.transform.position}");
                            alert.transform.SetParent(target.transform);
                            Animator anim = alert.GetComponent<Animator>();
                            if (anim != null) anim.SetTrigger("Bounce");
                            Destroy(alert, s.displayTime);
                        }
                    }

                    if (alertSound != null)
                        AudioSource.PlayClipAtPoint(alertSound, Camera.main.transform.position);

                    lastStages[group.name] = stage;
                }
                else
                {
                    // 如果阶段没变，也要确保显示状态正确
                    for (int j = 0; j < group.objects.Count; j++)
                        group.objects[j].SetActive(j == stage);
                }

            }
            else
            {
                lastStages[group.name] = stage; // 首次注册不触发感叹号             
            }

            // 控制显示
            for (int j = 0; j < group.objects.Count; j++)
                group.objects[j].SetActive(j == stage);
        }
    }
}
