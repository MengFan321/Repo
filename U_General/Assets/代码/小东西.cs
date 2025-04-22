using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class 小东西 : MonoBehaviour
{
    public Transform[] pathPoints; // 路径点
    public float moveSpeed = 2f;   // 移动速度
    public float stopTime = 2f;    // 停留时间（秒）

    private int currentPointIndex = 0;  // 当前路径点索引
    private bool isMoving = true;   // 判断是否在移动

    void Start()
    {
        // 确保有路径点
        if (pathPoints.Length > 0)
        {
            // 开始沿着路径移动
            StartCoroutine(MoveAlongPath());
        }
    }

    IEnumerator MoveAlongPath()
    {
        while (true)
        {
            // 如果物体正在移动
            if (isMoving)
            {
                // 移动到当前路径点
                MoveToCurrentPoint();

                // 判断是否到达路径点
                if (Vector3.Distance(transform.position, pathPoints[currentPointIndex].position) < 0.1f)
                {
                    // 停留几秒
                    isMoving = false;
                    yield return new WaitForSeconds(stopTime);

                    // 到达路径点后，进入下一路径点
                    currentPointIndex = (currentPointIndex + 1) % pathPoints.Length;

                    // 再次开始移动
                    isMoving = true;
                }
            }

            yield return null;  // 等待下一帧
        }
    }

    void MoveToCurrentPoint()
    {
        // 移动物体
        transform.position = Vector3.MoveTowards(transform.position, pathPoints[currentPointIndex].position, moveSpeed * Time.deltaTime);
    }

    // 在 Scene 视图中绘制 Gizmos
    void OnDrawGizmos()
    {
        // 如果路径点不为空
        if (pathPoints.Length > 0)
        {
            // 绘制路径点
            foreach (Transform pathPoint in pathPoints)
            {
                Gizmos.color = Color.red;  // 设置路径点颜色为红色
                Gizmos.DrawSphere(pathPoint.position, 0.1f);  // 绘制路径点（球形）
            }

            // 绘制路径线
            Gizmos.color = Color.green;  // 设置路径线颜色为绿色
            for (int i = 0; i < pathPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(pathPoints[i].position, pathPoints[i + 1].position);  // 绘制路径线
            }

            // 绘制最后一段路径线（连接最后一个路径点和第一个路径点）
            Gizmos.DrawLine(pathPoints[pathPoints.Length - 1].position, pathPoints[0].position);
        }
    }
}

