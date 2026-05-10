using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Tilemaps;
using MiniTransportTycoon;
using System;
using System.Diagnostics;
using System.Collections.Specialized;
using UnityEditor;

namespace MiniTransportTycoon
{
    public class SaveSystem : MonoBehaviour
    {
        [SerializeField] private GameData gameData;

        [SerializeField] private Tilemap roadTilemap;
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Tilemap houseTilemap;
        [SerializeField] private Tilemap decorationTilemap;

        [SerializeField] private Tilemap ironFactoryTilemap;
        [SerializeField] private Tilemap steelFactoryTilemap;
        [SerializeField] private Tilemap woodFactoryTilemap;
        [SerializeField] private Tilemap paperFactoryTilemap;
        [SerializeField] private Tilemap coalFactoryTilemap;

        [SerializeField] private Tilemap garageTilemap;
        [SerializeField] private Tilemap warehouseTilemap;
        [SerializeField] private Tilemap busStopTilemap;

        [SerializeField] private TileBase[] allTiles;

        [Header("Vehicle Prefabs")]
        [SerializeField] private Bus busPrefab;
        [SerializeField] private Truck truckPrefab;
        [SerializeField] private Car carPrefab;

        [Header("Facility Prefabs")]
        [SerializeField] private IronFactory ironFactoryPrefab;
        [SerializeField] private SteelFactory steelFactoryPrefab;
        [SerializeField] private WoodFactory woodFactoryPrefab;
        [SerializeField] private PaperFactory paperFactoryPrefab;
        [SerializeField] private CoalFactory coalFactoryPrefab;
        [SerializeField] private Warehouse warehousePrefab;

        private int activeSlot = -1;

        private string GetSavePath(int slot) => Path.Combine(Application.persistentDataPath, $"save_{slot}.json");

        //TEMP!!!
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Save(activeSlot);
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                Save(SaveReader.GetNextSlot());
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                PurgeAll();
            }

            if (Input.GetKeyDown(KeyCode.F9))
            {
                Load(activeSlot);
            }
        }

        private void Start()
        {
            if(GameSession.SlotToLoad != -1)
            {
                PurgeAll();
                Load(GameSession.SlotToLoad);
                activeSlot = GameSession.SlotToLoad;
                GameSession.SlotToLoad = -1;
            }else if (GameSession.CityName != "")
            {
                UnityEngine.Debug.Log("Start new game with city " + GameSession.CityName);
                gameData.CityName = GameSession.CityName;
                GameSession.CityName = "";
                activeSlot = SaveReader.GetNextSlot();
                UnityEngine.Debug.Log("Saving to slot " + activeSlot);
                Save(activeSlot);
            }
        }

        /*
         private void Start()
        {
            if(GameSession.SlotToLoad != -1)
            {
                PurgeAll();
                Load(GameSession.SlotToLoad);
                GameSession.SlotToLoad = -1;
            }else if (GameSession.CityName != "")
            {
                gameData.CityName = GameSession.CityName;
                GameSession.CityName = "";
            }
        }
         */

#if UNITY_EDITOR

private void OnValidate()
{
    string[] guids = AssetDatabase.FindAssets("t:TileBase", new[] {"Assets/Tilemaps/Tiles"});
    allTiles = new TileBase[guids.Length];
    for (int i = 0; i < guids.Length; i++)
	{
        string path = AssetDatabase.GUIDToAssetPath(guids[i]);
        allTiles[i] = AssetDatabase.LoadAssetAtPath<TileBase>(path);
	}
}

