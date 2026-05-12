using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityTilemap = UnityEngine.Tilemaps.Tilemap;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace MiniTransportTycoon
{
    [System.Serializable]
    public struct MaterialPrice
    {
        public Materials material;
        [Tooltip("Price per unit when sold to a warehouse.")]
        public float price;
    }

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
    private static readonly Color BuildPreviewColor = new Color(0.6f, 0.6f, 0.6f, 1f);
    private static readonly Color DestroyPreviewColor = new Color(0.8f, 0.3f, 0.3f, 1f);
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

    [Header("Warehouse Tiles")]
    [SerializeField] private UnityTilemap warehouseTilemap;

    [Header("Facility Tiles")]
    [SerializeField] private UnityTilemap ironFactoryTilemap;
    [SerializeField] private UnityTilemap paperFactoryTilemap;
    [SerializeField] private UnityTilemap steelFactoryTilemap;
    [SerializeField] private UnityTilemap woodFactoryTilemap;
    [SerializeField] private UnityTilemap coalFactoryTilemap;

    [Header("Houses Tiles")]
    [SerializeField] private UnityTilemap housesTilemap;

    [Header("Tree Tiles")]
    [SerializeField] private UnityTilemap treeTilemap;

    private UnityTilemap[] allTilemaps;

    [Header("Vehicle Placement")]
    [SerializeField] private bool placeBus= false;
    [SerializeField] private Bus busPrefab;
    [SerializeField] private int busPlacementCapacity = 50;
    [SerializeField] private float busPlacementSpeed = 1f;
    [SerializeField] private int busPlacementPrice = 100;
    [SerializeField] private bool placeTruck = false;
    [SerializeField] private int truckPlacementPrice = 100;
    [SerializeField] private Truck truckPrefab;
    [SerializeField] private Materials truckPlacementMaterial = Materials.Wood;
    [SerializeField] private int truckPlacementCapacity = 500;
    [SerializeField] private float truckPlacementSpeed = 1f;

    [Header("Material Prices")]
    [SerializeField] private List<MaterialPrice> materialPrices = new List<MaterialPrice>
    {
        new MaterialPrice { material = Materials.Wood, price = 0.2f },
        new MaterialPrice { material = Materials.Paper, price = 0.4f },
        new MaterialPrice { material = Materials.Iron, price = 0.5f },
        new MaterialPrice { material = Materials.Coal, price = 0.3f },
        new MaterialPrice { material = Materials.Steel, price = 0.8f }
    };
    private Dictionary<Materials, float> _materialPriceDict;

    [Header("Warehouse Runtime")]
    [SerializeField] private Warehouse warehousePrefab;
    [SerializeField] private Transform warehouseRuntimeRoot;

    [Header("Facility Runtime")]
    [SerializeField] private IronFactory ironFoundryPrefab;
    [SerializeField] private PaperFactory paperFactoryPrefab;
    [SerializeField] private SteelFactory steelFoundryPrefab;
    [SerializeField] private WoodFactory woodFactoryPrefab;
    [SerializeField] private CoalFactory copperRefineryPrefab;
    [SerializeField] private Transform facilityRuntimeRoot;

    [Header("Navigation")]
    [SerializeField] private NavigationMode navigationMode = NavigationMode.Camera;

    [Header("Button Settings")]
    [SerializeField] private Image buildButtonImage;
    [SerializeField] private Image busStopButtonImage;
    [SerializeField] private Image garageButtonImage;
    [SerializeField] private Image destroyButtonImage;
    [SerializeField] private Image placeBusButtonImage; 
    [SerializeField] private Color normalColor = new Color32(250, 233, 215, 255); 
    [SerializeField] private Color activeColor = new Color32(183, 181, 179, 255);

    [Header("Build Costs")]
    [SerializeField] private int roadPlacementCost = 50;
    [SerializeField] private int roadCostOnTreeMultiplier = 2;
    [SerializeField] private int busStopPlacementCost = 100;
    [SerializeField] private int garagePlacementCost = 300;

    private Vector3Int lastDraggedRoadCell = InvalidCellPosition;
    private readonly List<Vector3Int> previewedBuildCells = new List<Vector3Int>();
    private readonly HashSet<Vector3Int> occupiedGarageCells = new HashSet<Vector3Int>();
    private readonly HashSet<Vector3Int> occupiedWarehouseCells = new HashSet<Vector3Int>();
    private readonly HashSet<Vector3Int> occupiedFacilityCells = new HashSet<Vector3Int>();
    private readonly Dictionary<Vector3Int, Warehouse> warehousesByOrigin = new Dictionary<Vector3Int, Warehouse>();
    private readonly Dictionary<Vector3Int, Facility> facilitiesByOrigin = new Dictionary<Vector3Int, Facility>();
    private readonly List<Vector3Int> pendingCarStopSelections = new List<Vector3Int>(2);
    private readonly List<Vector3Int> pendingTruckStopSelections = new List<Vector3Int>(2);
    private readonly List<Vector3Int> roadCoordinates = new List<Vector3Int>();
    private readonly HashSet<Vector3Int> roadCoordinateLookup = new HashSet<Vector3Int>();
    private int nextWarehouseId = 1;
    private int nextFacilityId = 1;
    private bool pointerStartedOverUI;
    private Color lastPreviewColor = Color.white;

    private GameData gameData;

    void Awake()
    {
        gameData = GameData.Instance;

        // Populate price dictionary
        _materialPriceDict = new Dictionary<Materials, float>();
        if (materialPrices != null)
        {
            foreach (var mp in materialPrices)
            {
                _materialPriceDict[mp.material] = mp.price;
            }
        }

        allTilemaps = new Tilemap[]
        {
            groundTilemap,
            roadTilemap,
            busStopTilemap,
            garageTilemap,
            warehouseTilemap,
            ironFactoryTilemap,
            paperFactoryTilemap,
            steelFactoryTilemap,
            woodFactoryTilemap,
            coalFactoryTilemap,
            housesTilemap
        };
        RefreshWarehouseFootprintOccupancy();
        RefreshFacilityFootprintOccupancy();
        InitializeWarehousesFromTilemap();
        InitializeFacilitiesFromTilemaps();
        RefreshRoadCoordinates();
    }

    void Start()
    {
        if (gameData != null)
        {
            gameData.OnHourChanged += UpdateMarketPrices;
        }
    }

    void Update()
    {
        HandleNavigationModeHotkeys();
        SyncCarPlacementSelectionState();
        HandleMouseInput();
        UpdateBuildPreview();
    }

    void OnDisable()
    {
        ClearBuildPreview();
        ClearPendingCarStopSelections();
    }

    void OnDestroy()
    {
        if (gameData != null)
        {
            gameData.OnHourChanged -= UpdateMarketPrices;
        }
    }

    private void UpdateMarketPrices()
    {
        if (_materialPriceDict == null) return;

        List<Materials> keys = new List<Materials>(_materialPriceDict.Keys);
        foreach (Materials mat in keys)
        {
            float currentPrice = _materialPriceDict[mat];
            // Véletlenszerű elmozdulás -0.1 és +0.1 között (tőzsdeszerű ingadozás)
            float priceChange = UnityEngine.Random.Range(-0.1f, 0.1f);
            float newPrice = Mathf.Clamp(currentPrice + priceChange, 0.1f, 2.0f);
            
            // Kerekítés 2 tizedesjegyre a pontosság kedvéért
            _materialPriceDict[mat] = Mathf.Round(newPrice * 100f) / 100f;
        }
    }

    public float GetMaterialPrice(Materials material)
    {
        if (_materialPriceDict != null && _materialPriceDict.TryGetValue(material, out float price))
        {
            return price;
        }

        Debug.LogWarning($"Price for material '{material}' not found. Returning 0.");
        return 0f;
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

    public void TogglePlaceBusModeUI()
    {
        placeBus = !placeBus;
        if (placeBus)
        {
            placeTruck = false;
            ClearPendingTruckStopSelections();
        }

        if (placeBus)
        {
            SetNavigationMode(NavigationMode.Camera);
        }
        UpdateButtonColor();
    }

    public void StartBusPlacement(int capacity)
    {
        StartBusPlacement(capacity, busPlacementSpeed, busPlacementPrice);
    }

    public void StartBusPlacement(int capacity, float speed, int price)
    {
        busPlacementCapacity = Mathf.Max(0, capacity);
        busPlacementSpeed = Mathf.Max(0f, speed);
        placeBus = true;
        placeTruck = false;
        busPlacementPrice = Mathf.Max(0, price);
        ClearPendingTruckStopSelections();
        SetNavigationMode(NavigationMode.Camera);
        
        UpdateButtonColor();
        
        
    }

    public void ButtonStartBusPlacementSmall()
    {
        StartBusPlacement(15, 2.5f,300);
    }

    public void ButtonStartBusPlacementMedium()
    {
        StartBusPlacement(30, 2f, 500);
    }

    public void ButtonStartBusPlacementLarge()
    {
        StartBusPlacement(50, 1.5f, 700);
    }

    public void TogglePlaceTruckModeUI()
    {
        placeTruck = !placeTruck;
        if (placeTruck)
        {
            placeBus = false;
            ClearPendingCarStopSelections();
            SetNavigationMode(NavigationMode.Camera);
        }

        UpdateButtonColor();
    }

    

    public void StartTruckPlacement(Materials material, int capacity, float speed ,int price)
    {
        truckPlacementMaterial = material;
        truckPlacementCapacity = Mathf.Max(0, capacity);
        truckPlacementSpeed = Mathf.Max(0f, speed);
        truckPlacementPrice = Mathf.Max(0, price);
        placeTruck = true;
        placeBus = false;
        ClearPendingCarStopSelections();
        SetNavigationMode(NavigationMode.Camera);
        UpdateButtonColor();
    }

    public void ButtonStartTruckPlacementWoodSmall()
    {
        StartTruckPlacement(Materials.Wood, 200, 2.5f, 300);
    }

    public void ButtonStartTruckPlacementWoodMedium()
    {
        StartTruckPlacement(Materials.Wood, 350, 2f, 500);
    }

    public void ButtonStartTruckPlacementWoodLarge()
    {
        StartTruckPlacement(Materials.Wood, 500, 1.5f, 700);
    }

    public void ButtonStartTruckPlacementPaperSmall()
    {
        StartTruckPlacement(Materials.Paper, 200, 2.5f, 300);
        
    }

    public void ButtonStartTruckPlacementPaperMedium()
    {
        StartTruckPlacement(Materials.Paper, 350, 2f, 500);
    }

    public void ButtonStartTruckPlacementPaperLarge()
    {
        StartTruckPlacement(Materials.Paper, 500, 1.5f, 700);
    }

    public void ButtonStartTruckPlacementIronSmall()
    {
        StartTruckPlacement(Materials.Iron, 200, 2.5f, 300);
    }

    public void ButtonStartTruckPlacementIronMedium()
    {
        StartTruckPlacement(Materials.Iron, 350, 2f, 500);
    }

    public void ButtonStartTruckPlacementIronLarge()
    {
        StartTruckPlacement(Materials.Iron, 500, 1.5f, 700);
    }

    public void ButtonStartTruckPlacementCoalSmall()
    {
        StartTruckPlacement(Materials.Coal, 200, 2.5f, 300);
    }

    public void ButtonStartTruckPlacementCoalMedium()
    {
        StartTruckPlacement(Materials.Coal, 350, 2f, 500);
    }

    public void ButtonStartTruckPlacementCoalLarge()
    {
        StartTruckPlacement(Materials.Coal, 500, 1.5f, 700);
    }

    public void ButtonStartTruckPlacementSteelSmall()
    {
        StartTruckPlacement(Materials.Steel, 200, 2.5f, 300);
    }

    public void ButtonStartTruckPlacementSteelMedium()
    {
        StartTruckPlacement(Materials.Steel, 350, 2f, 500);
    }

    public void ButtonStartTruckPlacementSteelLarge()
    {
        StartTruckPlacement(Materials.Steel, 500, 1.5f, 700);
    }
    

    bool TrySpendMoneyFromGameData(int amount, string purchaseContext)
    {
        if (gameData == null)
        {
            Debug.LogError("Cannot spend money because GameData instance is missing.");
            return false;
        }

        return gameData.TrySpendMoney(amount, purchaseContext);
    }

    void ReportUserError(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (gameData != null)
        {
            gameData.ReportError(message);
            return;
        }

        Debug.LogError(message);
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
        if (busStopButtonImage != null)
        {
            if (navigationMode == NavigationMode.StopBuild)
            {
                busStopButtonImage.color = activeColor;
            }
            else
            {
                busStopButtonImage.color = normalColor;
            }
        }
        if (garageButtonImage != null)
        {
            if (navigationMode == NavigationMode.GarageBuild)
            {
                garageButtonImage.color = activeColor;
            }
            else
            {
                garageButtonImage.color = normalColor;
            }
        }
        if (destroyButtonImage != null)
        {
            if (navigationMode == NavigationMode.Destroy)
            {
                destroyButtonImage.color = activeColor;
            }
            else
            {
                destroyButtonImage.color = normalColor;
            }
        }
        if (placeBusButtonImage != null)
        {
            if (navigationMode == NavigationMode.Camera && placeBus)
            {
                placeBusButtonImage.color = activeColor;
            }
            else
            {
                placeBusButtonImage.color = normalColor;
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
        bool canPlaceCarInCurrentMode = placeBus&& navigationMode == NavigationMode.Camera;
        bool canPlaceTruckInCurrentMode = placeTruck && navigationMode == NavigationMode.Camera;

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
                SetNavigationMode(NavigationMode.Camera);
            }
            else if (navigationMode == NavigationMode.StopBuild)
            {
                PlaceBusStopAtMousePosition();
                SetNavigationMode(NavigationMode.Camera);
            }
            else if (navigationMode == NavigationMode.RoadBuild)
            {
                PlaceRoadAtMousePosition();
                lastDraggedRoadCell = GetMouseCellPosition();
            }

            else if (navigationMode == NavigationMode.Destroy)
            {
                DestroyTileAtMousePosition();
                lastDraggedRoadCell = GetMouseCellPosition();
            }
            else
            {
                if (canPlaceCarInCurrentMode)
                {
                    PlaceCarAtMousePosition();
                }
                else if (canPlaceTruckInCurrentMode)
                {
                    PlaceTruckAtMousePosition();
                }
                else if (LogFacilityClickAtMousePosition())
                {
                    return;
                }
                else if (LogWarehouseClickAtMousePosition())
                {
                    return;
                }
                else if (LogGarageClickAtMousePosition())
                {
                    return;
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

        if (Input.GetMouseButton(0))
        {
            if (navigationMode == NavigationMode.RoadBuild)
            {
                Vector3Int cellPos = GetMouseCellPosition();

                if (cellPos != lastDraggedRoadCell)
                {
                    PlaceRoad(cellPos);
                    lastDraggedRoadCell = cellPos;
                }
                return;
            }
            else if (navigationMode == NavigationMode.Destroy)
            {
                Vector3Int cellPos = GetMouseCellPosition();

                if (cellPos != lastDraggedRoadCell)
                {
                    DestroyTile(cellPos);
                    lastDraggedRoadCell = cellPos;
                }
                return;
            }
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

    bool LogWarehouseClickAtMousePosition()
    {
        if (warehouseTilemap == null)
            return false;

        Vector3Int clickedCellPos = GetMouseCellPosition(warehouseTilemap);

        if (!TryGetWarehouseAtCell(clickedCellPos, out Vector3Int warehouseOriginCell, out Warehouse warehouse))
            return false;

        if (warehouse == null)
        {
            Debug.LogWarning("Clicked warehouse at: " + warehouseOriginCell + " but no Warehouse script instance is registered.");
            return true;
        }

        Debug.Log("Clicked warehouse id " + warehouse.Id + " at: " + warehouseOriginCell + " (footprint cell: " + clickedCellPos + ")");
        return true;
    }

    bool LogFacilityClickAtMousePosition()
    {
        if (!TryGetFacilityAtMousePosition(out Vector3Int clickedCellPos, out Vector3Int facilityOriginCell, out Facility facility))
            return false;

        if (facility == null)
        {
            Debug.LogWarning("Clicked facility at: " + facilityOriginCell + " but no Facility script instance is registered.");
            return true;
        }

        Debug.Log("Clicked facility " + facility.GetType().Name + " id " + facility.Id + " at: " + facilityOriginCell + " (footprint cell: " + clickedCellPos + ")");
        return true;
    }

    bool TryGetFacilityAtMousePosition(out Vector3Int clickedCellPos, out Vector3Int facilityOriginCell, out Facility facility)
    {
        clickedCellPos = InvalidCellPosition;
        facilityOriginCell = InvalidCellPosition;
        facility = null;

        if (TryGetFacilityAtTilemapCell(ironFactoryTilemap, out clickedCellPos, out facilityOriginCell, out facility))
            return true;

        if (TryGetFacilityAtTilemapCell(paperFactoryTilemap, out clickedCellPos, out facilityOriginCell, out facility))
            return true;

        if (TryGetFacilityAtTilemapCell(steelFactoryTilemap, out clickedCellPos, out facilityOriginCell, out facility))
            return true;

        if (TryGetFacilityAtTilemapCell(woodFactoryTilemap, out clickedCellPos, out facilityOriginCell, out facility))
            return true;

        if (TryGetFacilityAtTilemapCell(coalFactoryTilemap, out clickedCellPos, out facilityOriginCell, out facility))
            return true;

        return false;
    }

    bool TryGetFacilityAtTilemapCell(UnityTilemap tilemap, out Vector3Int clickedCellPos, out Vector3Int facilityOriginCell, out Facility facility)
    {
        clickedCellPos = InvalidCellPosition;
        facilityOriginCell = InvalidCellPosition;
        facility = null;

        if (tilemap == null)
            return false;

        clickedCellPos = GetMouseCellPosition(tilemap);

        if (!TryGetFacilityOriginCell(clickedCellPos, tilemap, out facilityOriginCell))
            return false;

        facilitiesByOrigin.TryGetValue(facilityOriginCell, out facility);
        return true;
    }

    bool TryGetWarehouseAtCell(Vector3Int clickedCellPos, out Vector3Int warehouseOriginCell, out Warehouse warehouse)
    {
        warehouse = null;

        if (!TryGetWarehouseOriginCell(clickedCellPos, out warehouseOriginCell))
            return false;

        warehousesByOrigin.TryGetValue(warehouseOriginCell, out warehouse);
        return true;
    }

    public bool TryGetWarehouseAtRoutePoint(Vector3Int routePoint, out Warehouse warehouse)
    {
        warehouse = null;

        if (!warehousesByOrigin.TryGetValue(routePoint, out Warehouse routeWarehouse) || routeWarehouse == null)
            return false;

        warehouse = routeWarehouse;
        return true;
    }

    public bool TryGetFacilityAtRoutePoint(Vector3Int routePoint, out Facility facility)
    {
        facility = null;

        if (!facilitiesByOrigin.TryGetValue(routePoint, out Facility routeFacility) || routeFacility == null)
            return false;

        facility = routeFacility;
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

    bool TryGetWarehouseOriginCell(Vector3Int clickedCellPos, out Vector3Int warehouseOriginCell)
    {
        warehouseOriginCell = InvalidCellPosition;

        if (warehouseTilemap == null)
            return false;

        for (int i = 0; i < GarageFootprintOffsets.Length; i++)
        {
            Vector3Int candidateOriginCell = clickedCellPos - GarageFootprintOffsets[i];

            if (!warehouseTilemap.HasTile(candidateOriginCell))
                continue;

            warehouseOriginCell = candidateOriginCell;
            return true;
        }

        return false;
    }

    bool TryGetFacilityOriginCell(Vector3Int clickedCellPos, UnityTilemap tilemap, out Vector3Int facilityOriginCell)
    {
        facilityOriginCell = InvalidCellPosition;

        if (tilemap == null)
            return false;

        for (int i = 0; i < GarageFootprintOffsets.Length; i++)
        {
            Vector3Int candidateOriginCell = clickedCellPos - GarageFootprintOffsets[i];

            if (!tilemap.HasTile(candidateOriginCell))
                continue;

            facilityOriginCell = candidateOriginCell;
            return true;
        }

        return false;
    }

    void PlaceRoad(Vector3Int cellPos)
    {
        if (!TryValidateRoadPlacement(cellPos, out string validationError))
        {
            ReportUserError(validationError);
            return;
        }

        int roadCost = GetRoadPlacementCost(cellPos);
        if (!TrySpendMoneyFromGameData(roadCost, "build a road"))
            return;

        // Place the road tile
        TileBase defaultRoadTile = GetDefaultRoadTile();
        if (defaultRoadTile == null)
        {
            ReportUserError("Cannot build road because no road tile is configured.");
            return;
        }

        if (HasTreeAt(cellPos))
        {
            treeTilemap.SetTile(cellPos, null);
        }

        roadTilemap.SetTile(cellPos, defaultRoadTile);
        RegisterRoadCoordinate(cellPos);
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
        ClearPendingCarStopSelections();
    
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
        bool canBuild = false;
        Color previewColor = BuildPreviewColor;

        if (navigationMode == NavigationMode.RoadBuild)
        {
            canBuild = CanBuildRoadAt(cellPos);
        }
        else if (navigationMode == NavigationMode.GarageBuild)
        {
            canBuild = CanBuildGarageAt(cellPos);
        }
        else if (navigationMode == NavigationMode.StopBuild)
        {
            canBuild = CanBuildBusStopAt(cellPos);
        }
        else if (navigationMode == NavigationMode.Destroy)
        {
            canBuild = IsRoadCoordinate(cellPos) 
                    || (busStopTilemap != null && busStopTilemap.HasTile(cellPos)) 
                    || (garageTilemap != null && TryGetGarageOriginCell(cellPos, out _));
            previewColor = DestroyPreviewColor;
        }

        if (!canBuild)
        {
            ClearBuildPreview();
            return;
        }

        if (HasSamePreviewCells(previewCells) && lastPreviewColor == previewColor)
            return;

        ClearBuildPreview();
        ApplyBuildPreview(previewCells, previewColor);
        lastPreviewColor = previewColor;
    }

    void ClearBuildPreview()
    {
        if (previewedBuildCells.Count == 0)
            return;

        for (int i = 0; i < previewedBuildCells.Count; i++)
        {
            Vector3Int cellPos = previewedBuildCells[i];

            RestoreCellColor(groundTilemap, cellPos);
            RestoreCellColor(roadTilemap, cellPos);
            RestoreCellColor(busStopTilemap, cellPos);
            RestoreCellColor(garageTilemap, cellPos);
        }

        previewedBuildCells.Clear();
    }

    void RestoreCellColor(UnityTilemap tilemap, Vector3Int cellPos)
    {
        if (tilemap != null && tilemap.HasTile(cellPos))
        {
            tilemap.SetColor(cellPos, Color.white);
            tilemap.SetTileFlags(cellPos, tilemap.GetTileFlags(cellPos) | TileFlags.LockColor);
        }
    }

    bool CanBuildRoadAt(Vector3Int cellPos)
    {
        return TryValidateRoadPlacement(cellPos, out _);
    }

    int GetRoadPlacementCost(Vector3Int cellPos)
    {
        int normalizedBaseCost = Mathf.Max(0, roadPlacementCost);

        if (!HasTreeAt(cellPos))
            return normalizedBaseCost;

        int normalizedMultiplier = Mathf.Max(1, roadCostOnTreeMultiplier);
        return normalizedBaseCost * normalizedMultiplier;
    }

    bool HasTreeAt(Vector3Int cellPos)
    {
        return treeTilemap != null && treeTilemap.HasTile(cellPos);
    }

    bool TryValidateRoadPlacement(Vector3Int cellPos, out string errorMessage)
    {
        errorMessage = string.Empty;

        // Must be a valid ground (grass) tile
        if (!groundTilemap.HasTile(cellPos))
        {
            errorMessage = "You can only build roads on valid ground tiles.";
            return false;
        }

        // Cannot already have a road
        if (IsRoadCoordinate(cellPos))
        {
            errorMessage = "A road already exists at this location.";
            return false;
        }

        // Cannot build under any of the 4 garage tiles
        if (occupiedGarageCells.Contains(cellPos))
        {
            errorMessage = "You cannot build a road on a garage tile.";
            return false;
        }

        // Cannot build under warehouse footprint tiles.
        if (occupiedWarehouseCells.Contains(cellPos))
        {
            errorMessage = "You cannot build a road on a warehouse tile.";
            return false;
        }

        // Cannot build under facility footprint tiles.
        if (occupiedFacilityCells.Contains(cellPos))
        {
            errorMessage = "You cannot build a road on a factory tile.";
            return false;
        }

        // Check all other tilemaps - none should have a tile at this location
        for (int i = 0; i < allTilemaps.Length; i++)
        {
            UnityTilemap tilemap = allTilemaps[i];
            
            // Skip null tilemaps
            if (tilemap == null)
                continue;

            // Skip ground tilemap (we already checked it)
            if (tilemap == groundTilemap)
                continue;

            // If ANY other tilemap has a tile here → block
            if (tilemap.HasTile(cellPos))
            {
                errorMessage = "This tile is already occupied.";
                return false;
            }
        }

        // Road must be adjacent to an existing road
        if (!HasAdjacentRoad(cellPos))
        {
            errorMessage = "New roads must connect to an existing road.";
            return false;
        }

        return true;
    }

    bool CanBuildBusStopAt(Vector3Int cellPos)
    {
        return TryValidateBusStopPlacement(cellPos, out _);
    }

    bool TryValidateBusStopPlacement(Vector3Int cellPos, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (!groundTilemap.HasTile(cellPos))
        {
            errorMessage = "You can only place bus stops on valid ground tiles.";
            return false;
        }

        if (IsRoadCoordinate(cellPos))
        {
            errorMessage = "You cannot place a bus stop on top of a road.";
            return false;
        }

        if (busStopTilemap == null)
        {
            errorMessage = "Bus stop tilemap is not configured.";
            return false;
        }

        if (busStopTilemap.HasTile(cellPos))
        {
            errorMessage = "A bus stop already exists here.";
            return false;
        }

        if (occupiedWarehouseCells.Contains(cellPos) || occupiedFacilityCells.Contains(cellPos))
        {
            errorMessage = "You cannot place a bus stop on an occupied building tile.";
            return false;
        }

        if (!HasAdjacentRoad(cellPos))
        {
            errorMessage = "Bus stops must be placed next to a road.";
            return false;
        }

        return true;
    }

    bool CanBuildGarageAt(Vector3Int originCell)
    {
        return TryValidateGaragePlacement(originCell, out _);
    }

    bool TryValidateGaragePlacement(Vector3Int originCell, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (garageTilemap == null)
        {
            errorMessage = "Garage tilemap is not configured.";
            return false;
        }

        List<Vector3Int> garageFootprint = GetGarageFootprintCells(originCell);

        for (int i = 0; i < garageFootprint.Count; i++)
        {
            Vector3Int cellPos = garageFootprint[i];

            if (!groundTilemap.HasTile(cellPos))
            {
                errorMessage = "You can only place garages on valid ground tiles.";
                return false;
            }

            if (IsRoadCoordinate(cellPos))
            {
                errorMessage = "You cannot place a garage on a road tile.";
                return false;
            }

            if (busStopTilemap != null && busStopTilemap.HasTile(cellPos))
            {
                errorMessage = "You cannot place a garage on a bus stop.";
                return false;
            }

            if (occupiedGarageCells.Contains(cellPos))
            {
                errorMessage = "Another garage already occupies this area.";
                return false;
            }

            if (occupiedWarehouseCells.Contains(cellPos))
            {
                errorMessage = "You cannot place a garage on a warehouse tile.";
                return false;
            }

            if (occupiedFacilityCells.Contains(cellPos))
            {
                errorMessage = "You cannot place a garage on a factory tile.";
                return false;
            }
        }

        if (!HasAdjacentRoad(garageFootprint))
        {
            errorMessage = "Garages must connect to an adjacent road.";
            return false;
        }

        return true;
    }

    bool CanPlaceCarAt(Vector3Int cellPos)
    {
        return IsRoadCoordinate(cellPos);
    }

    bool HasAdjacentRoad(Vector3Int cellPos)
    {
        return IsRoadCoordinate(cellPos + Vector3Int.up)
            || IsRoadCoordinate(cellPos + Vector3Int.right)
            || IsRoadCoordinate(cellPos + Vector3Int.down)
            || IsRoadCoordinate(cellPos + Vector3Int.left);
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
        if (!TryGetRoutePointAtMousePosition(out Vector3Int routePointCell))
        {
            ReportUserError("Click a bus stop or garage to select a bus route point.");
            return;
        }

        SelectCarStop(routePointCell);
    }

    void PlaceTruckAtMousePosition()
    {
        if (!TryGetTruckRoutePointAtMousePosition(out Vector3Int routePointCell))
        {
            ReportUserError("Click a garage, warehouse, or factory to select a truck route point.");
            return;
        }

        SelectTruckStop(routePointCell, truckPlacementMaterial);
    }

    void PlaceCar(Vector3Int cellPos)
    {
        if (!CanPlaceCarAt(cellPos))
        {
            ReportUserError("You can only place a bus on a road tile.");
            return;
        }

        if (busPrefab == null)
        {
            ReportUserError("Cannot place bus because the bus prefab is missing.");
            return;
        }

        int carCost = Mathf.Max(0, busPrefab.Cost);
        if (!TrySpendMoneyFromGameData(carCost, "buy this bus"))
            return;

        Vector3 spawnPosition = roadTilemap.GetCellCenterWorld(cellPos);
        spawnPosition.z = busPrefab.transform.position.z;

        Bus carInstance = Instantiate(busPrefab, spawnPosition, busPrefab.transform.rotation);
        carInstance.SetRoadTilemap(roadTilemap);
        carInstance.SetGarageTilemap(garageTilemap);
        
        Debug.Log("Placed car at: " + cellPos);
    }

    void SelectCarStop(Vector3Int stopCellPos)
    {
        SelectCarStop(stopCellPos, busPlacementCapacity, busPlacementSpeed);
    }

    void SelectCarStop(Vector3Int stopCellPos, int capacity, float speed)
    {
        Vector3Int normalizedRoutePoint = NormalizeRoutePoint(stopCellPos);

        if (normalizedRoutePoint == InvalidCellPosition)
        {
            ReportUserError("Click a bus stop or garage to select a bus route point.");
            return;
        }

        if (pendingCarStopSelections.Count >= 2 && normalizedRoutePoint == pendingCarStopSelections[0])
        {
            bool placedBus = TryPlaceCarFromSelectedStops(new List<Vector3Int>(pendingCarStopSelections), capacity, speed);

            if (!placedBus)
            {
                ReportUserError("Could not create a bus loop route from the selected points.");
            }

            ClearPendingCarStopSelections();
            return;
        }

        if (pendingCarStopSelections.Contains(normalizedRoutePoint))
        {
            ReportUserError("Route point already selected. Click the first point again to finalize the loop.");
            return;
        }

        pendingCarStopSelections.Add(normalizedRoutePoint);
        Debug.Log("Selected route point " + pendingCarStopSelections.Count + " at: " + normalizedRoutePoint
            + ". Click the first selected point to finalize route.");
    }

    bool TryPlaceCarFromSelectedStops(List<Vector3Int> selectedRoutePoints)
    {
        return TryPlaceCarFromSelectedStops(selectedRoutePoints, busPlacementCapacity, busPlacementSpeed);
    }

    bool TryPlaceCarFromSelectedStops(List<Vector3Int> selectedRoutePoints, int capacity, float speed)
    {
        if (busPrefab == null)
        {
            ReportUserError("Cannot place bus because the bus prefab is missing.");
            return false;
        }

        if (selectedRoutePoints == null || selectedRoutePoints.Count < 2)
        {
            ReportUserError("Cannot create a bus route with fewer than two route points.");
            return false;
        }

        List<Vector3Int> normalizedRoutePoints = new List<Vector3Int>(selectedRoutePoints.Count);
        List<Vector3Int> routeRoadCells = new List<Vector3Int>(selectedRoutePoints.Count);

        for (int i = 0; i < selectedRoutePoints.Count; i++)
        {
            Vector3Int normalizedRoutePoint = NormalizeRoutePoint(selectedRoutePoints[i]);

            if (normalizedRoutePoint == InvalidCellPosition)
            {
                ReportUserError("Cannot place bus because one route point is not a bus stop or garage.");
                return false;
            }

            if (!TryGetClosestRoadTile(normalizedRoutePoint, out Vector3Int closestRoadCell))
            {
                ReportUserError("Cannot place bus because no road was found near route point: " + normalizedRoutePoint);
                return false;
            }

            normalizedRoutePoints.Add(normalizedRoutePoint);
            routeRoadCells.Add(closestRoadCell);
        }

        List<List<Vector3Int>> loopRouteLegs = new List<List<Vector3Int>>(routeRoadCells.Count);

        for (int i = 0; i < routeRoadCells.Count; i++)
        {
            Vector3Int startRoadCell = routeRoadCells[i];
            Vector3Int endRoadCell = routeRoadCells[(i + 1) % routeRoadCells.Count];
            List<Vector3Int> legPath = FindRoadPath(startRoadCell, endRoadCell);

            if (legPath == null)
            {
                ReportUserError("Cannot place bus because route points are not connected by roads.");
                return false;
            }

            loopRouteLegs.Add(legPath);
        }

        Vector3Int spawnRoadCell = routeRoadCells[0];

        if (roadTilemap == null)
        {
            ReportUserError("Cannot place bus because road tilemap is missing.");
            return false;
        }

        if (!TrySpendMoneyFromGameData(busPlacementPrice, "buy this bus"))
            return false;

        Vector3 spawnPosition = roadTilemap.GetCellCenterWorld(spawnRoadCell);
        spawnPosition.z = busPrefab.transform.position.z;

        Bus carInstance = Instantiate(busPrefab, spawnPosition, busPrefab.transform.rotation);
        carInstance.SetRoadTilemap(roadTilemap);
        carInstance.SetGarageTilemap(garageTilemap);
        carInstance.SetRoadCoordinates(roadCoordinates);
        carInstance.SetStopRoute(normalizedRoutePoints);
        carInstance.SetMaxCarryingAmount(capacity);
        carInstance.SetSpeed(speed);
        carInstance.SetLoopRoute(loopRouteLegs);
        carInstance.SetCost(busPlacementPrice);
        
        
        Debug.Log("Placed car at: " + spawnRoadCell + " with capacity " + Mathf.Max(0, capacity) + ", speed " + Mathf.Max(0f, speed) + " and loop route points: " + string.Join(" -> ", normalizedRoutePoints));
        placeBus = false;
        return true;
    }

    void SelectTruckStop(Vector3Int stopCellPos, Materials material)
    {
        SelectTruckStop(stopCellPos, material, truckPlacementCapacity, truckPlacementSpeed);
    }

    void SelectTruckStop(Vector3Int stopCellPos, Materials material, int capacity, float speed)
    {
        Vector3Int normalizedRoutePoint = NormalizeTruckRoutePoint(stopCellPos);

        if (normalizedRoutePoint == InvalidCellPosition)
        {
            ReportUserError("Click a garage, warehouse, or factory to select a truck route point.");
            return;
        }

        if (pendingTruckStopSelections.Count >= 2 && normalizedRoutePoint == pendingTruckStopSelections[0])
        {
            bool placedTruck = TryPlaceTruckFromSelectedStops(new List<Vector3Int>(pendingTruckStopSelections), material, capacity, speed);

            if (!placedTruck)
            {
                ReportUserError("Could not create a truck loop route from the selected points.");
            }

            ClearPendingTruckStopSelections();
            return;
        }

        if (pendingTruckStopSelections.Contains(normalizedRoutePoint))
        {
            ReportUserError("Truck route point already selected. Click the first point again to finalize the loop.");
            return;
        }

        pendingTruckStopSelections.Add(normalizedRoutePoint);
        Debug.Log("Selected truck route point " + pendingTruckStopSelections.Count + " at: " + normalizedRoutePoint
            + ". Click the first selected point to finalize route.");
    }

    public bool TryPlaceTruckFromSelectedStops(List<Vector3Int> selectedRoutePoints, Materials material)
    {
        return TryPlaceTruckFromSelectedStops(selectedRoutePoints, material, truckPlacementCapacity, truckPlacementSpeed);
    }

    public bool TryPlaceTruckFromSelectedStops(List<Vector3Int> selectedRoutePoints, Materials material, int capacity)
    {
        return TryPlaceTruckFromSelectedStops(selectedRoutePoints, material, capacity, truckPlacementSpeed);
    }

    public bool TryPlaceTruckFromSelectedStops(List<Vector3Int> selectedRoutePoints, Materials material, int capacity, float speed)
    {
        if (truckPrefab == null)
        {
            ReportUserError("Cannot place truck because the truck prefab is missing.");
            return false;
        }

        if (selectedRoutePoints == null || selectedRoutePoints.Count < 2)
        {
            ReportUserError("Cannot create a truck route with fewer than two route points.");
            return false;
        }

        List<Vector3Int> normalizedRoutePoints = new List<Vector3Int>(selectedRoutePoints.Count);
        List<Vector3Int> routeRoadCells = new List<Vector3Int>(selectedRoutePoints.Count);

        for (int i = 0; i < selectedRoutePoints.Count; i++)
        {
            Vector3Int normalizedRoutePoint = NormalizeTruckRoutePoint(selectedRoutePoints[i]);

            if (normalizedRoutePoint == InvalidCellPosition)
            {
                ReportUserError("Cannot place truck because one route point is not a garage, warehouse, or factory.");
                return false;
            }

            if (!TryGetClosestRoadTile(normalizedRoutePoint, out Vector3Int closestRoadCell))
            {
                ReportUserError("Cannot place truck because no road was found near route point: " + normalizedRoutePoint);
                return false;
            }

            normalizedRoutePoints.Add(normalizedRoutePoint);
            routeRoadCells.Add(closestRoadCell);
        }

        if (!HasValidFactoryForTruckMaterial(normalizedRoutePoints, material))
        {
            return false;
        }

        List<List<Vector3Int>> loopRouteLegs = new List<List<Vector3Int>>(routeRoadCells.Count);

        for (int i = 0; i < routeRoadCells.Count; i++)
        {
            Vector3Int startRoadCell = routeRoadCells[i];
            Vector3Int endRoadCell = routeRoadCells[(i + 1) % routeRoadCells.Count];
            List<Vector3Int> legPath = FindRoadPath(startRoadCell, endRoadCell);

            if (legPath == null)
            {
                ReportUserError("Cannot place truck because route points are not connected by roads.");
                return false;
            }

            loopRouteLegs.Add(legPath);
        }

        if (roadTilemap == null)
        {
            ReportUserError("Cannot place truck because road tilemap is missing.");
            return false;
        }

        if (!TrySpendMoneyFromGameData(truckPlacementPrice, "buy this truck"))
            return false;

        Vector3Int spawnRoadCell = routeRoadCells[0];
        Vector3 spawnPosition = roadTilemap.GetCellCenterWorld(spawnRoadCell);
        spawnPosition.z = truckPrefab.transform.position.z;

        Truck truckInstance = Instantiate(truckPrefab, spawnPosition, truckPrefab.transform.rotation);
        truckInstance.SetRoadTilemap(roadTilemap);
        truckInstance.SetGarageTilemap(garageTilemap);
        truckInstance.SetRoadCoordinates(roadCoordinates);
        truckInstance.SetStopRoute(normalizedRoutePoints);
        truckInstance.SetMaterialType(material);
        truckInstance.SetMaxCarryingAmount(capacity);
        truckInstance.SetSpeed(speed);
        truckInstance.SetLoopRoute(loopRouteLegs);
        truckInstance.SetCost(truckPlacementPrice);
        
        Debug.Log("Placed truck at: " + spawnRoadCell + " for material " + material + " with capacity " + Mathf.Max(0, capacity) + ", speed " + Mathf.Max(0f, speed) + " and loop route points: " + string.Join(" -> ", normalizedRoutePoints));
        placeTruck = false;
        return true;
    }

    bool HasValidFactoryForTruckMaterial(List<Vector3Int> routePoints, Materials material)
    {
        List<Facility> facilitiesOnRoute = new List<Facility>();
        HashSet<Facility> uniqueFacilities = new HashSet<Facility>();

        for (int i = 0; i < routePoints.Count; i++)
        {
            if (!facilitiesByOrigin.TryGetValue(routePoints[i], out Facility facility) || facility == null)
                continue;

            if (!uniqueFacilities.Add(facility))
                continue;

            facilitiesOnRoute.Add(facility);
        }

        if (facilitiesOnRoute.Count == 0)
        {
            ReportUserError("Cannot place truck because the route must include at least one factory stop.");
            return false;
        }

        if (facilitiesOnRoute.Count > 2)
        {
            ReportUserError("Cannot place truck because the route can include at most two factory stops.");
            return false;
        }

        if (facilitiesOnRoute.Count == 1)
        {
            Facility onlyFactory = facilitiesOnRoute[0];

            if (onlyFactory.ProducedMaterialType != material)
            {
                ReportUserError("Cannot place truck because the selected factory does not produce " + material + ".");
                return false;
            }

            return true;
        }

        Facility firstFactory = facilitiesOnRoute[0];
        Facility secondFactory = facilitiesOnRoute[1];

        bool firstToSecondCompatible = firstFactory.ProducedMaterialType == material && secondFactory.RequiresInputMaterial(material);
        bool secondToFirstCompatible = secondFactory.ProducedMaterialType == material && firstFactory.RequiresInputMaterial(material);

        if (!firstToSecondCompatible && !secondToFirstCompatible)
        {
            ReportUserError("Cannot place truck because selected factories are not compatible for material " + material + ".");
            return false;
        }

        return true;
    }

    bool TryGetRoutePointAtMousePosition(out Vector3Int routePointCell)
    {
        routePointCell = InvalidCellPosition;

        if (busStopTilemap != null)
        {
            Vector3Int busStopCellPos = GetMouseCellPosition(busStopTilemap);

            if (busStopTilemap.HasTile(busStopCellPos))
            {
                routePointCell = busStopCellPos;
                return true;
            }
        }

        if (garageTilemap != null)
        {
            Vector3Int garageCellPos = GetMouseCellPosition(garageTilemap);

            if (TryGetGarageOriginCell(garageCellPos, out Vector3Int garageOriginCell))
            {
                routePointCell = garageOriginCell;
                return true;
            }
        }

        return false;
    }

    bool TryGetTruckRoutePointAtMousePosition(out Vector3Int routePointCell)
    {
        routePointCell = InvalidCellPosition;

        if (garageTilemap != null)
        {
            Vector3Int garageCellPos = GetMouseCellPosition(garageTilemap);

            if (TryGetGarageOriginCell(garageCellPos, out Vector3Int garageOriginCell))
            {
                routePointCell = garageOriginCell;
                return true;
            }
        }

        if (warehouseTilemap != null)
        {
            Vector3Int warehouseCellPos = GetMouseCellPosition(warehouseTilemap);

            if (TryGetWarehouseOriginCell(warehouseCellPos, out Vector3Int warehouseOriginCell))
            {
                routePointCell = warehouseOriginCell;
                return true;
            }
        }

        if (TryGetFacilityAtMousePosition(out _, out Vector3Int facilityOriginCell, out _))
        {
            routePointCell = facilityOriginCell;
            return true;
        }

        return false;
    }

    Vector3Int NormalizeRoutePoint(Vector3Int selectedCellPos)
    {
        if (busStopTilemap != null && busStopTilemap.HasTile(selectedCellPos))
            return selectedCellPos;

        if (garageTilemap != null && TryGetGarageOriginCell(selectedCellPos, out Vector3Int garageOriginCell))
            return garageOriginCell;

        return InvalidCellPosition;
    }

    Vector3Int NormalizeTruckRoutePoint(Vector3Int selectedCellPos)
    {
        if (garageTilemap != null && TryGetGarageOriginCell(selectedCellPos, out Vector3Int garageOriginCell))
            return garageOriginCell;

        if (warehouseTilemap != null && TryGetWarehouseOriginCell(selectedCellPos, out Vector3Int warehouseOriginCell))
            return warehouseOriginCell;

        if (TryGetFacilityOriginFromAnyTilemap(selectedCellPos, out Vector3Int facilityOriginCell))
            return facilityOriginCell;

        return InvalidCellPosition;
    }

    bool TryGetFacilityOriginFromAnyTilemap(Vector3Int selectedCellPos, out Vector3Int facilityOriginCell)
    {
        facilityOriginCell = InvalidCellPosition;

        if (TryGetFacilityOriginCell(selectedCellPos, ironFactoryTilemap, out facilityOriginCell))
            return true;

        if (TryGetFacilityOriginCell(selectedCellPos, paperFactoryTilemap, out facilityOriginCell))
            return true;

        if (TryGetFacilityOriginCell(selectedCellPos, steelFactoryTilemap, out facilityOriginCell))
            return true;

        if (TryGetFacilityOriginCell(selectedCellPos, woodFactoryTilemap, out facilityOriginCell))
            return true;

        if (TryGetFacilityOriginCell(selectedCellPos, coalFactoryTilemap, out facilityOriginCell))
            return true;

        return false;
    }

    void PlaceBusStop(Vector3Int cellPos)
    {
        if (!TryValidateBusStopPlacement(cellPos, out string validationError))
        {
            ReportUserError(validationError);
            return;
        }

        if (!TrySpendMoneyFromGameData(busStopPlacementCost, "build a bus stop"))
            return;

        TileBase busStopTile = GetBusStopTileForNearestRoad(cellPos);
        if (busStopTile == null)
        {
            ReportUserError("Cannot place bus stop because no bus stop tiles are configured.");
            return;
        }

        busStopTilemap.SetTile(cellPos, busStopTile);
        Debug.Log("Placed bus stop at: " + cellPos + " (" + navigationMode + ")");
        // Pénz levonása és az erőforrások kezelése itt történik majd
    }

    void PlaceGarage(Vector3Int originCell)
    {
        if (!TryValidateGaragePlacement(originCell, out string validationError))
        {
            ReportUserError(validationError);
            return;
        }

        if (garageTilemap == null || garageTile == null)
        {
            ReportUserError("Cannot place garage because garage tiles are not configured.");
            return;
        }

        if (!TrySpendMoneyFromGameData(garagePlacementCost, "build a garage"))
            return;

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

    bool TryGetClosestRoadTile(Vector3Int startCell, out Vector3Int roadCell)
    {
        roadCell = InvalidCellPosition;

        if (roadTilemap == null)
            return false;

        if (IsRoadCoordinate(startCell))
        {
            roadCell = startCell;
            return true;
        }

        for (int i = 0; i < RoadNeighborOffsets.Length; i++)
        {
            Vector3Int adjacentCell = startCell + RoadNeighborOffsets[i];

            if (IsRoadCoordinate(adjacentCell))
            {
                roadCell = adjacentCell;
                return true;
            }
        }

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        queue.Enqueue(startCell);
        visited.Add(startCell);

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            for (int i = 0; i < RoadNeighborOffsets.Length; i++)
            {
                Vector3Int next = current + RoadNeighborOffsets[i];

                if (!visited.Add(next))
                    continue;

                if (IsRoadCoordinate(next))
                {
                    roadCell = next;
                    return true;
                }

                if (groundTilemap.HasTile(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return false;
    }

    Vector3Int GetNearestRoadDirection(Vector3Int cellPos)
    {
        // Direct adjacency has highest priority, then radius search.
        if (IsRoadCoordinate(cellPos + Vector3Int.up))
            return Vector3Int.up;
        if (IsRoadCoordinate(cellPos + Vector3Int.right))
            return Vector3Int.right;
        if (IsRoadCoordinate(cellPos + Vector3Int.down))
            return Vector3Int.down;
        if (IsRoadCoordinate(cellPos + Vector3Int.left))
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

                if (IsRoadCoordinate(next))
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

    List<Vector3Int> FindRoadPath(Vector3Int startCell, Vector3Int endCell)
    {
        if (!CanPlaceCarAt(startCell) || !CanPlaceCarAt(endCell))
            return null;

        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        queue.Enqueue(startCell);
        cameFrom[startCell] = InvalidCellPosition;

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            if (current == endCell)
            {
                return ReconstructRoadPath(cameFrom, endCell);
            }

            for (int i = 0; i < RoadNeighborOffsets.Length; i++)
            {
                Vector3Int next = current + RoadNeighborOffsets[i];

                if (cameFrom.ContainsKey(next) || !IsRoadCoordinate(next))
                    continue;

                cameFrom[next] = current;
                queue.Enqueue(next);
            }
        }

        return null;
    }

    List<Vector3Int> ReconstructRoadPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int endCell)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int current = endCell;

        while (current != InvalidCellPosition)
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
    }

    void SyncCarPlacementSelectionState()
    {
        if (placeBus&& navigationMode == NavigationMode.Camera)
        {
            ClearPendingTruckStopSelections();
            return;
        }

        if (placeTruck && navigationMode == NavigationMode.Camera)
        {
            ClearPendingCarStopSelections();
            return;
        }

        ClearPendingCarStopSelections();
        ClearPendingTruckStopSelections();
    }

    void ClearPendingCarStopSelections()
    {
        pendingCarStopSelections.Clear();
    }

    void ClearPendingTruckStopSelections()
    {
        pendingTruckStopSelections.Clear();
    }

    void RefreshRoadCoordinates()
    {
        roadCoordinates.Clear();
        roadCoordinateLookup.Clear();

        if (roadTilemap == null)
            return;

        BoundsInt cellBounds = roadTilemap.cellBounds;

        foreach (Vector3Int cellPos in cellBounds.allPositionsWithin)
        {
            if (!roadTilemap.HasTile(cellPos))
                continue;

            RegisterRoadCoordinate(cellPos);
        }
    }

    void RefreshWarehouseFootprintOccupancy()
    {
        occupiedWarehouseCells.Clear();

        if (warehouseTilemap == null)
            return;

        BoundsInt cellBounds = warehouseTilemap.cellBounds;

        foreach (Vector3Int cellPos in cellBounds.allPositionsWithin)
        {
            if (!warehouseTilemap.HasTile(cellPos))
                continue;

            List<Vector3Int> warehouseFootprint = GetGarageFootprintCells(cellPos);

            for (int i = 0; i < warehouseFootprint.Count; i++)
            {
                occupiedWarehouseCells.Add(warehouseFootprint[i]);
            }
        }
    }

    void RefreshFacilityFootprintOccupancy()
    {
        occupiedFacilityCells.Clear();
        AddFacilityTilemapOccupancy(ironFactoryTilemap);
        AddFacilityTilemapOccupancy(paperFactoryTilemap);
        AddFacilityTilemapOccupancy(steelFactoryTilemap);
        AddFacilityTilemapOccupancy(woodFactoryTilemap);
        AddFacilityTilemapOccupancy(coalFactoryTilemap);
    }

    void AddFacilityTilemapOccupancy(UnityTilemap tilemap)
    {
        if (tilemap == null)
            return;

        BoundsInt cellBounds = tilemap.cellBounds;

        foreach (Vector3Int cellPos in cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(cellPos))
                continue;

            List<Vector3Int> facilityFootprint = GetGarageFootprintCells(cellPos);

            for (int i = 0; i < facilityFootprint.Count; i++)
            {
                occupiedFacilityCells.Add(facilityFootprint[i]);
            }
        }
    }

    void InitializeWarehousesFromTilemap()
    {
        warehousesByOrigin.Clear();
        nextWarehouseId = 1;

        if (warehouseTilemap == null)
            return;

        if (warehousePrefab == null)
        {
            Debug.LogWarning("Warehouse tilemap is assigned, but warehousePrefab is missing. Cannot spawn warehouse scripts.");
            return;
        }

        HashSet<Vector3Int> warehouseOrigins = new HashSet<Vector3Int>();
        BoundsInt cellBounds = warehouseTilemap.cellBounds;

        foreach (Vector3Int cellPos in cellBounds.allPositionsWithin)
        {
            if (!warehouseTilemap.HasTile(cellPos))
                continue;

            if (TryGetWarehouseOriginCell(cellPos, out Vector3Int warehouseOriginCell))
            {
                warehouseOrigins.Add(warehouseOriginCell);
            }
        }

        foreach (Vector3Int warehouseOriginCell in warehouseOrigins)
        {
            RegisterWarehouseInstance(warehouseOriginCell);
        }
    }

    void InitializeFacilitiesFromTilemaps()
    {
        facilitiesByOrigin.Clear();
        nextFacilityId = 1;

        RegisterFacilitiesFromTilemap(ironFactoryTilemap, ironFoundryPrefab);
        RegisterFacilitiesFromTilemap(paperFactoryTilemap, paperFactoryPrefab);
        RegisterFacilitiesFromTilemap(steelFactoryTilemap, steelFoundryPrefab);
        RegisterFacilitiesFromTilemap(woodFactoryTilemap, woodFactoryPrefab);
        RegisterFacilitiesFromTilemap(coalFactoryTilemap, copperRefineryPrefab);
    }

    void RegisterFacilitiesFromTilemap(UnityTilemap facilityTilemap, Facility facilityPrefab)
    {
        if (facilityTilemap == null)
            return;

        if (facilityPrefab == null)
        {
            Debug.LogWarning("Facility tilemap is assigned, but its prefab is missing. Cannot spawn facility scripts.");
            return;
        }

        HashSet<Vector3Int> facilityOrigins = new HashSet<Vector3Int>();
        BoundsInt cellBounds = facilityTilemap.cellBounds;

        foreach (Vector3Int cellPos in cellBounds.allPositionsWithin)
        {
            if (!facilityTilemap.HasTile(cellPos))
                continue;

            if (TryGetFacilityOriginCell(cellPos, facilityTilemap, out Vector3Int facilityOriginCell))
            {
                facilityOrigins.Add(facilityOriginCell);
            }
        }

        foreach (Vector3Int facilityOriginCell in facilityOrigins)
        {
            RegisterFacilityInstance(facilityOriginCell, facilityTilemap, facilityPrefab);
        }
    }

    public void RegisterPlacedWarehouse(Vector3Int warehouseOriginCell)
    {
        RefreshWarehouseFootprintOccupancy();
        RegisterWarehouseInstance(warehouseOriginCell);
    }

    public void RegisterPlacedFacility(Vector3Int facilityOriginCell, UnityTilemap facilityTilemap, Facility facilityPrefab)
    {
        RefreshFacilityFootprintOccupancy();
        RegisterFacilityInstance(facilityOriginCell, facilityTilemap, facilityPrefab);
    }

    Warehouse RegisterWarehouseInstance(Vector3Int warehouseOriginCell)
    {
        if (warehousesByOrigin.TryGetValue(warehouseOriginCell, out Warehouse existingWarehouse) && existingWarehouse != null)
            return existingWarehouse;

        if (warehousePrefab == null || warehouseTilemap == null)
            return null;

        Vector3 spawnPosition = warehouseTilemap.GetCellCenterWorld(warehouseOriginCell);
        spawnPosition.z = warehousePrefab.transform.position.z;

        Warehouse warehouseInstance = Instantiate(
            warehousePrefab,
            spawnPosition,
            warehousePrefab.transform.rotation,
            warehouseRuntimeRoot);

        warehouseInstance.Initialize(nextWarehouseId++);
        warehousesByOrigin[warehouseOriginCell] = warehouseInstance;
        return warehouseInstance;
    }

    Facility RegisterFacilityInstance(Vector3Int facilityOriginCell, UnityTilemap facilityTilemap, Facility facilityPrefab)
    {
        if (facilitiesByOrigin.TryGetValue(facilityOriginCell, out Facility existingFacility) && existingFacility != null)
            return existingFacility;

        if (facilityTilemap == null || facilityPrefab == null)
            return null;

        Vector3 spawnPosition = facilityTilemap.GetCellCenterWorld(facilityOriginCell);
        spawnPosition.z = facilityPrefab.transform.position.z;

        Facility facilityInstance = Instantiate(
            facilityPrefab,
            spawnPosition,
            facilityPrefab.transform.rotation,
            facilityRuntimeRoot);

        facilityInstance.Initialize(nextFacilityId++);
        facilitiesByOrigin[facilityOriginCell] = facilityInstance;
        return facilityInstance;
    }

    void RegisterRoadCoordinate(Vector3Int cellPos)
    {
        if (!roadCoordinateLookup.Add(cellPos))
            return;

        roadCoordinates.Add(cellPos);
    }

        void DestroyTileAtMousePosition()
        {
            DestroyTile(GetMouseCellPosition());
        }

        void DestroyTile(Vector3Int cellPos)
        {
            if (busStopTilemap != null && busStopTilemap.HasTile(cellPos))
            {
                busStopTilemap.SetTile(cellPos, null);
                HandleStopDestroyed(cellPos);
                Debug.Log("Destroyed bus stop at: " + cellPos);
                return;
            }

            if (garageTilemap != null && TryGetGarageOriginCell(cellPos, out Vector3Int garageOriginCell))
            {
                garageTilemap.SetTile(garageOriginCell, null);
                
                List<Vector3Int> garageFootprint = GetGarageFootprintCells(garageOriginCell);
                for (int i = 0; i < garageFootprint.Count; i++)
                {
                    occupiedGarageCells.Remove(garageFootprint[i]);
                }
                HandleStopDestroyed(garageOriginCell);
                Debug.Log("Destroyed garage at: " + garageOriginCell);
                return;
            }

            if (IsRoadCoordinate(cellPos))
            {
                roadTilemap.SetTile(cellPos, null);
                UnregisterRoadCoordinate(cellPos);
                UpdateRoadTiles(cellPos);
                Debug.Log("Destroyed road at: " + cellPos);
                RecalculateAllVehicleRoutes();
                return;
            }
        }

        void HandleStopDestroyed(Vector3Int stopCell)
        {
            if (VehicleManager.Instance == null) return;
            
            List<Vehicle> allVehicles = new List<Vehicle>(VehicleManager.Instance.GetAllVehicles());
            for (int i = 0; i < allVehicles.Count; i++)
            {
                Vehicle v = allVehicles[i];
                if (v.StopRoute != null && v.StopRoute.Contains(stopCell))
                {
                    v.StopRoute.Remove(stopCell);
                }
            }
            RecalculateAllVehicleRoutes();
        }

        void RecalculateAllVehicleRoutes()
        {
            if (VehicleManager.Instance == null) return;

            List<Vehicle> allVehicles = new List<Vehicle>(VehicleManager.Instance.GetAllVehicles());
            for (int i = 0; i < allVehicles.Count; i++)
            {
                Vehicle v = allVehicles[i];
                
                v.SetRoadCoordinates(roadCoordinates);

                if (v.StopRoute == null || v.StopRoute.Count < 2)
                {
                    SellVehicle(v);
                    continue;
                }

                List<Vector3Int> routeRoadCells = new List<Vector3Int>();
                bool routeValid = true;

                for (int j = 0; j < v.StopRoute.Count; j++)
                {
                    if (!TryGetClosestRoadTile(v.StopRoute[j], out Vector3Int closestRoadCell))
                    {
                        routeValid = false;
                        break;
                    }
                    routeRoadCells.Add(closestRoadCell);
                }

                if (!routeValid)
                {
                    SellVehicle(v);
                    continue;
                }

                List<List<Vector3Int>> loopRouteLegs = new List<List<Vector3Int>>();
                for (int j = 0; j < routeRoadCells.Count; j++)
                {
                    Vector3Int start = routeRoadCells[j];
                    Vector3Int end = routeRoadCells[(j + 1) % routeRoadCells.Count];
                    List<Vector3Int> leg = FindRoadPath(start, end);

                    if (leg == null)
                    {
                        routeValid = false;
                        break;
                    }
                    loopRouteLegs.Add(leg);
                }

                if (!routeValid)
                {
                    SellVehicle(v);
                    continue;
                }

                if (v is Bus bus)
                {
                    bus.SetLoopRoute(loopRouteLegs);
                }
                else if (v is Truck truck)
                {
                    truck.SetLoopRoute(loopRouteLegs);
                }
                else
                {
                    continue;
                }

                Vector3Int currentCell = roadTilemap.WorldToCell(v.transform.position);
                List<Vector3Int> recoveryPath = FindRoadPath(currentCell, routeRoadCells[0]);
                
                if (recoveryPath != null && recoveryPath.Count > 0)
                {
                    List<Vector3Int> mergedRoute = new List<Vector3Int>(recoveryPath);
                    
                    if (loopRouteLegs.Count > 0 && loopRouteLegs[0].Count > 0)
                    {
                        int startIndex = 0;
                        if (mergedRoute.Count > 0 && mergedRoute[mergedRoute.Count - 1] == loopRouteLegs[0][0])
                        {
                            startIndex = 1;
                        }
                        
                        for (int j = startIndex; j < loopRouteLegs[0].Count; j++)
                        {
                            mergedRoute.Add(loopRouteLegs[0][j]);
                        }
                    }
                    
                    if (mergedRoute.Count > 0)
                    {
                        mergedRoute.RemoveAt(0);
                    }
                    
                    v.ApplyBaseRoute(mergedRoute);
                }
                else
                {
                    Vector3 newPos = roadTilemap.GetCellCenterWorld(routeRoadCells[0]);
                    newPos.z = v.transform.position.z;
                    v.transform.position = newPos;
                }
            }
        }
        void SellVehicle(Vehicle vehicle)
        {
            if (gameData != null)
            {
                gameData.Money += (int)(vehicle.Cost * 0.01f * vehicle.GetDurability());
            }
            vehicle.DestroyVehicle();
            VehicleManager.Instance.UnregisterVehicle(vehicle);
            Debug.Log("Vehicle " + vehicle.Id + " was automatically sold because its route became invalid.");
        }

        void UnregisterRoadCoordinate(Vector3Int cellPos)
        {
            if (roadCoordinateLookup.Remove(cellPos))
            {
                roadCoordinates.Remove(cellPos);
            }
        }

    bool IsRoadCoordinate(Vector3Int cellPos)
    {
        return roadCoordinateLookup.Contains(cellPos);
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
        if (navigationMode == NavigationMode.GarageBuild)
            return GetGarageFootprintCells(originCell);
            
        if (navigationMode == NavigationMode.Destroy && garageTilemap != null && TryGetGarageOriginCell(originCell, out Vector3Int garageOriginCell))
            return GetGarageFootprintCells(garageOriginCell);
            
        return new List<Vector3Int> { originCell };
    }

    bool IsBuildPlacementMode()
    {
        return navigationMode == NavigationMode.RoadBuild
            || navigationMode == NavigationMode.StopBuild
                || navigationMode == NavigationMode.GarageBuild
                || navigationMode == NavigationMode.Destroy;
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

    void ApplyBuildPreview(List<Vector3Int> previewCells, Color color)
    {
        for (int i = 0; i < previewCells.Count; i++)
        {
            Vector3Int cellPos = previewCells[i];

            previewedBuildCells.Add(cellPos);
            
            TintCellColor(groundTilemap, cellPos, color);
            TintCellColor(roadTilemap, cellPos, color);
            TintCellColor(busStopTilemap, cellPos, color);
            TintCellColor(garageTilemap, cellPos, color);
        }
    }

    void TintCellColor(UnityTilemap tilemap, Vector3Int cellPos, Color color)
    {
        if (tilemap != null && tilemap.HasTile(cellPos))
        {
            tilemap.SetTileFlags(cellPos, tilemap.GetTileFlags(cellPos) & ~TileFlags.LockColor);
            tilemap.SetColor(cellPos, color);
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

                if (IsRoadCoordinate(neighborCell))
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

            if (IsRoadCoordinate(neighborCell))
            {
                UpdateRoadTile(neighborCell);
            }
        }
    }

    void UpdateRoadTile(Vector3Int cellPos)
    {
        if (!IsRoadCoordinate(cellPos))
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

        if (IsRoadCoordinate(cellPos + Vector3Int.up))
            mask |= 1;

        if (IsRoadCoordinate(cellPos + Vector3Int.right))
            mask |= 2;

        if (IsRoadCoordinate(cellPos + Vector3Int.down))
            mask |= 4;

        if (IsRoadCoordinate(cellPos + Vector3Int.left))
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
}
