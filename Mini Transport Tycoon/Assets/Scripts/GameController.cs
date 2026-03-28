using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityTilemap = UnityEngine.Tilemaps.Tilemap;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    private static readonly Vector3Int InvalidCellPosition = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
    private static readonly Vector3Int[] RoadNeighborOffsets =
    {
        Vector3Int.up,
        Vector3Int.right,
        Vector3Int.down,
        Vector3Int.left
    };
    private static readonly Vector3Int[] GarageFootprintOffsets =
    {
        Vector3Int.zero,
        Vector3Int.right,
        Vector3Int.up,
        Vector3Int.up + Vector3Int.right
    };
    private static readonly Color BuildPreviewColor = new Color(1f, 0.96f, 0.8f, 1f);
    private static readonly List<RaycastResult> UIRaycastResults = new List<RaycastResult>();

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

    [Header("Bus Stop Tiles")]
    [SerializeField] private UnityTilemap busStopTilemap;
    [SerializeField] private TileBase busStopUpTile;
    [SerializeField] private TileBase busStopRightTile;
    [SerializeField] private TileBase busStopDownTile;
    [SerializeField] private TileBase busStopLeftTile;

    [Header("Garage Tiles")]
    [SerializeField] private UnityTilemap garageTilemap;
    [SerializeField] private TileBase garageTile;

    [Header("Vehicle Placement")]
    [SerializeField] private bool placeCar = false;
    [SerializeField] private Car carPrefab;

    [Header("Navigation")]
    [SerializeField] private NavigationMode navigationMode = NavigationMode.Camera;

    [Header("Build Settings")]
    [SerializeField] private Image buildButtonImage;
    [SerializeField] private Color normalColor = new Color32(250, 233, 215, 255); 
    [SerializeField] private Color activeColor = new Color32(183, 181, 179, 255);

    private Vector3Int lastDraggedRoadCell = InvalidCellPosition;
    private readonly List<Vector3Int> previewedBuildCells = new List<Vector3Int>();
    private readonly List<TileFlags> previewedBuildCellFlags = new List<TileFlags>();
    private readonly HashSet<Vector3Int> occupiedGarageCells = new HashSet<Vector3Int>();
    private bool pointerStartedOverUI;

    void Update()
    {
        HandleNavigationModeHotkeys();
        HandleMouseInput();
        UpdateBuildPreview();
    }

    void OnDisable()
    {
        ClearBuildPreview();
    }

    public void ToggleBuildModeUI()
    {
        ToggleNavigationMode(NavigationMode.RoadBuild);
    }

    public void ToggleBusStopBuildModeUI()
    {
        ToggleNavigationMode(NavigationMode.StopBuild);
    }

    public void ToggleGarageBuildModeUI()
    {
        ToggleNavigationMode(NavigationMode.GarageBuild);
    }

    public void ToggleDestroyModeUI()
    {
        ToggleNavigationMode(NavigationMode.Destroy);
    }

    public void SetCameraModeUI()
    {
        SetNavigationMode(NavigationMode.Camera);
    }

    private void UpdateButtonColor()
    {
        if (buildButtonImage != null)
        {
            if (navigationMode == NavigationMode.RoadBuild)
            {
                buildButtonImage.color = activeColor;
            }
            else
            {
                buildButtonImage.color = normalColor;
            }
        }
    }

    void HandleNavigationModeHotkeys()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleNavigationMode(NavigationMode.RoadBuild);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            ToggleNavigationMode(NavigationMode.StopBuild);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleNavigationMode(NavigationMode.GarageBuild);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ToggleNavigationMode(NavigationMode.Destroy);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetNavigationMode(NavigationMode.Camera);
        }
    }

    void HandleMouseInput()
    {
        bool isCameraMode = navigationMode == NavigationMode.Camera;
        bool canPlaceCarInCurrentMode = placeCar && navigationMode == NavigationMode.Camera;

        if (!IsBuildPlacementMode() && !isCameraMode)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            pointerStartedOverUI = IsPointerOverUI();

            if (pointerStartedOverUI)
            {
                lastDraggedRoadCell = InvalidCellPosition;
                return;
            }

            if (navigationMode == NavigationMode.GarageBuild)
            {
                PlaceGarageAtMousePosition();
            }
            else if (navigationMode == NavigationMode.StopBuild)
            {
                PlaceBusStopAtMousePosition();
            }
            else if (navigationMode == NavigationMode.RoadBuild)
            {
                PlaceRoadAtMousePosition();
                lastDraggedRoadCell = GetMouseCellPosition();
            }
            else
            {
                if (LogGarageClickAtMousePosition())
                    return;

                if (canPlaceCarInCurrentMode)
                {
                    PlaceCarAtMousePosition();
                }
            }
            return;
        }

        if (pointerStartedOverUI || IsPointerOverUI())
        {
            if (Input.GetMouseButtonUp(0))
            {
                lastDraggedRoadCell = InvalidCellPosition;
                pointerStartedOverUI = false;
            }

            return;
        }

        if (Input.GetMouseButton(0) && navigationMode == NavigationMode.RoadBuild)
        {
            Vector3Int cellPos = GetMouseCellPosition();

            if (cellPos != lastDraggedRoadCell)
            {
                PlaceRoad(cellPos);
                lastDraggedRoadCell = cellPos;
            }
            return;
        }

        if (Input.GetMouseButtonUp(0))
        {
            lastDraggedRoadCell = InvalidCellPosition;
            pointerStartedOverUI = false;
        }
    }

    void PlaceRoadAtMousePosition()
    {
        PlaceRoad(GetMouseCellPosition());
    }

    Vector3Int GetMouseCellPosition()
    {
        return GetMouseCellPosition(roadTilemap);
    }

    Vector3Int GetMouseCellPosition(UnityTilemap tilemap)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0f;
        return tilemap.WorldToCell(worldPos);
    }

    bool LogGarageClickAtMousePosition()
    {
        if (garageTilemap == null)
            return false;

        Vector3Int clickedCellPos = GetMouseCellPosition(garageTilemap);

        if (!TryGetGarageOriginCell(clickedCellPos, out Vector3Int garageOriginCell))
            return false;

        Debug.Log("Clicked garage at: " + garageOriginCell + " (footprint cell: " + clickedCellPos + ")");
        
        // TODO: Open garage management UI here.
        
        return true;
    }

    bool TryGetGarageOriginCell(Vector3Int clickedCellPos, out Vector3Int garageOriginCell)
    {
        garageOriginCell = InvalidCellPosition;

        if (garageTilemap == null)
            return false;

        for (int i = 0; i < GarageFootprintOffsets.Length; i++)
        {
            Vector3Int candidateOriginCell = clickedCellPos - GarageFootprintOffsets[i];

            if (!garageTilemap.HasTile(candidateOriginCell))
                continue;

            garageOriginCell = candidateOriginCell;
            return true;
        }

        return false;
    }

    void PlaceRoad(Vector3Int cellPos)
    {
        if (!CanBuildRoadAt(cellPos))
            return;

        // Place the road tile
        TileBase defaultRoadTile = GetDefaultRoadTile();
        if (defaultRoadTile == null)
            return;

        roadTilemap.SetTile(cellPos, defaultRoadTile);
        UpdateRoadTiles(cellPos);
        Debug.Log("Placed road at: " + cellPos);
        // Pénz levonása és az erőforrások kezelése itt történik majd
    }

    void ToggleNavigationMode(NavigationMode mode)
    {
        SetNavigationMode(navigationMode == mode ? NavigationMode.Camera : mode);
    }

    void SetNavigationMode(NavigationMode mode)
    {
        navigationMode = mode;
        Debug.Log("Navigation mode: " + navigationMode);
        UpdateButtonColor();
        lastDraggedRoadCell = InvalidCellPosition;
        pointerStartedOverUI = false;
        ClearBuildPreview();
    }

    void UpdateBuildPreview()
    {
        if (!IsBuildPlacementMode() || pointerStartedOverUI || IsPointerOverUI())
        {
            ClearBuildPreview();
            return;
        }

        Vector3Int cellPos = GetMouseCellPosition();
        List<Vector3Int> previewCells = GetPreviewCells(cellPos);
        bool canBuild = navigationMode == NavigationMode.RoadBuild
            ? CanBuildRoadAt(cellPos)
            : navigationMode == NavigationMode.GarageBuild
                ? CanBuildGarageAt(cellPos)
                : navigationMode == NavigationMode.StopBuild && CanBuildBusStopAt(cellPos);

        if (!canBuild)
        {
            ClearBuildPreview();
            return;
        }

        if (HasSamePreviewCells(previewCells))
            return;

        ClearBuildPreview();
        ApplyBuildPreview(previewCells);
    }

    void ClearBuildPreview()
    {
        if (previewedBuildCells.Count == 0)
            return;

        for (int i = 0; i < previewedBuildCells.Count; i++)
        {
            Vector3Int cellPos = previewedBuildCells[i];
            TileFlags tileFlags = previewedBuildCellFlags[i];

            groundTilemap.RemoveTileFlags(cellPos, TileFlags.LockColor);
            groundTilemap.SetColor(cellPos, Color.white);
            groundTilemap.SetTileFlags(cellPos, tileFlags);
        }

        previewedBuildCells.Clear();
        previewedBuildCellFlags.Clear();
    }

    bool CanBuildRoadAt(Vector3Int cellPos)
    {
        return groundTilemap.HasTile(cellPos) && !roadTilemap.HasTile(cellPos);
    }

    bool CanBuildBusStopAt(Vector3Int cellPos)
    {
        return groundTilemap.HasTile(cellPos)
            && !roadTilemap.HasTile(cellPos)
            && busStopTilemap != null
            && !busStopTilemap.HasTile(cellPos)
            && HasAdjacentRoad(cellPos);
    }

    bool CanBuildGarageAt(Vector3Int originCell)
    {
        if (garageTilemap == null)
            return false;

        List<Vector3Int> garageFootprint = GetGarageFootprintCells(originCell);

        for (int i = 0; i < garageFootprint.Count; i++)
        {
            Vector3Int cellPos = garageFootprint[i];

            if (!groundTilemap.HasTile(cellPos))
                return false;

            if (roadTilemap.HasTile(cellPos))
                return false;

            if (busStopTilemap != null && busStopTilemap.HasTile(cellPos))
                return false;

            if (occupiedGarageCells.Contains(cellPos))
                return false;
        }

        return HasAdjacentRoad(garageFootprint);
    }

    bool CanPlaceCarAt(Vector3Int cellPos)
    {
        return roadTilemap != null && roadTilemap.HasTile(cellPos);
    }

    bool HasAdjacentRoad(Vector3Int cellPos)
    {
        return roadTilemap.HasTile(cellPos + Vector3Int.up)
            || roadTilemap.HasTile(cellPos + Vector3Int.right)
            || roadTilemap.HasTile(cellPos + Vector3Int.down)
            || roadTilemap.HasTile(cellPos + Vector3Int.left);
    }

    void PlaceBusStopAtMousePosition()
    {
        PlaceBusStop(GetMouseCellPosition());
    }

    void PlaceGarageAtMousePosition()
    {
        PlaceGarage(GetMouseCellPosition());
    }

    void PlaceCarAtMousePosition()
    {
        PlaceCar(GetMouseCellPosition());
    }

    void PlaceCar(Vector3Int cellPos)
    {
        if (!CanPlaceCarAt(cellPos))
            return;

        if (carPrefab == null)
        {
            Debug.LogWarning("Cannot place car because carPrefab is not assigned.");
            return;
        }

        Vector3 spawnPosition = roadTilemap.GetCellCenterWorld(cellPos);
        spawnPosition.z = carPrefab.transform.position.z;

        Instantiate(carPrefab, spawnPosition, carPrefab.transform.rotation);
        Debug.Log("Placed car at: " + cellPos);
    }

    void PlaceBusStop(Vector3Int cellPos)
    {
        if (!CanBuildBusStopAt(cellPos))
            return;

        TileBase busStopTile = GetBusStopTileForNearestRoad(cellPos);
        if (busStopTile == null)
            return;

        busStopTilemap.SetTile(cellPos, busStopTile);
        Debug.Log("Placed bus stop at: " + cellPos + " (" + navigationMode + ")");
        // Pénz levonása és az erőforrások kezelése itt történik majd
    }

    void PlaceGarage(Vector3Int originCell)
    {
        if (!CanBuildGarageAt(originCell))
            return;

        if (garageTilemap == null || garageTile == null)
        {
            Debug.LogWarning("Cannot place garage because garageTilemap or garageTile is not assigned.");
            return;
        }

        List<Vector3Int> garageFootprint = GetGarageFootprintCells(originCell);

        for (int i = 0; i < garageFootprint.Count; i++)
        {
            occupiedGarageCells.Add(garageFootprint[i]);
        }

        garageTilemap.SetTile(originCell, garageTile);
        Debug.Log("Placed garage at: " + originCell + " (single tile, 2x2 footprint)");
        // Pénz levonása és az erőforrások kezelése itt történik majd
    }

    TileBase GetBusStopTileForNearestRoad(Vector3Int cellPos)
    {
        Vector3Int direction = GetNearestRoadDirection(cellPos);

        if (direction == Vector3Int.up && busStopUpTile != null)
            return busStopUpTile;
        if (direction == Vector3Int.right && busStopRightTile != null)
            return busStopRightTile;
        if (direction == Vector3Int.down && busStopDownTile != null)
            return busStopDownTile;
        if (direction == Vector3Int.left && busStopLeftTile != null)
            return busStopLeftTile;

        return busStopUpTile ?? busStopRightTile ?? busStopDownTile ?? busStopLeftTile;
    }

    Vector3Int GetNearestRoadDirection(Vector3Int cellPos)
    {
        // Direct adjacency has highest priority, then radius search.
        if (roadTilemap.HasTile(cellPos + Vector3Int.up))
            return Vector3Int.up;
        if (roadTilemap.HasTile(cellPos + Vector3Int.right))
            return Vector3Int.right;
        if (roadTilemap.HasTile(cellPos + Vector3Int.down))
            return Vector3Int.down;
        if (roadTilemap.HasTile(cellPos + Vector3Int.left))
            return Vector3Int.left;

        // BFS to nearest road tile
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        queue.Enqueue(cellPos);
        visited.Add(cellPos);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            foreach (Vector3Int offset in RoadNeighborOffsets)
            {
                Vector3Int next = current + offset;
                if (visited.Contains(next))
                    continue;

                if (roadTilemap.HasTile(next))
                {
                    Vector3Int diff = next - cellPos;
                    if (Mathf.Abs(diff.x) >= Mathf.Abs(diff.y))
                        return diff.x > 0 ? Vector3Int.right : Vector3Int.left;
                    return diff.y > 0 ? Vector3Int.up : Vector3Int.down;
                }

                if (groundTilemap.HasTile(next) && !visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        // Default direction if none found
        return Vector3Int.up;
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        UIRaycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, UIRaycastResults);
        return UIRaycastResults.Count > 0;
    }

    List<Vector3Int> GetPreviewCells(Vector3Int originCell)
    {
        return navigationMode == NavigationMode.GarageBuild ? GetGarageFootprintCells(originCell) : new List<Vector3Int> { originCell };
    }

    bool IsBuildPlacementMode()
    {
        return navigationMode == NavigationMode.RoadBuild
            || navigationMode == NavigationMode.StopBuild
            || navigationMode == NavigationMode.GarageBuild;
    }

    List<Vector3Int> GetGarageFootprintCells(Vector3Int originCell)
    {
        List<Vector3Int> garageFootprint = new List<Vector3Int>(GarageFootprintOffsets.Length);

        for (int i = 0; i < GarageFootprintOffsets.Length; i++)
        {
            garageFootprint.Add(originCell + GarageFootprintOffsets[i]);
        }

        return garageFootprint;
    }

    bool HasSamePreviewCells(List<Vector3Int> previewCells)
    {
        if (previewedBuildCells.Count != previewCells.Count)
            return false;

        for (int i = 0; i < previewCells.Count; i++)
        {
            if (previewedBuildCells[i] != previewCells[i])
                return false;
        }

        return true;
    }

    void ApplyBuildPreview(List<Vector3Int> previewCells)
    {
        for (int i = 0; i < previewCells.Count; i++)
        {
            Vector3Int cellPos = previewCells[i];

            previewedBuildCells.Add(cellPos);
            previewedBuildCellFlags.Add(groundTilemap.GetTileFlags(cellPos));
            groundTilemap.RemoveTileFlags(cellPos, TileFlags.LockColor);
            groundTilemap.SetColor(cellPos, BuildPreviewColor);
        }
    }

    bool HasAdjacentRoad(List<Vector3Int> footprintCells)
    {
        HashSet<Vector3Int> footprint = new HashSet<Vector3Int>(footprintCells);

        for (int i = 0; i < footprintCells.Count; i++)
        {
            Vector3Int cellPos = footprintCells[i];

            for (int j = 0; j < RoadNeighborOffsets.Length; j++)
            {
                Vector3Int neighborCell = cellPos + RoadNeighborOffsets[j];

                if (footprint.Contains(neighborCell))
                    continue;

                if (roadTilemap.HasTile(neighborCell))
                    return true;
            }
        }

        return false;
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
