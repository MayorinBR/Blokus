using UnityEngine;

/// <summary>
/// Handles mirroring (flipping) of a piece along its local X and Z axes.
/// Works by directly modifying child block positions rather than rotating the parent,
/// allowing flip state to be preserved independently of rotation.
/// Attached to each piece by <see cref="PieceManager"/>.
/// </summary>
public class PieceFlipper : MonoBehaviour
{
    private GameObject piece;
    private Vector3[] originalPositions;
    private Quaternion originalRotation;
    private Transform[] blocks;

    /// <summary>
    /// Initialises the flipper with the target piece. Must be called once immediately after adding this component.
    /// Caches original block positions and resets to the default rotation.
    /// </summary>
    /// <param name="pieceObject">The piece GameObject this flipper controls.</param>
    public void Initialize(GameObject pieceObject)
    {
        piece = pieceObject;
        CacheOriginalPositions();
        ResetToDefaultRotation();
    }

    private void CacheOriginalPositions()
    {
        var children = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in piece.transform)
        {
            if (!child.name.Contains("Collider"))
                children.Add(child);
        }

        blocks = children.ToArray();
        originalPositions = new Vector3[blocks.Length];

        for (int i = 0; i < blocks.Length; i++)
            originalPositions[i] = blocks[i].localPosition;
    }

    /// <summary>
    /// Resets the piece to its default rotation (Y = 90°) and restores original block positions.
    /// Called when a piece is selected from the palette or returned after an invalid drop.
    /// </summary>
    public void ResetToDefaultRotation()
    {
        piece.transform.rotation = Quaternion.Euler(0, 90, 0);

        for (int i = 0; i < blocks.Length; i++)
            blocks[i].localPosition = originalPositions[i];
    }

    /// <summary>
    /// Restores the piece to the rotation it had when <see cref="Initialize"/> was called.
    /// </summary>
    public void ResetToOriginal()
    {
        piece.transform.rotation = originalRotation;
        for (int i = 0; i < blocks.Length; i++)
            blocks[i].localPosition = originalPositions[i];
    }

    /// <summary>
    /// Mirrors all blocks along the local X axis (horizontal flip).
    /// </summary>
    public void FlipX()
    {
        foreach (Transform block in blocks)
        {
            block.localPosition = new Vector3(
                -block.localPosition.x,
                block.localPosition.y,
                block.localPosition.z
            );
        }
    }

    /// <summary>
    /// Mirrors all blocks along the local Z axis (vertical flip).
    /// </summary>
    public void FlipZ()
    {
        foreach (Transform block in blocks)
        {
            block.localPosition = new Vector3(
                block.localPosition.x,
                block.localPosition.y,
                -block.localPosition.z
            );
        }
    }
}