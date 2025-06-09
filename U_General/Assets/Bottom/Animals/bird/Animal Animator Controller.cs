using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;

    public AudioClip clickSound; // �������벥�ŵ���Ч

    // �� �� Awake ��Ԥ����Ƶ
    void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (clickSound != null)
        {
            // ǿ�ư���Ƶ���ݼ��ز����뵽�ڴ�
            clickSound.LoadAudioData();
        }

        animator.ResetTrigger("Click Trigger");
    }

    void OnMouseDown()
    {
        animator.SetTrigger("Click Trigger");

        // ������Ч
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
