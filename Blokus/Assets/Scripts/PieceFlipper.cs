using UnityEngine;

public class PieceFlipper : MonoBehaviour
{
    private GameObject piece;
    private Vector3[] originalPositions;
    private Quaternion originalRotation;
    private Quaternion defaultRotation = Quaternion.Euler(0, 90, 0);
    private Transform[] blocks;

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
            {
                children.Add(child);
            }
        }

        blocks = children.ToArray();
        originalPositions = new Vector3[blocks.Length];

        for (int i = 0; i < blocks.Length; i++)
        {
            originalPositions[i] = blocks[i].localPosition;
        }
    }

    public void ResetToDefaultRotation()
    {
        piece.transform.rotation = Quaternion.Euler(0, 90, 0);

        // Restaura posições originais
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].localPosition = originalPositions[i];
        }
    }

    public void ResetToOriginal()
    {
        piece.transform.rotation = originalRotation;
        for (int i = 0; i < blocks.Length; i++)
        {
            blocks[i].localPosition = originalPositions[i];
        }
    }

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