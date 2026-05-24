using UnityEngine;
using TMPro;

/// <summary>
/// Manages the live in-game HUD: real-time score counters and pieces-remaining
/// counters for both players. Updated every time a piece is placed.
/// All display strings are resolved through <see cref="LocalizationManager"/>.
/// End-of-game results are handled by <see cref="GameOverUI"/>.
/// </summary>
[DefaultExecutionOrder(-50)]
public class ScoreUI : MonoBehaviour
{
    public static ScoreUI Instance { get; private set; }

    [Header("Live Scores")]
    [Tooltip("One element per player. Displays the running score throughout the game.")]
    public TextMeshProUGUI[] liveScoreTexts;

    [Header("Pieces Remaining")]
    [Tooltip("One element per player. Displays the number of unplayed pieces.")]
    public TextMeshProUGUI[] piecesRemainingTexts;

    private int[] lastScores = new int[2];

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;

        UpdatePiecesRemaining();

        if (ScoreManager.Instance != null)
        {
            UpdateScores(new int[]
            {
                ScoreManager.Instance.GetPlayerScore(0),
                ScoreManager.Instance.GetPlayerScore(1)
            });
        }
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
    }

    /// <summary>
    /// Refreshes the live score display.
    /// Called by <see cref="ScoreManager"/> after every piece placement.
    /// </summary>
    /// <param name="scores">Current score array indexed by player (0-based).</param>
    public void UpdateScores(int[] scores)
    {
        lastScores = scores;

        if (liveScoreTexts == null) return;

        for (int i = 0; i < scores.Length && i < liveScoreTexts.Length; i++)
        {
            if (liveScoreTexts[i] == null) continue;
            liveScoreTexts[i].text = $"{scores[i]}";
            liveScoreTexts[i].color = GameManager.Instance.playerColors[i];
        }
    }

    /// <summary>
    /// Refreshes the pieces-remaining counters for both players.
    /// </summary>
    public void UpdatePiecesRemaining()
    {
        if (piecesRemainingTexts == null || piecesRemainingTexts.Length < 2) return;

        for (int i = 0; i < 2; i++)
        {
            if (piecesRemainingTexts[i] == null) continue;
            int remaining = PiecePalette.Instance.GetAvailablePiecesForPlayer(i).Count;
            piecesRemainingTexts[i].text = string.Format(L(LocalizationKeys.PiecesRemaining), remaining);
            piecesRemainingTexts[i].color = GameManager.Instance.playerColors[i];
        }
    }

    private void OnLanguageChanged()
    {
        UpdatePiecesRemaining();
        UpdateScores(lastScores);
    }

    private string L(string key) =>
        LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText(key) : key;
}