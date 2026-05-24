using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the in-game pause menu, toggled by the Escape key.
/// Pauses <see cref="Time.timeScale"/> while the menu is open and provides
/// options to resume, restart the current match, return to the main menu, or quit.
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Panel")]
    [Tooltip("Root panel of the pause menu. Shown/hidden by this script.")]
    [SerializeField] private GameObject pausePanel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    private bool isPaused;

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);

        resumeButton.onClick.AddListener(Resume);
        restartButton.onClick.AddListener(Restart);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SetPaused(!isPaused);
    }

    /// <summary>
    /// Closes the pause menu and resumes gameplay.
    /// </summary>
    public void Resume() => SetPaused(false);

    private void SetPaused(bool value)
    {
        isPaused = value;
        if (pausePanel != null) pausePanel.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    private void Restart()
    {
        SetPaused(false);
        GameManager.Instance.ResetGame();
        ScoreManager.Instance.InitializeScores();
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        Destroy(GameManager.Instance.gameObject);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}