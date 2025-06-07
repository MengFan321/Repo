using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCControllerWithSound : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;

    public AudioClip clickSound; // 拖入点击时播放的音效剪辑

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>(); // 获取挂载在本物体上的 AudioSource

        animator.ResetTrigger("Click Trigger");
    }

    void OnMouseDown()
    {
        animator.SetTrigger("Click Trigger");

        // 播放音效（如果设定了音频组件和剪辑）
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
