using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;

    public AudioClip clickSound; // 拖入你想播放的音效

    // ① 在 Awake 中预热音频
    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (clickSound != null)
        {
            // 强制把音频数据加载并解码到内存
            clickSound.LoadAudioData();
        }

        animator.ResetTrigger("Click Trigger");
    }

    void OnMouseDown()
    {
        animator.SetTrigger("Click Trigger");

        // 播放音效
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
