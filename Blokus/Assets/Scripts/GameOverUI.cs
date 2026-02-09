using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    public Button restartButton;
    public Button difficultySelectButton;

    void Start()
    {
        // Configurar os listeners dos botões
        restartButton.onClick.AddListener(RestartGame);
        difficultySelectButton.onClick.AddListener(GoToDifficultySelection);
    }

    private void RestartGame()
    {
        // Reiniciar o jogo com as mesmas configurações
        GameManager.Instance.ResetGame();
        ScoreManager.Instance.InitializeScores();
        this.gameObject.SetActive(false);
    }

    private void GoToDifficultySelection()
    {
        Destroy(GameManager.Instance.gameObject); // Limpa instâncias persistentes
        Destroy(GameSettings.Instance.gameObject); // Limpa configurações
        SceneManager.LoadScene("DifficultySelectionScene");
    }
}