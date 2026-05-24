using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles difficulty and mode selection from the main menu.
/// Each button sets the appropriate <see cref="GameSettings"/> values and loads the game scene.
/// </summary>
public class DifficultyButtons : MonoBehaviour
{
    /// <summary>
    /// Configures the AI for easy difficulty and starts the game.
    /// </summary>
    public void SetEasyDifficulty()
    {
        GameSettings.Instance.isPvP = false;
        GameSettings.Instance.aiDelay = 2f;
        LoadGameScene();
    }

    /// <summary>
    /// Configures the AI for medium difficulty and starts the game.
    /// </summary>
    public void SetMediumDifficulty()
    {
        GameSettings.Instance.isPvP = false;
        GameSettings.Instance.aiDelay = 1f;
        LoadGameScene();
    }

    /// <summary>
    /// Configures the AI for hard difficulty and starts the game.
    /// </summary>
    public void SetHardDifficulty()
    {
        GameSettings.Instance.isPvP = false;
        GameSettings.Instance.aiDelay = 0.5f;
        LoadGameScene();
    }

    /// <summary>
    /// Enables player-vs-player mode (no AI) and starts the game.
    /// </summary>
    public void SetPvPMode()
    {
        GameSettings.Instance.isPvP = true;
        GameSettings.Instance.aiDelay = 0f;
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }
}