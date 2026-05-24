using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the main menu (title screen). Handles navigation to the game, tutorial,
/// and settings panel, applies the saved background sprite, and quits the application.
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "GameScene";
    public string tutorialSceneName = "TutorialScene";

    [Header("UI References")]
    public Button playAiButton;
    public Button playVSButton;
    public Button tutorialButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Player Color Images")]
    public Image p1MenuImage;
    public Image p2MenuImage;
    public Image rightBgImage;
    public Image leftBgImage;

    [Header("Settings Panel")]
    public GameObject settingsPanel;

    [Header("Transition")]
    public Image transitionOverlay;
    public float transitionDuration = 0.35f;

    private void Start()
    {
        if (transitionOverlay != null)
        {
            Color c = transitionOverlay.color;
            c.a = 0f;
            transitionOverlay.color = c;
        }

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        playAiButton.onClick.AddListener(OnPlayAIPressed);
        playVSButton.onClick.AddListener(OnPlayVSPressed);
        tutorialButton.onClick.AddListener(OnTutorialPressed);
        settingsButton.onClick.AddListener(OnSettingsPressed);
        quitButton.onClick.AddListener(OnQuitPressed);

        RefreshPlayerColors();
    }

    /// <summary>
    /// Updates the menu images with the colors defined in GameSettings.
    /// </summary>
    public void RefreshPlayerColors()
    {
        if (GameSettings.Instance == null) return;

        if (p1MenuImage != null)
            p1MenuImage.color = GameSettings.Instance.GetPlayerColor(0);

        if (p2MenuImage != null)
            p2MenuImage.color = GameSettings.Instance.GetPlayerColor(1);

        if (leftBgImage != null)
            leftBgImage.color = GameSettings.Instance.GetPlayerColor(0);

        if (rightBgImage != null)
            rightBgImage.color = GameSettings.Instance.GetPlayerColor(1);
    }

    private void OnPlayAIPressed()
    {
        GameSettings.Instance.isPvP = true;
        GameSettings.Instance.aiDelay = 1f;
        StartCoroutine(FadeAndLoad(gameSceneName));
    }

    private void OnPlayVSPressed()
    {
        GameSettings.Instance.isPvP = false;
        StartCoroutine(FadeAndLoad(gameSceneName));
    }

    private void OnTutorialPressed()
    {
        StartCoroutine(FadeAndLoad(tutorialSceneName));
    }

    private void OnSettingsPressed()
    {
        if (settingsPanel == null) return;
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void OnQuitPressed()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private System.Collections.IEnumerator FadeAndLoad(string sceneName)
    {
        SetButtonsInteractable(false);

        if (transitionOverlay != null)
        {
            float elapsed = 0f;
            Color c = transitionOverlay.color;

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Clamp01(elapsed / transitionDuration);
                transitionOverlay.color = c;
                yield return null;
            }

            c.a = 1f;
            transitionOverlay.color = c;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void SetButtonsInteractable(bool interactable)
    {
        playAiButton.interactable = interactable;
        playVSButton.interactable = interactable;
        tutorialButton.interactable = interactable;
        settingsButton.interactable = interactable;
        quitButton.interactable = interactable;
    }
}