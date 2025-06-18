using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemMove : MonoBehaviour
{
    public float moveSpeed = 5.0f; // 物体向右移动的速度

    private Rigidbody2D rb; // 用于控制物体的刚体组件

    void Start()
    {
        // 获取物体的Rigidbody2D组件
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 在Update中控制物体向右移动
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
        // 只让物体向右移动，y 方向速度为 0
        rb.velocity = new Vector2(moveSpeed, 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 当物体进入触发器时，销毁当前物体
        Destroy(gameObject);
    }
}
