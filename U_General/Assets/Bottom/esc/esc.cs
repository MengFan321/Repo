using UnityEngine;
using UnityEngine.UI;

public class ExitPanelController : MonoBehaviour
{
    public GameObject exitPanel;     // UI���
    public Button quitButton;        // �˳���ť

    private bool isPanelVisible = false;

    void Start()
    {
        exitPanel.SetActive(false); // ��ʼʱ�������

        quitButton.onClick.AddListener(QuitGame); // ���˳���ť
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
        // �༭�����˳�
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ����������˳�
        Application.Quit();
#endif
    }
}
