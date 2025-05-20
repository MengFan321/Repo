using UnityEngine;

public class IconClickOpensPanel : MonoBehaviour
{
    public GameObject panelToOpen;  // �� Inspector ��ָ��Ҫ�򿪵� UI ����

    void OnMouseDown()
    {
        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);  // ���� UI ���
        }
    }
}
