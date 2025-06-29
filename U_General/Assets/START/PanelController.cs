using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public GameObject panel;      // �������
    public Button openButton;     // �򿪰�ť
    public Button closeButton;    // �رհ�ť
    public Button quitButton;     // �˳���ť

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
        Debug.Log("�˳���Ϸ");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
