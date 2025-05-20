using UnityEngine;

public class CloseTwoPanelsOnClick : MonoBehaviour
{
    public GameObject panel1;  // 第一个窗口
    public GameObject panel2;  // 第二个窗口

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
