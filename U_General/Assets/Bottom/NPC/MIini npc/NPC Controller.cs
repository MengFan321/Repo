using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCControllerWithSound : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;

    public AudioClip clickSound; // ������ʱ���ŵ���Ч����

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>(); // ��ȡ�����ڱ������ϵ� AudioSource

        animator.ResetTrigger("Click Trigger");
    }

    void OnMouseDown()
    {
        animator.SetTrigger("Click Trigger");

        // ������Ч������趨����Ƶ����ͼ�����
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
