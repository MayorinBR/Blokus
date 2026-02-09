using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultyButtons : MonoBehaviour
{
    public void SetEasyDifficulty()
    {
        GameSettings.Instance.aiDelay = 2f; // 2 segundos
        LoadGameScene();
    }

    public void SetMediumDifficulty()
    {
        GameSettings.Instance.aiDelay = 1f; // 1 segundo
        LoadGameScene();
    }

    public void SetHardDifficulty()
    {
        GameSettings.Instance.aiDelay = 0.5f; // 0.5 segundos
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene"); // Nome da sua cena do jogo
    }
}