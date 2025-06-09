using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCControllerWithSound : MonoBehaviour
{
    private Animator animator;
    private AudioSource audioSource;

    [Header("�����Ч")]
    public AudioClip clickSound; // ������ʱ���ŵ���Ч����

    void Awake()
    {
        // �� ��ȡ���
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // �� Ԥ����Ч��ǿ�ư�ѹ�����ݽ��뵽�ڴ�
        if (clickSound != null)
        {
            clickSound.LoadAudioData();
        }
    }

    void Start()
    {
        // ���ô�����״̬
        animator.ResetTrigger("Click Trigger");
    }

    void OnMouseDown()
    {
        // ��������
        animator.SetTrigger("Click Trigger");

        // ������Ч
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
