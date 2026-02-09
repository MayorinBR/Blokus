using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultyButtons : MonoBehaviour
{
    public void SetEasyDifficulty()
    {
        GameSettings.Instance.aiDelay = 1f; 
        LoadGameScene();
    }

    public void SetMediumDifficulty()
    {
        GameSettings.Instance.aiDelay = 1f; 
        LoadGameScene();
    }

    public void SetHardDifficulty()
    {
        GameSettings.Instance.aiDelay = 1f; 
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene"); // Nome da sua cena do jogo
    }

    public void SetPvPMode()
    {
        GameSettings.Instance.isPvP = true;
        GameSettings.Instance.aiDelay = 0; // Não importa o delay no PvP
        LoadGameScene();
    }
}