using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the turn-indicator HUD. Applies visual emphasis to the active player's
/// label and shows an animated "AI is thinking" indicator during the AI's move delay.
/// All display strings are resolved through <see cref="LocalizationManager"/> and
/// refresh automatically when the active language changes.
/// </summary>
[DefaultExecutionOrder(-50)]
public class TurnUI : MonoBehaviour
{
    public static TurnUI Instance { get; private set; }

    [Header("Turn Texts")]
    [Tooltip("One text element per player. The active player receives larger, bolder styling.")]
    public TextMeshProUGUI[] playerTurnTexts;

    [Header("AI Thinking Indicator")]
    [Tooltip("Root panel shown only while the AI is processing its move.")]
    public GameObject aiThinkingPanel;
    [Tooltip("Text inside the AI thinking panel. Driven by an animated dots cycle.")]
    public TextMeshProUGUI aiThinkingText;

    [Header("Format Settings")]
    public int normalFontSize = 24;
    public int activeFontSize = 32;
    public FontWeight normalFontWeight = FontWeight.Regular;
    public FontWeight activeFontWeight = FontWeight.Bold;

    [Header("Color Settings")]
    [Range(0f, 1f)]
    [Tooltip("Alpha applied to the inactive player's label color.")]
    public float inactiveColorAlpha = 0.5f;

    private Coroutine thinkingDotsCoroutine;
    private int lastKnownPlayer;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (aiThinkingPanel != null)
            aiThinkingPanel.SetActive(false);
    }

    private void Start()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += RefreshTurnTexts;
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= RefreshTurnTexts;
    }

    /// <summary>
    /// Refreshes all player turn labels, emphasising the active player.
    /// Stores the current player index so the display can be refreshed on language change.
    /// </summary>
    /// <param name="currentPlayer">Zero-based index of the player whose turn just began.</param>
    public void UpdateTurnUI(int currentPlayer)
    {
        lastKnownPlayer = currentPlayer;
        RefreshTurnTexts();
    }

    /// <summary>
    /// Shows or hides the animated "AI is thinking" indicator.
    /// Call with <c>true</c> at the start of the AI's turn and <c>false</c> when it ends.
    /// </summary>
    /// <param name="show">Whether to display the indicator.</param>
    public void ShowAIThinking(bool show)
    {
        if (aiThinkingPanel != null)
            aiThinkingPanel.SetActive(show);

        if (show)
        {
            if (thinkingDotsCoroutine != null) StopCoroutine(thinkingDotsCoroutine);
            thinkingDotsCoroutine = StartCoroutine(AnimateThinkingDots());
        }
        else
        {
            if (thinkingDotsCoroutine != null)
            {
                StopCoroutine(thinkingDotsCoroutine);
                thinkingDotsCoroutine = null;
            }
        }
    }

    private void RefreshTurnTexts()
    {
        bool isPvP = GameSettings.Instance != null && GameSettings.Instance.isPvP;

        for (int i = 0; i < playerTurnTexts.Length; i++)
        {
            if (playerTurnTexts[i] == null) continue;

            bool isActive = i == lastKnownPlayer;
            Color playerColor = GameManager.Instance.playerColors[i];

            playerTurnTexts[i].fontSize = isActive ? activeFontSize : normalFontSize;
            playerTurnTexts[i].fontWeight = isActive ? activeFontWeight : normalFontWeight;

            if (isActive)
            {
                playerTurnTexts[i].color = playerColor;
                playerTurnTexts[i].text = (!isPvP && i == 1)
                    ? L(LocalizationKeys.TurnAI)
                    : string.Format(L(LocalizationKeys.TurnPlayer), i + 1);
            }
            else
            {
                Color inactive = playerColor;
                inactive.a = inactiveColorAlpha;
                playerTurnTexts[i].color = inactive;
                playerTurnTexts[i].text = (!isPvP && i == 1)
                    ? L(LocalizationKeys.LabelAI)
                    : string.Format(L(LocalizationKeys.LabelPlayer), i + 1);
            }
        }

        if (ScoreUI.Instance != null)
            ScoreUI.Instance.UpdatePiecesRemaining();
    }

    private IEnumerator AnimateThinkingDots()
    {
        if (aiThinkingText == null) yield break;

        int dotCount = 0;
        while (true)
        {
            dotCount = (dotCount % 3) + 1;
            aiThinkingText.text = L(LocalizationKeys.AIThinking) + new string('.', dotCount);
            yield return new WaitForSeconds(0.4f);
        }
    }

    private string L(string key) =>
        LocalizationManager.Instance != null ? LocalizationManager.Instance.GetText(key) : key;
}