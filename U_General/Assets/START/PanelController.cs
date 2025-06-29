using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public GameObject panel;      // 设置面板
    public Button openButton;     // 打开按钮
    public Button closeButton;    // 关闭按钮
    public Button quitButton;     // 退出按钮

    void Start()
    {
        panel.SetActive(false);

        openButton.onClick.AddListener(ShowPanel);
        closeButton.onClick.AddListener(HidePanel);
        quitButton.onClick.AddListener(QuitGame);
    }

    public void ShowPanel()
    {
        panel.SetActive(true);
    }

    public void HidePanel()
    {
        panel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
