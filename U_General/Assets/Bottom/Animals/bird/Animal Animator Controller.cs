using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;

    public AudioClip clickSound; // 拖入你想播放的音效

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>(); // 获取挂载在本物体上的 AudioSource

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
