using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages main menu UI panels and button interactions
/// Toggles between main menu and controls panel
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject MenuPanel;
    [SerializeField] private GameObject ControlsPanel;

    private bool m_isControlPanel = false;

    private void Awake()
    {
        MenuPanel.SetActive(true);
        ControlsPanel.SetActive(false);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
        BgmManager.Instance.PlayBGM("in_game_bgm", 0.1f);
    }

    // Toggles between menu and controls panel
    public void Controls()
    {
        if (m_isControlPanel)
        {
            MenuPanel.SetActive(true);
            ControlsPanel.SetActive(false);
        }
        if (!m_isControlPanel)
        {
            MenuPanel.SetActive(false);
            ControlsPanel.SetActive(true);
        }

        m_isControlPanel = !m_isControlPanel;
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
