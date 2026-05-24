using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Renders as an icon in the HUD. While the pointer hovers over the icon,
/// a tooltip panel with the full controls legend is shown; it hides on exit.
/// Text is resolved through <see cref="LocalizationManager"/> and refreshes
/// automatically on language change.
/// </summary>
public class ControlsHUD : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("The panel that appears on hover. Start it inactive in the Inspector.")]
    [SerializeField] private GameObject tooltipPanel;

    [Tooltip("TextMeshProUGUI inside the tooltip panel.")]
    [SerializeField] private TextMeshProUGUI controlsText;

    private void Start()
    {
        tooltipPanel.SetActive(false);

        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged += RefreshText;

        RefreshText();
    }

    private void OnDestroy()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.OnLanguageChanged -= RefreshText;
    }

    /// <summary>Shows the tooltip panel when the pointer enters the icon.</summary>
    public void OnPointerEnter(PointerEventData _) => tooltipPanel.SetActive(true);

    /// <summary>Hides the tooltip panel when the pointer leaves the icon.</summary>
    public void OnPointerExit(PointerEventData _) => tooltipPanel.SetActive(false);

    private void RefreshText()
    {
        if (controlsText == null) return;

        controlsText.text =
            $"<b>{L(LocalizationKeys.ControlsTitle)}</b>\n" +
            $"{L(LocalizationKeys.ControlsRotate)}\n" +
            $"{L(LocalizationKeys.ControlsFlip)}\n" +
            $"{L(LocalizationKeys.ControlsRMB)}\n" +
            $"{L(LocalizationKeys.ControlsPause)}";
    }

    private string L(string key) =>
        LocalizationManager.Instance != null
            ? LocalizationManager.Instance.GetText(key)
            : key;
}