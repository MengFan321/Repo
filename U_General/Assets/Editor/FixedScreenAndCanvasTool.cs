using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class FixedScreenAndCanvasTool : EditorWindow
{
    private int targetWidth = 2400;
    private int targetHeight = 1350;

    [MenuItem("Tools/画面+UI分辨率锁定助手")]
    public static void ShowWindow()
    {
        GetWindow<FixedScreenAndCanvasTool>("分辨率锁定助手");
    }

    void OnGUI()
    {
        GUILayout.Label("设置你的目标分辨率（建议与你UI设计一致）", EditorStyles.boldLabel);

        targetWidth = EditorGUILayout.IntField("参考宽度 (px)", targetWidth);
        targetHeight = EditorGUILayout.IntField("参考高度 (px)", targetHeight);

        GUILayout.Space(10);

        if (GUILayout.Button("一键锁定主相机画面比例（加黑边）"))
        {
            ApplyFixedCamera();
        }

        if (GUILayout.Button("一键配置 CanvasScaler UI 适配"))
        {
            ApplyCanvasScaler();
        }

        if (GUILayout.Button("🎯 一键完成所有设置"))
        {
            ApplyFixedCamera();
            ApplyCanvasScaler();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("🔍 检查 Canvas 是否缺少 PixelPerfect"))
        {
            CheckCanvasPixelPerfect();
        }
    }

    void ApplyFixedCamera()
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            EditorUtility.DisplayDialog("未找到主相机", "请确保场景中存在 Camera 且 Tag 为 MainCamera。", "好");
            return;
        }

        // 确保 FixedResolution 脚本可用
        var frType = typeof(FixedResolution);
        if (frType == null)
        {
            EditorUtility.DisplayDialog("错误", "找不到 FixedResolution 脚本，请确认已正确定义并放在 Assets 中。", "好");
            return;
        }

        FixedResolution fr = cam.GetComponent<FixedResolution>();
        if (fr == null)
        {
            fr = cam.gameObject.AddComponent<FixedResolution>();
            Debug.Log("🛠️ 已自动添加 FixedResolution 脚本到主相机");
        }

        fr.targetWidth = targetWidth;
        fr.targetHeight = targetHeight;

        EditorUtility.SetDirty(fr);
        Debug.Log("✅ 主相机画面比例锁定完成！");
    }


    void ApplyCanvasScaler()
    {
        CanvasScaler[] scalers = FindObjectsOfType<CanvasScaler>();
        if (scalers.Length == 0)
        {
            EditorUtility.DisplayDialog("未找到 CanvasScaler", "场景中没有 CanvasScaler 组件。", "好");
            return;
        }

        foreach (CanvasScaler scaler in scalers)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(targetWidth, targetHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            EditorUtility.SetDirty(scaler);
        }

        Debug.Log("✅ 所有 CanvasScaler 已配置为 Expand 模式！");
    }

    void CheckCanvasPixelPerfect()
    {
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        int missingCount = 0;

        foreach (Canvas canvas in canvases)
        {
            var pixelScript = canvas.GetComponent<UnityEngine.U2D.PixelPerfectCamera>();
            if (pixelScript == null)
            {
                Debug.LogWarning($"⚠️ Canvas \"{canvas.name}\" 没有挂载 PixelPerfectCamera 组件！");
                missingCount++;
            }
        }

        if (missingCount == 0)
        {
            EditorUtility.DisplayDialog("检查完成", "所有 Canvas 都已启用 Pixel Perfect（挂载组件）！", "好");
        }
        else
        {
            EditorUtility.DisplayDialog("⚠️ 检查完成", $"{missingCount} 个 Canvas 未启用 Pixel Perfect，请检查！", "明白");
        }
    }

}
