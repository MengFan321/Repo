using UnityEngine;

public class IconClickOpensPanel : MonoBehaviour
{
    public GameObject panelToOpen;  // 在 Inspector 中指定要打开的 UI 界面

    void OnMouseDown()
    {
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);  // 激活 UI 面板
        }
    }
}
