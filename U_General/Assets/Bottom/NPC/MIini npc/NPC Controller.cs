using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCControllerWithSound : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;

    [Header("点击音效")]
    public AudioClip clickSound; // 拖入点击时播放的音效剪辑

    void Awake()
    {
        // ① 获取组件
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // ② 预热音效：强制把压缩数据解码到内存
        if (clickSound != null)
        {
            clickSound.LoadAudioData();
        }
    }

    void Start()
    {
        // 重置触发器状态
        animator.ResetTrigger("Click Trigger");
    }

    void OnMouseDown()
    {
        // 触发动画
        animator.SetTrigger("Click Trigger");

        // 播放音效
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
