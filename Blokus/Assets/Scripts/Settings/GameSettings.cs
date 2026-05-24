using UnityEngine;

/// <summary>
/// Persistent singleton that holds game configuration, including player color palettes.
/// </summary>
public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance;

    [Header("Gameplay")]
    public float aiDelay = 1f;
    public bool isPvP = false;

    [Header("Colors")]
    public Color[] pieceColorOptions = new Color[] { Color.red, Color.blue, 
        Color.green, Color.yellow, Color.blueViolet, Color.pink, Color.cyan, Color.orange };
    public int player1ColorIndex = 0;
    public int player2ColorIndex = 1;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Returns the color assigned to a specific player based on current settings.
    /// </summary>
    /// <param name="playerIndex">0 for Player 1, 1 for Player 2.</param>
    /// <returns>The selected Color object.</returns>
    public Color GetPlayerColor(int playerIndex)
    {
        if (pieceColorOptions == null || pieceColorOptions.Length == 0) return Color.white;

        int index = (playerIndex == 0) ? player1ColorIndex : player2ColorIndex;
        int clampedIndex = Mathf.Clamp(index, 0, pieceColorOptions.Length - 1);
        return pieceColorOptions[clampedIndex];
    }
}