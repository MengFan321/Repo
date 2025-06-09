using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public struct YearMonth
{
    public int year;   // ��
    public int month;  // �£�1-12��
}

[Serializable]
public class SwitchGroup
{
    [Tooltip("��������֣���������")]
    public string name;
    [Tooltip("Ҫ�ֻ���ʾ�������б�˳�򼴸��׶���ʾ˳��")]
    public List<GameObject> objects;
    [Tooltip("�л���ֵ�б� (���� = objects.Count - 1)��\nÿ����ֵ�������һ����(��)�������һ���׶�")]
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
            Debug.LogError("�Ҳ��� TimeSystem_7.Instance�����鵥������");
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
        // ���㡰�¡�Ϊ��λ���ۼ�ֵ������Ƚ�
        int curTotal = (timeSystem.CurrentGameYear * 12) + (timeSystem.CurrentGameMonth - 1);

        foreach (var group in groups)
        {
            // �ĸ��׶�
            int stage = 0;
            for (int i = 0; i < group.thresholds.Count; i++)
            {
                var th = group.thresholds[i];
                int thTotal = th.year * 12 + (th.month - 1);
                if (curTotal >= thTotal) stage = i + 1;
            }
            stage = Mathf.Clamp(stage, 0, group.objects.Count - 1);

            // �����Ӧ�׶ε����壬��������
            for (int j = 0; j < group.objects.Count; j++)
            {
                group.objects[j].SetActive(j == stage);
            }
        }
    }
}
