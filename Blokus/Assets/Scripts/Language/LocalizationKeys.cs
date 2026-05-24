/// <summary>
/// Compile-time string constants for every localization key used in the game.
/// Using constants instead of raw strings prevents typos and centralises key management.
/// Add a matching entry to every <see cref="LanguageData"/> asset in the Inspector.
/// </summary>
public static class LocalizationKeys
{
    // ── Turn UI ───────────────────────────────────────────────────────────────

    /// <summary>"Player {0} Turn" — active player banner. Format arg 0 = 1-based player number.</summary>
    public const string TurnPlayer = "game_turn_player";

    /// <summary>"AI Turn" — shown when it is the AI's turn in single-player mode.</summary>
    public const string TurnAI = "game_turn_ai";

    /// <summary>"Player {0}" — inactive player label. Format arg 0 = 1-based player number.</summary>
    public const string LabelPlayer = "game_label_player";

    /// <summary>"AI" — inactive AI label in single-player mode.</summary>
    public const string LabelAI = "game_label_ai";

    /// <summary>"AI is thinking" — base text for the animated thinking indicator. Dots are appended in code.</summary>
    public const string AIThinking = "game_ai_thinking";

    // ── Score UI ──────────────────────────────────────────────────────────────

    /// <summary>"{0} Pieces" — pieces-remaining counter. Format arg 0 = remaining piece count.</summary>
    public const string PiecesRemaining = "game_pieces_remaining";

    // ── Game Over ─────────────────────────────────────────────────────────────

    /// <summary>"Player 1 Wins!"</summary>
    public const string WinnerP1 = "game_winner_p1";

    /// <summary>"Player 2 Wins!"</summary>
    public const string WinnerP2 = "game_winner_p2";

    /// <summary>"It's a Tie!"</summary>
    public const string Tie = "game_tie";

    /// <summary>"Player {0}: {1} pts" — final score line. Arg 0 = 1-based player number, arg 1 = score.</summary>
    public const string FinalScore = "game_final_score";

    // ── Controls HUD ──────────────────────────────────────────────────────────

    /// <summary>"Controls" — header of the controls legend panel.</summary>
    public const string ControlsTitle = "game_controls_title";

    /// <summary>"A / D  —  Rotate piece"</summary>
    public const string ControlsRotate = "game_controls_rotate";

    /// <summary>"W / S  —  Flip piece"</summary>
    public const string ControlsFlip = "game_controls_flip";

    /// <summary>"RMB  —  Rotate 90°"</summary>
    public const string ControlsRMB = "game_controls_rmb";

    /// <summary>"ESC  —  Pause menu"</summary>
    public const string ControlsPause = "game_controls_pause";

    // ── Notifications ─────────────────────────────────────────────────────────

    /// <summary>"Player {0} has no valid moves. Turn skipped." — Format arg 0 = 1-based player number.</summary>
    public const string NotifyPlayerNoMoves = "game_notify_player_no_moves";

    /// <summary>"AI has no available pieces."</summary>
    public const string NotifyAINoPieces = "game_notify_ai_no_pieces";

    /// <summary>"AI found no valid moves. Skipping turn."</summary>
    public const string NotifyAINoMoves = "game_notify_ai_no_moves";

    /// <summary>"No valid moves" — persistent panel shown when a player is stuck.</summary>
    public const string StatusNoMoves = "game_status_no_moves";

    /// <summary>"No pieces remaining" — persistent panel shown when the AI used all pieces.</summary>
    public const string StatusNoPieces = "game_status_no_pieces";
}