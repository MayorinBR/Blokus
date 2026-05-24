using UnityEngine;

/// <summary>
/// Configures the main camera for an orthographic top-down view that frames the board
/// with a consistent margin on all sides. Runs once on scene load.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Tooltip("Extra world-space units added around the board on each side.")]
    [SerializeField] private float boardMargin = 1f;

    private void Start()
    {
        float boardSize = BoardManager.BoardSize;

        Camera.main.orthographic = true;
        Camera.main.transform.rotation = Quaternion.Euler(90, 0, 0);

        float totalHeight = boardSize + boardMargin * 2;
        Camera.main.orthographicSize = totalHeight / 2;

        Camera.main.transform.position = new Vector3(0f, 15f, 0f);
    }
}