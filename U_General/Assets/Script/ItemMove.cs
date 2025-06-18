using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMove : MonoBehaviour
{
    public float moveSpeed = 5.0f; // ���������ƶ����ٶ�

    private Rigidbody2D rb; // ���ڿ�������ĸ������

    void Start()
    {
        // ��ȡ�����Rigidbody2D���
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // ��Update�п������������ƶ�
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        // ֻ�����������ƶ���y �����ٶ�Ϊ 0
        rb.velocity = new Vector2(moveSpeed, 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ��������봥����ʱ�����ٵ�ǰ����
        Destroy(gameObject);
    }
}
