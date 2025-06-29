using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;    // 音乐播放器
    public AudioClip[] bgmClips;      // 存储音乐片段

    private int lastMusicIndex = -1;  // 上一个播放的音乐索引

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    // 切换背景音乐
    public void SwitchMusic(int index)
    {
        if (index < 0 || index >= bgmClips.Length) return;

        // 如果索引没有变化，不切换音乐
        if (index == lastMusicIndex) return;

        audioSource.clip = bgmClips[index];
        audioSource.Play();
        lastMusicIndex = index;
    }
}
