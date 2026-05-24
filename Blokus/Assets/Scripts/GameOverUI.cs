using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the game-over overlay. Displays the winner announcement and final scores
/// for both players, and provides options to restart or return to the main menu.
/// All display strings are resolved through <see cref="LocalizationManager"/> and
/// refresh automatically when the active language changes.
/// Call <see cref="Show"/> from GameManager when the game ends.
/// </summary>
[DefaultExecutionOrder(-50)]
public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [Header("Buttons")]
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Result Display")]
    [Tooltip("Text that announces the winner or a tie.")]
    public TextMeshProUGUI winnerText;
    [Tooltip("One element per player (index 0 = Player 1). Displays the final score.")]
    public TextMeshProUGUI[] playerScoreTexts;

    private int[] lastScores;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        restartButton.onClick.AddListener(RestartGame);
        mainMenuButton.onClick.AddListener(GoToMenu);

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += RefreshDisplay;

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= RefreshDisplay;
    }

    /// <summary>
    /// Activates the game-over overlay and populates it with final scores and the winner message.
    /// Stores the scores so the display can be refreshed if the language changes while visible.
    /// </summary>
    /// <param name="scores">Final score array indexed by player (0-based).</param>
    public void Show(int[] scores)
    {
        lastScores = scores;
        gameObject.SetActive(true);
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (lastScores == null) return;

        if (winnerText != null)
        {
            winnerText.text = lastScores[0] > lastScores[1] ? L(LocalizationKeys.WinnerP1)
                            : lastScores[1] > lastScores[0] ? L(LocalizationKeys.WinnerP2)
                            : L(LocalizationKeys.Tie);
        }

        for (int i = 0; i < playerScoreTexts.Length && i < lastScores.Length; i++)
        {
            if (playerScoreTexts[i] == null) continue;
            playerScoreTexts[i].text = string.Format(L(LocalizationKeys.FinalScore), i + 1, lastScores[i]);
            playerScoreTexts[i].color = GameManager.Instance.playerColors[i];
        }
    }

    private void RestartGame()
    {
        GameManager.Instance.ResetGame();
        ScoreManager.Instance.InitializeScores();
        gameObject.SetActive(false);
    }

    private void GoToMenu()
    {
        Destroy(GameManager.Instance.gameObject);
        SceneManager.LoadScene("MainMenuScene");
    }

    private string L(string key) =>
        LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText(key) : key;
}