#endif

        public void SaveToCurrentSlot()
        {
            Save(activeSlot);
        }

        public void Save(int slot)
        {
            SaveData data = new SaveData();

            data.money = gameData.Money;
            data.iron = gameData.Iron;
            data.steel = gameData.Steel;
            data.wood = gameData.Wood;
            data.paper = gameData.Paper;
            data.coal = gameData.Coal;
            data.currentDate = gameData.CurrentDate.ToString();

            data.saveDate = DateTime.Now.ToString();//
            data.slot = slot;//
            data.cityName = gameData.CityName;

            SaveTilemap(roadTilemap, data.roadTiles);
            SaveTilemap(groundTilemap, data.groundTiles);
            SaveTilemap(houseTilemap, data.houseTiles);
            SaveTilemap(decorationTilemap, data.decorationTiles);

            SaveTilemap(ironFactoryTilemap, data.ironFactoryTiles);
            SaveTilemap(steelFactoryTilemap, data.steelFactoryTiles);
            SaveTilemap(woodFactoryTilemap, data.woodFactoryTiles);
            SaveTilemap(paperFactoryTilemap, data.paperFactoryTiles);
            SaveTilemap(coalFactoryTilemap, data.coalFactoryTiles);

            SaveTilemap(busStopTilemap, data.busStopTiles);
            SaveTilemap(garageTilemap, data.garageTiles);
            SaveTilemap(warehouseTilemap, data.warehouseTiles);

            SaveFacilities(ref data);
            SaveWareHouses(ref data);
            SaveVehicles(ref data);

            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(GetSavePath(slot), json);
            UnityEngine.Debug.Log("Game saved to: " + GetSavePath(slot));
        }

        private void SaveTilemap(Tilemap tilemap, List<TileSaveData> tileList)
        {
            foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
            {
                if (!tilemap.HasTile(pos))
                {
                    continue;
                }

                tileList.Add(new TileSaveData
                {
                    x = pos.x,
                    y = pos.y,
                    tileType = tilemap.GetTile(pos).name
                });
            }
        }

        public void PurgeAll()
        {
            // Clear all tilemaps
            roadTilemap.ClearAllTiles();
            groundTilemap.ClearAllTiles();
            houseTilemap.ClearAllTiles();
            decorationTilemap.ClearAllTiles();
            garageTilemap.ClearAllTiles();
            busStopTilemap.ClearAllTiles();

            // Destroy all vehicles
            Vehicle[] allVehicles = FindObjectsOfType<Vehicle>();
            foreach (Vehicle v in allVehicles)
                Destroy(v.gameObject);

            // Destroy all facilities
            Facility[] allFacilities = FindObjectsOfType<Facility>();
            foreach (Facility f in allFacilities)
                Destroy(f.gameObject);

            // Destroy all warehouses
            Warehouse[] allWarehouses = FindObjectsOfType<Warehouse>();
            foreach (Warehouse w in allWarehouses)
                Destroy(w.gameObject);

            // Reset GameData
            gameData.Money = 0;
            gameData.Iron = 0;
            gameData.Steel = 0;
            gameData.Wood = 0;
            gameData.Paper = 0;
            gameData.Coal = 0;

            woodFactoryTilemap.ClearAllTiles();
            ironFactoryTilemap.ClearAllTiles();
            steelFactoryTilemap.ClearAllTiles();
            paperFactoryTilemap.ClearAllTiles();
            coalFactoryTilemap.ClearAllTiles();
            warehouseTilemap.ClearAllTiles();

            UnityEngine.Debug.Log("Purge complete!");
        }

        public void Load(int slot)
        {
            string path = GetSavePath(slot);

            if (!File.Exists(path))
            {
                UnityEngine.Debug.LogWarning("No save file found in slot " + slot);
                return;
            }

            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            ironFactoryTilemap.ClearAllTiles();
            steelFactoryTilemap.ClearAllTiles();
            woodFactoryTilemap.ClearAllTiles();
            paperFactoryTilemap.ClearAllTiles();
            coalFactoryTilemap.ClearAllTiles();
            warehouseTilemap.ClearAllTiles();

            //restore GameData fields
            gameData.Money = data.money;
            gameData.Iron = data.iron;
            gameData.Steel = data.steel;
            gameData.Wood = data.wood;
            gameData.Paper = data.paper;
            gameData.Coal = data.coal;

            //restore tilemaps
            roadTilemap.ClearAllTiles();
            groundTilemap.ClearAllTiles();
            houseTilemap.ClearAllTiles();
            decorationTilemap.ClearAllTiles();
            garageTilemap.ClearAllTiles();
            busStopTilemap.ClearAllTiles();

            LoadTilemap(roadTilemap, data.roadTiles);
            LoadTilemap(groundTilemap, data.groundTiles);
            LoadTilemap(houseTilemap, data.houseTiles);
            LoadTilemap(decorationTilemap, data.decorationTiles);
            LoadTilemap(garageTilemap, data.garageTiles);
            LoadTilemap(busStopTilemap, data.busStopTiles);

            LoadTilemap(ironFactoryTilemap, data.ironFactoryTiles);
            LoadTilemap(steelFactoryTilemap, data.steelFactoryTiles);
            LoadTilemap(woodFactoryTilemap, data.woodFactoryTiles);
            LoadTilemap(paperFactoryTilemap, data.paperFactoryTiles);
            LoadTilemap(coalFactoryTilemap, data.coalFactoryTiles);
            LoadTilemap(warehouseTilemap, data.warehouseTiles);

            //other objects
            LoadFacilities(ref data);
            LoadWarehouses(ref data);
            LoadVehicles(ref data);

            UnityEngine.Debug.Log("Game loaded from slot " + slot);
        }

        private void LoadTilemap(Tilemap tilemap, List<TileSaveData> tileList)
        {
            foreach (TileSaveData tileData in tileList)
            {
                TileBase tile = FindTileByName(tileData.tileType);
                if (tile == null)
                {
                    UnityEngine.Debug.LogWarning("Could not find tile:" + tileData.tileType);
                    continue;
                }

                tilemap.SetTile(new Vector3Int(tileData.x, tileData.y, 0), tile);
            }
        }

        private TileBase FindTileByName(string tileName)
        {
            foreach (TileBase tile in allTiles)
            {
                if (tile.name == tileName)
                {
                    return tile;
                }
            }
            return null;
        }

        private void SaveFacilities(ref SaveData data)
        {
            Facility[] allFacilities = FindObjectsOfType<Facility>();
            foreach (Facility f in allFacilities)
            {
                FacilitySaveData fd = new FacilitySaveData
                {
                    facilityType = f.GetType().Name,
                    id = f.Id,
                    posX = f.transform.position.x,
                    posY = f.transform.position.y,
                    resourceAmount = f.RemainingResourceAmount,
                    callCount = f.CallCount,
                    productivityMultiplier = f.ProductivityMultiplier,
                    inputInventory = new List<MaterialAmount>(f.InputInventory)
                };
                data.facilities.Add(fd);
            }
        }

        private void SaveWareHouses(ref SaveData data)
        {
            Warehouse[] allWarehouses = FindObjectsOfType<Warehouse>();
            foreach (Warehouse w in allWarehouses)
            {
                WarehouseSaveData wd = new WarehouseSaveData
                {
                    id = w.Id,
                    posX = (int)w.transform.position.x,
                    posY = (int)w.transform.position.y,
                    needs = new List<MaterialAmount>(w.Needs),
                    inventory = new List<MaterialAmount>(w.Inventory)
                };
                data.warehouses.Add(wd);
            }
        }

        private void SaveVehicles(ref SaveData data)
        {
            Vehicle[] allVehicles = FindObjectsOfType<Vehicle>();
            foreach (Vehicle v in allVehicles)
            {
                VehicleSaveData vd = new VehicleSaveData
                {
                    id = v.Id,
                    posX = (int)v.transform.position.x,
                    posY = (int)v.transform.position.y,
                    speed = v.Speed,
                    age = v.Age,
                    route = SerializeRoute(v.Route),
                    stopRoute = SerializeRoute(v.StopRoute)
                };

                if (v is Truck truck)
                {
                    vd.materialType = truck.MaterialType.ToString();
                    vd.carryingAmount = truck.CarryingAmount;
                    vd.maxCarryingAmount = truck.MaxCarryingAmount;
                }
                else if (v is Car car)
                {
                    vd.shuttleRouteForward = SerializeRoute(car.ShuttleRouteForward);
                    vd.shuttleRouteBackward = SerializeRoute(car.ShuttleRouteBackward);
                    vd.useShuttleRoute = car.UseShuttleRoute;
                    vd.nextShuttleLegIsForward = car.NextShuttleLegIsForward;
                }

                data.vehicles.Add(vd);
            }

            data.nextVehicleId = 1;

            if (VehicleManager.Instance != null)
            {
                data.nextVehicleId = VehicleManager.Instance.NextId;
            }
        }

        private List<Vector3IntSaveData> SerializeRoute(List<Vector3Int> route)
        {
            List<Vector3IntSaveData> result = new List<Vector3IntSaveData>();

            if (route == null)
            {
                return result;
            }

            foreach (Vector3Int v in route)
            {
                result.Add(new Vector3IntSaveData
                {
                    x = v.x,
                    y = v.y,
                    z = v.z
                });
            }

            return result;
        }

        private void LoadFacilities(ref SaveData data)
        {
            Facility[] existingFacilities = FindObjectsOfType<Facility>();

            foreach (Facility f in existingFacilities)
            {
                Destroy(f.gameObject);
            }

            foreach (FacilitySaveData fd in data.facilities)
            {
                Facility prefab = GetFacilityPrefab(fd.facilityType);
                if (prefab == null)
                {
                    UnityEngine.Debug.LogError("Could not find prefab for facility type: " + fd.facilityType);
                    continue;
                }

                Vector3 position = new Vector3(fd.posX, fd.posY, 0);
                Facility instance = Instantiate(prefab, position, Quaternion.identity);
                instance.Initialize(fd.id);

                if (fd.inputInventory != null)
                {
                    foreach (MaterialAmount ma in fd.inputInventory)
                    {
                        instance.AddInputMaterial(ma.material, ma.amount);
                    }
                }

                instance.RestoreProductivityState(fd.callCount, fd.productivityMultiplier);
            }
        }

        private Facility GetFacilityPrefab(string facilityType)
        {
            switch (facilityType)
            {
                case "IronFactory": return ironFactoryPrefab;
                case "SteelFactory": return steelFactoryPrefab;
                case "WoodFactory": return woodFactoryPrefab;
                case "PaperFactory": return paperFactoryPrefab;
                case "CoalFactory": return coalFactoryPrefab;
                default: return null;
            }
        }

        private void LoadWarehouses(ref SaveData data)
        {
            Warehouse[] existingWarehouses = FindObjectsOfType<Warehouse>();

            foreach (Warehouse w in existingWarehouses)
            {
                Destroy(w.gameObject);
            }

            foreach (WarehouseSaveData wd in data.warehouses)
            {
                Vector3 position = new Vector3(wd.posX, wd.posY, 0);
                Warehouse instance = Instantiate(warehousePrefab, position, Quaternion.identity);
                instance.Initialize(wd.id, wd.needs, wd.inventory);
            }
        }

        private void LoadVehicles(ref SaveData data)
        {
            Vehicle[] existingVehicles = FindObjectsOfType<Vehicle>();

            foreach (Vehicle v in existingVehicles)
            {
                Destroy(v.gameObject);
            }

            foreach (VehicleSaveData vd in data.vehicles)
            {
                Vehicle prefab = GetVehiclePrefab(vd.vehicleType);
                if (prefab == null)
                {
                    UnityEngine.Debug.LogWarning("Could not find prefab for vehicle type: " + vd.vehicleType);
                    continue;
                }

                Vector3 position = new Vector3(vd.posX, vd.posY, 0);
                Vehicle instance = Instantiate(prefab, position, Quaternion.identity);

                instance.SetSpeed(vd.speed);
                instance.SetRoadTilemap(roadTilemap);
                instance.SetStopRoute(DeserializeRoute(vd.stopRoute));
                instance.SetRoute(DeserializeRoute(vd.route));

                if (instance is Truck truck)
                {
                    if (Enum.TryParse(vd.materialType, out Materials material))
                    {
                        truck.SetMaterialType(material);
                    }
                    truck.SetMaxCarryingAmount(vd.maxCarryingAmount);
                    truck.LoadMaterial(vd.carryingAmount);
                }
                else if (instance is Car car)
                {
                    if (vd.useShuttleRoute)
                    {
                        List<Vector3Int> forward = DeserializeRoute(vd.shuttleRouteForward);
                        car.SetShuttleRoute(forward);
                    }
                }
            }

            if (VehicleManager.Instance != null)
            {
                VehicleManager.Instance.SetNextId(data.nextVehicleId);
            }
        }

        private Vehicle GetVehiclePrefab(string vehicleType)
        {
            switch (vehicleType)
            {
                case "Bus": return busPrefab;
                case "Truck": return truckPrefab;
                case "Car": return carPrefab;
                default: return null;
            }
        }

        private List<Vector3Int> DeserializeRoute(List<Vector3IntSaveData> savedRoute)
        {
            List<Vector3Int> route = new List<Vector3Int>();
            if (savedRoute == null)
            {
                return route;
            }
            foreach (Vector3IntSaveData v in savedRoute)
            {
                route.Add(new Vector3Int(v.x, v.y, v.z));
            }

            return route;
        }
    }
}
