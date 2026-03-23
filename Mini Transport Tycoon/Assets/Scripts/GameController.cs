using UnityEngine;
using UnityEngine.Tilemaps;
using UnityTilemap = UnityEngine.Tilemaps.Tilemap;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class GameController : MonoBehaviour
{
    private static readonly Vector3Int[] RoadNeighborOffsets =
    {
        Vector3Int.up,
        Vector3Int.right,
        Vector3Int.down,
        Vector3Int.left
    };

    [Header("Tilemaps")]
    [SerializeField] private UnityTilemap groundTilemap;
    [SerializeField] private UnityTilemap roadTilemap;

    [Header("Straight Road Tiles")]
    [SerializeField] private TileBase roadStraightUpDownTile;
    [SerializeField] private TileBase roadStraightLeftRightTile;

    [Header("Turning Road Tiles")]
    [SerializeField] private TileBase roadTurnUpRightTile;
    [SerializeField] private TileBase roadTurnRightDownTile;
    [SerializeField] private TileBase roadTurnDownLeftTile;
    [SerializeField] private TileBase roadTurnLeftUpTile;

    [Header("3-Way Intersection Tiles")]
    [SerializeField] private TileBase roadTJunctionUpRightDownTile;
    [SerializeField] private TileBase roadTJunctionRightDownLeftTile;
    [SerializeField] private TileBase roadTJunctionDownLeftUpTile;
    [SerializeField] private TileBase roadTJunctionLeftUpRightTile;

    [Header("4-Way Intersection Tile")]
    [SerializeField] private TileBase roadIntersectionTile;

    [Header("Build Settings")]
    [SerializeField] private bool buildMode = false;
    [SerializeField] private Image buildButtonImage;
    [SerializeField] private Color normalColor = new Color32(250, 233, 215, 255); 
    [SerializeField] private Color activeColor = new Color32(183, 181, 179, 255);

    void Update()
    {
        ToggleBuildMode();
        HandleMouseInput();
    }

    public void ToggleBuildModeUI()
    {
        buildMode = !buildMode;
        Debug.Log("Build mode: " + buildMode);
        
        UpdateButtonColor();
    }

    private void UpdateButtonColor()
    {
        if (buildButtonImage != null)
        {
            if (buildMode)
            {
                buildButtonImage.color = activeColor;
            }
            else
            {
                buildButtonImage.color = normalColor;
            }
        }
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
        TileBase defaultRoadTile = GetDefaultRoadTile();
        if (defaultRoadTile == null)
            return;

        roadTilemap.SetTile(cellPos, defaultRoadTile);
        UpdateRoadTiles(cellPos);
        Debug.Log("Placed road at: " + cellPos);
    }

    void UpdateRoadTiles(Vector3Int centerCell)
    {
        UpdateRoadTile(centerCell);

        for (int i = 0; i < RoadNeighborOffsets.Length; i++)
        {
            Vector3Int neighborCell = centerCell + RoadNeighborOffsets[i];

            if (roadTilemap.HasTile(neighborCell))
            {
                UpdateRoadTile(neighborCell);
            }
        }
    }

    void UpdateRoadTile(Vector3Int cellPos)
    {
        if (!roadTilemap.HasTile(cellPos))
            return;

        TileBase roadTile = GetRoadTileForMask(GetRoadNeighborMask(cellPos));
        if (roadTile == null)
            return;

        roadTilemap.SetTile(cellPos, roadTile);
        roadTilemap.SetTransformMatrix(cellPos, Matrix4x4.identity);
    }

    int GetRoadNeighborMask(Vector3Int cellPos)
    {
        int mask = 0;

        if (roadTilemap.HasTile(cellPos + Vector3Int.up))
            mask |= 1;

        if (roadTilemap.HasTile(cellPos + Vector3Int.right))
            mask |= 2;

        if (roadTilemap.HasTile(cellPos + Vector3Int.down))
            mask |= 4;

        if (roadTilemap.HasTile(cellPos + Vector3Int.left))
            mask |= 8;

        return mask;
    }

    TileBase GetRoadTileForMask(int mask)
    {
        switch (mask)
        {
            case 0:
            case 1:
            case 4:
            case 5:
                return roadStraightUpDownTile != null ? roadStraightUpDownTile : GetDefaultRoadTile();

            case 2:
            case 8:
            case 10:
                return roadStraightLeftRightTile != null ? roadStraightLeftRightTile : GetDefaultRoadTile();

            case 3:
                return roadTurnUpRightTile != null ? roadTurnUpRightTile : GetDefaultRoadTile();

            case 6:
                return roadTurnRightDownTile != null ? roadTurnRightDownTile : GetDefaultRoadTile();

            case 12:
                return roadTurnDownLeftTile != null ? roadTurnDownLeftTile : GetDefaultRoadTile();

            case 9:
                return roadTurnLeftUpTile != null ? roadTurnLeftUpTile : GetDefaultRoadTile();

            case 7:
                return roadTJunctionUpRightDownTile != null ? roadTJunctionUpRightDownTile : GetDefaultRoadTile();

            case 14:
                return roadTJunctionRightDownLeftTile != null ? roadTJunctionRightDownLeftTile : GetDefaultRoadTile();

            case 13:
                return roadTJunctionDownLeftUpTile != null ? roadTJunctionDownLeftUpTile : GetDefaultRoadTile();

            case 11:
                return roadTJunctionLeftUpRightTile != null ? roadTJunctionLeftUpRightTile : GetDefaultRoadTile();

            case 15:
                return roadIntersectionTile != null ? roadIntersectionTile : GetDefaultRoadTile();

            default:
                return GetDefaultRoadTile();
        }
    }

    TileBase GetDefaultRoadTile()
    {
        return roadStraightUpDownTile
            ?? roadStraightLeftRightTile
            ?? roadTurnUpRightTile
            ?? roadTurnRightDownTile
            ?? roadTurnDownLeftTile
            ?? roadTurnLeftUpTile
            ?? roadTJunctionUpRightDownTile
            ?? roadTJunctionRightDownLeftTile
            ?? roadTJunctionDownLeftUpTile
            ?? roadTJunctionLeftUpRightTile
            ?? roadIntersectionTile;
    }
}
