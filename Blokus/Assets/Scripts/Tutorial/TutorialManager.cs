using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controls the tutorial scene. Drives a linear sequence of self-contained slides,
/// each with a title, body text, and an optional media element.
/// </remarks>
public class TutorialManager : MonoBehaviour
{
    // ── Step Data ─────────────────────────────────────────────────────────────

    /// <summary>Media type displayed alongside a tutorial step.</summary>
    public enum MediaType
    {
        None,
        Still,
        Video
    }

    /// <summary>
    /// All data that defines one tutorial slide. Assign in the Inspector.
    /// Use localization keys in titleKey and bodyKey for multi-language support,
    /// or paste raw display text directly — both work.
    /// </summary>
    [System.Serializable]
    public class TutorialStep
    {
        [Tooltip("Localization key for the slide title, or raw display text.")]
        public string titleKey;

        [Tooltip("Localization key for the body explanation, or raw display text. "
               + "Supports multiple paragraphs using \\n.")]
        [TextArea(4, 10)]
        public string bodyKey;

        [Tooltip("Type of media shown on this slide.")]
        public MediaType mediaType = MediaType.None;

        [Tooltip("Sprite displayed when MediaType is Still.")]
        public Sprite stillSprite;

        [Tooltip("VideoClip to play when MediaType is Video. "
               + "The video loops automatically.")]
        public VideoClip videoClip;
    }

    // ── Inspector Fields ──────────────────────────────────────────────────────

    [Header("Steps")]
    [Tooltip("Ordered list of tutorial slides. Add one entry per concept to teach.")]
    public List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    [Header("Text UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI stepCounterText;
    public TextMeshProUGUI bodyText;

    [Header("Media UI")]
    [Tooltip("Image component used for Still and AnimatedFrames media types.")]
    public Image stillImage;
    [Tooltip("RawImage used as the render surface for video playback.")]
    public RawImage videoDisplay;
    [Tooltip("VideoPlayer component that drives the video. Can live on any GameObject.")]
    public VideoPlayer videoPlayer;

    [Header("Navigation")]
    public Button prevButton;
    public Button nextButton;
    public Button skipButton;
    public Slider progressBar;

    [Header("Transition")]
    [Tooltip("Full-screen Image used for the cross-fade between slides. "
           + "Set its color to black in the Inspector; the script drives alpha.")]
    public Image transitionOverlay;
    [Tooltip("Duration in seconds of the fade-out and fade-in between slides.")]
    public float transitionDuration = 0.2f;

    [Header("Scene Loading")]
    public string gameSceneName = "GameScene";

    [Header("Player Color Images")]
    [Tooltip("Image tinted with Player 1's color.")]
    public Image player1Image;
    [Tooltip("Image tinted with Player 2's color.")]
    public Image player2Image;

    // ── Runtime State ─────────────────────────────────────────────────────────

    private int currentStep = 0;
    private Coroutine gifCoroutine;
    private Coroutine transitionCoroutine;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        if (prevButton != null) prevButton.onClick.AddListener(OnPrevPressed);
        if (nextButton != null) nextButton.onClick.AddListener(OnNextPressed);
        if (skipButton != null) skipButton.onClick.AddListener(OnSkipPressed);

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += RefreshCurrentStep;

        SetOverlayAlpha(0f);

        if (progressBar != null)
        {
            progressBar.minValue = 0;
            progressBar.maxValue = Mathf.Max(1, tutorialSteps.Count - 1);
            progressBar.interactable = false;
        }

        if (tutorialSteps.Count == 0)
        {
            Debug.LogError("TutorialManager: tutorialSteps list is empty.");
            return;
        }

        ShowStep(0);
        ApplyPlayerColors();
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= RefreshCurrentStep;
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    /// <summary>
    /// Moves to the next slide, or loads the game scene when on the last slide.
    /// </summary>
    private void OnNextPressed()
    {
        if (currentStep < tutorialSteps.Count - 1)
            BeginTransition(currentStep + 1);
        else
            BeginLoadScene();
    }

    /// <summary>
    /// Returns to the previous slide. The button is non-interactable on slide zero.
    /// </summary>
    private void OnPrevPressed()
    {
        if (currentStep > 0)
            BeginTransition(currentStep - 1);
    }

    /// <summary>
    /// Immediately exits the tutorial and loads the game scene.
    /// </summary>
    private void OnSkipPressed()
    {
        BeginLoadScene();
    }

    private void BeginTransition(int targetStep)
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionCoroutine(targetStep));
    }

    private IEnumerator TransitionCoroutine(int targetStep)
    {
        yield return StartCoroutine(FadeOverlay(0f, 1f));
        ShowStep(targetStep);
        yield return StartCoroutine(FadeOverlay(1f, 0f));
    }

