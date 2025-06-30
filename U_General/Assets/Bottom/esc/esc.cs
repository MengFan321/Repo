using UnityEngine;
using UnityEngine.UI;

public class ExitPanelController : MonoBehaviour
{
    public GameObject exitPanel;     // UI面板
    public Button quitButton;        // 退出按钮

    private bool isPanelVisible = false;

    void Start()
    {
        exitPanel.SetActive(false); // 开始时隐藏面板

        quitButton.onClick.AddListener(QuitGame); // 绑定退出按钮
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPanelVisible = !isPanelVisible;
            exitPanel.SetActive(isPanelVisible);
        }
    }

    void QuitGame()
    {
        // 编辑器中退出
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 真机或打包后退出
        Application.Quit();
#endif
    }
}
