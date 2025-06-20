using UnityEngine.SceneManagement;
using UnityEngine;
public class SceneChanger : MonoBehaviour
{
    public void LoadTargetScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
