using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class FixedResolution : MonoBehaviour
{
    public int targetWidth = 2400;
    public int targetHeight = 1350;

    void Update()
    {
        if (targetHeight == 0) return;

        float targetAspect = (float)targetWidth / targetHeight;
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera cam = GetComponent<Camera>();

        if (scaleHeight < 1.0f)
        {
            cam.rect = new Rect(0, (1f - scaleHeight) / 2f, 1f, scaleHeight);
        }
        else
        {
            float scaleWidth = 1.0f / scaleHeight;
            cam.rect = new Rect((1f - scaleWidth) / 2f, 0, scaleWidth, 1f);
        }
    }
}
