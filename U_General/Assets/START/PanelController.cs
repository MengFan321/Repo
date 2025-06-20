using UnityEngine;
using UnityEngine.UI;

public class PanelController : MonoBehaviour
{
    public GameObject panel;       // ��Ҫ���������
    public Button openButton;      // ���ڴ����İ�ť

    void Start()
    {
        // Ĭ���������
        panel.SetActive(false);

        // ����ť��ӵ������
        openButton.onClick.AddListener(ShowPanel);
    }

    void ShowPanel()
    {
        panel.SetActive(true);
    }
}