    private void BeginLoadScene()
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        yield return StartCoroutine(FadeOverlay(0f, 1f));
        SceneManager.LoadScene(gameSceneName);
    }

    // ── Step Display ──────────────────────────────────────────────────────────

    /// <summary>
    /// Populates all UI elements for the given step index and starts media playback.
    /// </summary>
    /// <param name="index">Zero-based index into tutorialSteps.</param>
    private void ShowStep(int index)
    {
        StopCurrentMedia();

        currentStep = index;
        TutorialStep step = tutorialSteps[index];

        if (titleText != null) titleText.text = Resolve(step.titleKey);
        if (bodyText != null) bodyText.text = Resolve(step.bodyKey);
        if (stepCounterText != null) stepCounterText.text = $"{index + 1} / {tutorialSteps.Count}";
        if (progressBar != null) progressBar.value = index;
        if (prevButton != null) prevButton.interactable = index > 0;

        UpdateNextButtonLabel(index);
        ApplyMedia(step);
    }

    /// <summary>Re-displays the current step with fresh localized text.</summary>
    private void RefreshCurrentStep()
    {
        ShowStep(currentStep);
    }

    private void UpdateNextButtonLabel(int index)
    {
        TextMeshProUGUI label = nextButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label == null) return;

        bool isLastStep = index == tutorialSteps.Count - 1;
        label.text = isLastStep
            ? Resolve("tut_btn_play", "Play!")
            : Resolve("tut_btn_next", "Next");
    }

    // ── Media ─────────────────────────────────────────────────────────────────

    private void ApplyMedia(TutorialStep step)
    {
        if (stillImage != null) stillImage.gameObject.SetActive(false);
        if (videoDisplay != null) videoDisplay.gameObject.SetActive(false);

        switch (step.mediaType)
        {
            case MediaType.Still:
                if (stillImage != null && step.stillSprite != null)
                {
                    stillImage.sprite = step.stillSprite;
                    stillImage.gameObject.SetActive(true);
                }
                break;

            case MediaType.Video:
                if (videoPlayer != null && videoDisplay != null && step.videoClip != null)
                {
                    RenderTexture rt = new RenderTexture(1280, 720, 0);
                    videoPlayer.targetTexture = rt;
                    videoDisplay.texture = rt;
                    videoPlayer.clip = step.videoClip;
                    videoPlayer.isLooping = true;
                    videoPlayer.Play();
                    videoDisplay.gameObject.SetActive(true);
                }
                break;

            case MediaType.None:
            default:
                break;
        }
    }

    private void StopCurrentMedia()
    {
        if (gifCoroutine != null)
        {
            StopCoroutine(gifCoroutine);
            gifCoroutine = null;
        }

        if (videoPlayer != null)
        {
            videoPlayer.Stop();

            if (videoPlayer.targetTexture != null)
            {
                videoPlayer.targetTexture.Release();
                videoPlayer.targetTexture = null;
            }
        }
    }

    private void ApplyPlayerColors()
    {
        if (GameSettings.Instance == null) return;

        if (player1Image != null)
            player1Image.color = GameSettings.Instance.GetPlayerColor(0);

        if (player2Image != null)
            player2Image.color = GameSettings.Instance.GetPlayerColor(1);
    }

    // ── Transition Helpers ────────────────────────────────────────────────────

    private IEnumerator FadeOverlay(float from, float to)
    {
        if (transitionOverlay == null) yield break;

        float elapsed = 0f;
        Color color = transitionOverlay.color;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / transitionDuration));
            transitionOverlay.color = color;
            yield return null;
        }

        color.a = to;
        transitionOverlay.color = color;
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (transitionOverlay == null) return;
        Color c = transitionOverlay.color;
        c.a = alpha;
        transitionOverlay.color = c;
    }

    // ── Localization Helper ───────────────────────────────────────────────────

    /// <summary>
    /// Resolves a localization key to its display string. If the key is missing
    /// or LocalizationManager is unavailable, returns the explicit fallback if
    /// provided, otherwise returns the raw key string (allowing raw text in bodyKey).
    /// </summary>
    /// <param name="key">Localization key or raw display text.</param>
    /// <param name="fallback">Optional explicit fallback string.</param>
    /// <returns>The resolved display string.</returns>
    private string Resolve(string key, string fallback = null)
    {
        if (string.IsNullOrEmpty(key))
            return fallback ?? string.Empty;

        if (LocalizationManager.Instance == null)
            return fallback ?? key;

        string value = LocalizationManager.Instance.GetText(key);
        return value.StartsWith("MISSING") ? (fallback ?? key) : value;
    }
}