using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightClickMovement : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 5f; // 移动速度
    [SerializeField] private float stoppingDistance = 0.1f; // 停止距离阈值

    private Vector3 _targetPosition; // 目标位置
    private bool _isMoving = false;   // 移动状态标志
    private Camera _mainCamera;       // 主摄像机缓存

    void Start()
    {
        _mainCamera = Camera.main;
        _targetPosition = transform.position;
    }

    [Header("矩形范围设置")]
    public Vector2 minBoundary;  // 左下角的边界
    public Vector2 maxBoundary;  // 右上角的边界

    void Update()
    {
        HandleMouseInput();

        // 获取物体当前位置
        Vector3 position = transform.position;

        // 限制X轴和Y轴的位置
        position.x = Mathf.Clamp(position.x, minBoundary.x, maxBoundary.x);
        position.y = Mathf.Clamp(position.y, minBoundary.y, maxBoundary.y);

        // 更新物体的位置
        transform.position = position;
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // 处理鼠标输入
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(1)) // 检测右键点击
        {
            SetTargetPosition();
        }
    }

    // 设置目标位置
    private void SetTargetPosition()
    {
        // 转换鼠标位置到世界坐标
        Vector3 mouseWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;  // 确保Z轴为0，因为我们是2D游戏

        // 检查鼠标点击位置是否在矩形范围内
        if (mouseWorldPos.x >= minBoundary.x && mouseWorldPos.x <= maxBoundary.x &&
            mouseWorldPos.y >= minBoundary.y && mouseWorldPos.y <= maxBoundary.y)
        {
            // 如果在范围内，设置目标位置并开始移动
            _targetPosition = new Vector3(mouseWorldPos.x, mouseWorldPos.y, transform.position.z);
            _isMoving = true;
        }
        else
        {
            // 如果点击在范围外，则不进行任何操作
            _isMoving = false;
        }
    }

    // 处理移动逻辑
    private void HandleMovement()
    {
        if (!_isMoving) return;

        // 计算移动步长
        float step = moveSpeed * Time.fixedDeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, step);

        // 检查是否到达目标
        if (Vector3.Distance(transform.position, _targetPosition) < stoppingDistance)
        {
            _isMoving = false;
        }
    }

    // 可视化调试（可选）
    void OnDrawGizmosSelected()
    {
        // 画出矩形范围
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(minBoundary.x, minBoundary.y, 0), new Vector3(maxBoundary.x, minBoundary.y, 0)); // Bottom line
        Gizmos.DrawLine(new Vector3(maxBoundary.x, minBoundary.y, 0), new Vector3(maxBoundary.x, maxBoundary.y, 0)); // Right line
        Gizmos.DrawLine(new Vector3(maxBoundary.x, maxBoundary.y, 0), new Vector3(minBoundary.x, maxBoundary.y, 0)); // Top line
        Gizmos.DrawLine(new Vector3(minBoundary.x, maxBoundary.y, 0), new Vector3(minBoundary.x, minBoundary.y, 0)); // Left line

        // 画出目标位置
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _targetPosition);
        Gizmos.DrawWireSphere(_targetPosition, 0.2f);
    }
}

