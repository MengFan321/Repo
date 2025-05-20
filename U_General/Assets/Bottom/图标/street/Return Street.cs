using UnityEngine;

public class CloseTwoPanelsOnClick : MonoBehaviour
{
    public GameObject panel1;  // ��һ������
    public GameObject panel2;  // �ڶ�������

    void OnMouseDown()
    {
        if (panel1 != null)
        {
            panel1.SetActive(false);
        }

        if (panel2 != null)
        {
            panel2.SetActive(false);
        }
    }
}
