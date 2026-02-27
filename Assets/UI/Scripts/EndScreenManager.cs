using UnityEngine;
using UnityEngine.SceneManagement;
public class EndScreenManager : MonoBehaviour
{

    public void Respawn()
    {
        SceneManager.LoadScene(1);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
