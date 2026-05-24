using UnityEngine;

/// <summary>
/// Handles mouse-driven drag-and-drop interaction for a single piece.
/// Attached to each piece GameObject by <see cref="PieceManager"/>.
/// Supports rotation (A / D / RMB) and flipping (W / S) while dragging,
/// and validates placement on mouse release via <see cref="GameManager"/>.
/// </summary>
public class PieceDragger : MonoBehaviour
{
    private GameObject selectedPiece;
    private Vector3 offset;
    private float zCoord;
    private Vector3 originalPosition;
    private bool wasDragging = false;
    private bool isBeingDestroyed = false;

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        if (Input.GetMouseButtonDown(0))
            TrySelectPiece();

        if (selectedPiece != null)
        {
            if (wasDragging && selectedPiece.GetComponent<PieceDragger>() != null)
                HandlePieceTransformations();

            if (Input.GetMouseButton(0))
            {
                Vector3 newPos = GetMouseWorldPos() + offset;
                selectedPiece.transform.position = new Vector3(newPos.x, 0, newPos.z);
                wasDragging = true;

                if (BoardManager.Instance.enableHighlight)
                    BoardManager.Instance.HighlightValidPositions(selectedPiece);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (wasDragging)
                    HandlePiecePlacement();
            }
        }
    }

    private void HandlePieceTransformations()
    {
        PieceFlipper flipper = selectedPiece.GetComponent<PieceFlipper>();

        if (Input.GetKeyDown(KeyCode.A))
        {
            selectedPiece.transform.Rotate(0, -90, 0);
            UpdateVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            selectedPiece.transform.Rotate(0, 90, 0);
            UpdateVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            flipper.FlipZ();
            UpdateVisuals();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            flipper.FlipX();
            UpdateVisuals();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            selectedPiece.transform.Rotate(0, 90, 0);
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        Vector3 newPos = GetMouseWorldPos() + offset;
        selectedPiece.transform.position = new Vector3(newPos.x, 0, newPos.z);

        if (BoardManager.Instance.enableHighlight)
            BoardManager.Instance.HighlightValidPositions(selectedPiece);
    }

    private void TrySelectPiece()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        Transform rootPiece = hit.transform.root;
        PieceDragger dragger = rootPiece.GetComponent<PieceDragger>();
        if (dragger == null || dragger != this) return;

        string pieceName = rootPiece.name.Split('_')[0];
        PieceManager.PieceType type = (PieceManager.PieceType)System.Enum.Parse(
            typeof(PieceManager.PieceType), pieceName);

        bool isPvP = GameSettings.Instance != null && GameSettings.Instance.isPvP;

        if (!isPvP && rootPiece.name.Contains("_Player1"))
        {
            Debug.Log("Cannot drag AI pieces in single-player mode.");
            return;
        }

        if ((GameManager.Instance.currentPlayer == 0 && PiecePalette.Instance.player1Pieces.ContainsKey(type)) ||
            (GameManager.Instance.currentPlayer == 1 && PiecePalette.Instance.player2Pieces.ContainsKey(type)))
        {
            selectedPiece = rootPiece.gameObject;
            zCoord = Camera.main.WorldToScreenPoint(selectedPiece.transform.position).z;
            offset = selectedPiece.transform.position - GetMouseWorldPos();
            originalPosition = selectedPiece.transform.position;
            wasDragging = false;

            PiecePalette.Instance.PieceSelected(selectedPiece);
        }
    }

    private void HandlePiecePlacement()
    {
        if (isBeingDestroyed) return;

        bool isValid = GameManager.Instance.IsValidMove(selectedPiece);

        if (isValid && GameManager.Instance.PlacePiece(selectedPiece, selectedPiece.transform.position))
        {
            CleanUp();
            return;
        }

        ReturnPieceToOriginalPosition();
    }

    /// <summary>
    /// Clears the drag selection and resets the board highlights.
    /// Call this after successful placement to release the piece reference.
    /// </summary>
    public void CleanUp()
    {
        if (isBeingDestroyed) return;

        if (selectedPiece != null)
        {
            BoardManager.Instance.ClearHighlights();
            selectedPiece = null;
        }
        wasDragging = false;
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return Camera.main.ScreenToWorldPoint(mousePoint);
    }

    private void ReturnPieceToOriginalPosition()
    {
        if (selectedPiece != null)
        {
            selectedPiece.transform.position = originalPosition;

            PieceFlipper flipper = selectedPiece.GetComponent<PieceFlipper>();
            if (flipper != null)
                flipper.ResetToDefaultRotation();
            else
                selectedPiece.transform.rotation = Quaternion.Euler(0, 90, 0);

            selectedPiece.transform.localScale = Vector3.one * PiecePalette.Instance.pieceScale;
        }

        CleanUp();
    }
}