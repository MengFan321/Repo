using System.Collections.Generic;
using UnityEngine;



public class MusicTimeController : MonoBehaviour
{
    public TimeSystem_7 timeSystem;
    public MusicManager musicManager;

    [Header("�����л���")]
    public List<MusicChangeEntry> musicSchedule = new List<MusicChangeEntry>();

    private HashSet<string> triggeredDates = new HashSet<string>();

    void Update()
    {
        if (timeSystem == null || musicManager == null) return;

        string currentDate = timeSystem.CurrentDateString;

        foreach (var entry in musicSchedule)
        {
            if (entry.dateString == currentDate && !triggeredDates.Contains(currentDate))
            {
                musicManager.SwitchMusic(entry.musicIndex);
                triggeredDates.Add(currentDate); // �����ظ��л�
                break;
            }
        }
    }
}
[System.Serializable]
public class MusicChangeEntry
{
    public string dateString; // ��ʽ��"1987-05"
    public int musicIndex;    // ��Ӧ AudioClip ������
}