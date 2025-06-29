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
    [Tooltip("��̾�����Ŀ�������ƫ��λ��")]
    public Vector3 offset = new Vector3(0, 2, 0);

    [Tooltip("��̾����ʾʱ�䣨�룩")]
    public float displayTime = 3f;
}

[Serializable]
public class SwitchGroup
{
    [Tooltip("��������֣���������")]
    public string name;

    [Tooltip("Ҫ�ֻ���ʾ�������б�˳�򼴸��׶���ʾ˳��")]
    public List<GameObject> objects;

    [Tooltip("�л���ֵ�б� (���� = objects.Count - 1)��ÿ����ֵ�������һ����(��)�������һ���׶�")]
    public List<YearMonth> thresholds;

    [Tooltip("ÿ���׶εĸ�̾����ʾ���ã����� = objects.Count��")]
    public List<AlertSettings> alertSettings;
}

public class MultiGroupSwitcher : MonoBehaviour
{
    public List<SwitchGroup> groups;

    [Header("��̾��Ԥ�������Ч")]
    public GameObject alertPrefab;   // ע�⣺������Project������Prefab
    public AudioClip alertSound;

    private TimeSystem_7 timeSystem;
    private Dictionary<string, int> lastStages = new Dictionary<string, int>(); // ��������Key����

    void Start()
    {
        timeSystem = TimeSystem_7.Instance;
        if (timeSystem == null)
        {
            Debug.LogError("�Ҳ��� TimeSystem_7.Instance�����鵥������");
            enabled = false;
            return;
        }

        // ��ʼ��lastStagesΪ��ǰ�׶Σ���������ʾ
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

        UpdateAllGroups(); // ��֡ˢ����ʾ
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

            // ���׶��Ƿ�仯
            if (lastStages.TryGetValue(group.name, out int lastStage))
            {
                // ���׶��Ƿ�仯
                if (stage != lastStage)
                {
                    // ���� �ȴ�/�ر����н��� ���� 
                    for (int j = 0; j < group.objects.Count; j++)
                        group.objects[j].SetActive(j == stage);

                    // ���� ��ȥ����̾�� ���� 
                    GameObject target = group.objects[stage];
                    // ��ʱ�� target.activeSelf һ���� true
                    if (target != null && alertPrefab != null)
                    {
                        AlertSettings s = (group.alertSettings != null && stage < group.alertSettings.Count)
                            ? group.alertSettings[stage]
                            : new AlertSettings();

                        if (s.displayTime > 0f)
                        {
                            Vector3 spawnPos = target.transform.position + s.offset;
                            Debug.Log($"�������ɸ�̾�ţ�λ�ã�{spawnPos}");
                            GameObject alert = Instantiate(alertPrefab, spawnPos, Quaternion.identity);
                            Debug.Log($"��̾��ʵ�������ɣ�ʵ������{alert.name} λ�ã�{alert.transform.position}");
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
                    // ����׶�û�䣬ҲҪȷ����ʾ״̬��ȷ
                    for (int j = 0; j < group.objects.Count; j++)
                        group.objects[j].SetActive(j == stage);
                }

            }
            else
            {
                lastStages[group.name] = stage; // �״�ע�᲻������̾��             
            }

            // ������ʾ
            for (int j = 0; j < group.objects.Count; j++)
                group.objects[j].SetActive(j == stage);
        }
    }
}
