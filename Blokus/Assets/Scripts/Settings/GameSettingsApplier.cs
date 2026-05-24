using UnityEngine;

/// <summary>
/// Applies the selected colors from GameSettings to all pieces currently in the scene.
/// </summary>
public class GameSettingsApplier : MonoBehaviour
{
    private void Start()
    {
        ApplyAll();
    }

    /// <summary>
    /// Iterates through all instantiated pieces in the palette and updates their material color.
    /// </summary>
    public void ApplyAll()
    {
        GameSettings gs = GameSettings.Instance;
        if (gs == null || PiecePalette.Instance == null) return;

        UpdateCollectionColors(PiecePalette.Instance.player1Pieces.Values, gs.GetPlayerColor(0));
        UpdateCollectionColors(PiecePalette.Instance.player2Pieces.Values, gs.GetPlayerColor(1));
    }

    private void UpdateCollectionColors(System.Collections.Generic.IEnumerable<GameObject> pieces, Color color)
    {
        foreach (GameObject piece in pieces)
        {
            if (piece == null) continue;
            foreach (Renderer r in piece.GetComponentsInChildren<Renderer>())
            {
                r.material.color = color;
            }
        }
    }
}