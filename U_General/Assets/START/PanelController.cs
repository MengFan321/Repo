using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public GameObject panel;       // 需要弹出的面板
    public Button openButton;      // 用于打开面板的按钮

    void Start()
    {
        // 默认隐藏面板
        panel.SetActive(false);

        // 给按钮添加点击监听
        openButton.onClick.AddListener(ShowPanel);
    }

    void ShowPanel()
    {
        panel.SetActive(true);
    }
}
