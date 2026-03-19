using UnityEngine;
using UnityEngine.Tilemaps;
using UnityTilemap = UnityEngine.Tilemaps.Tilemap;

public class GameController : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private UnityTilemap groundTilemap;
    [SerializeField] private UnityTilemap roadTilemap;

    [Header("Road Tile")]
    [SerializeField] private TileBase roadTile;

    [Header("Build Settings")]
    [SerializeField] private bool buildMode = false;

    void Update()
    {
        ToggleBuildMode();
        HandleMouseInput();
    }

    void ToggleBuildMode()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            buildMode = !buildMode;
            Debug.Log("Build mode: " + buildMode);
        }
    }

    void HandleMouseInput()
    {
        if (!buildMode)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            PlaceRoad();
        }
    }

    void PlaceRoad()
    {
        // Convert mouse position to world position
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;

        // Convert world position to tile cell
        Vector3Int cellPos = roadTilemap.WorldToCell(worldPos);

        // Only place road if ground exists under it
        if (!groundTilemap.HasTile(cellPos))
            return;

        // Prevent placing road twice
        if (roadTilemap.HasTile(cellPos))
            return;

        // Place the road tile
        roadTilemap.SetTile(cellPos, roadTile);
        Debug.Log("Placed road at: " + cellPos);

        // TODO: Add automatic tile updating for corners and intersections
    }
}
