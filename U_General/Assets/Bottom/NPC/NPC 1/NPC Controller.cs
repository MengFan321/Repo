using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
       
        animator.ResetTrigger("Click Trigger");
    }

    void OnMouseDown()
    {
        animator.SetTrigger("Click Trigger");
    }
}
