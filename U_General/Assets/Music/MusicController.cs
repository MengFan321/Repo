using System.Collections.Generic;
using UnityEngine;



public class MusicTimeController : MonoBehaviour
{
    public TimeSystem_7 timeSystem;
    public MusicManager musicManager;

    [Header("音乐切换表")]
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
                triggeredDates.Add(currentDate); // 避免重复切换
                break;
            }
        }
    }
}
[System.Serializable]
public class MusicChangeEntry
{
    public string dateString; // 格式："1987-05"
    public int musicIndex;    // 对应 AudioClip 的索引
}