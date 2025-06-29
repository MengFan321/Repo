using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;    // ���ֲ�����
    public AudioClip[] bgmClips;      // �洢����Ƭ��

    private int lastMusicIndex = -1;  // ��һ�����ŵ���������

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    // �л���������
    public void SwitchMusic(int index)
    {
        if (index < 0 || index >= bgmClips.Length) return;

        // �������û�б仯�����л�����
        if (index == lastMusicIndex) return;

        audioSource.clip = bgmClips[index];
        audioSource.Play();
        lastMusicIndex = index;
    }
}
