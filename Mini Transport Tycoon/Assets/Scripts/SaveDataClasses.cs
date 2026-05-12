using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

namespace MiniTransportTycoon
{
    [Serializable]
    public class SaveData
    {
        public int slot;
        public string saveDate;
        public string cityName;

        public int money;
        public int iron;
        public int steel;
        public int wood;
        public int paper;
        public int coal;

        public string currentDate;

        public List<TileSaveData> roadTiles = new List<TileSaveData>();
        public List<TileSaveData> groundTiles = new List<TileSaveData>();
        public List<TileSaveData> houseTiles = new List<TileSaveData>();
        public List<TileSaveData> decorationTiles = new List<TileSaveData>();

        public List<TileSaveData> woodFactoryTiles = new List<TileSaveData>();
        public List<TileSaveData> ironFactoryTiles = new List<TileSaveData>();
        public List<TileSaveData> steelFactoryTiles = new List<TileSaveData>();
        public List<TileSaveData> paperFactoryTiles = new List<TileSaveData>();
        public List<TileSaveData> coalFactoryTiles = new List<TileSaveData>();

        public List<TileSaveData> garageTiles = new List<TileSaveData>();
        public List<TileSaveData> busStopTiles = new List<TileSaveData>();
        public List<TileSaveData> warehouseTiles = new List<TileSaveData>();

        public List<FacilitySaveData> facilities = new List<FacilitySaveData>();
        public List<WarehouseSaveData> warehouses = new List<WarehouseSaveData>();
        public List<VehicleSaveData> vehicles = new List<VehicleSaveData>();

        public int nextVehicleId;
    }

    [Serializable]
    public class TileSaveData
    {
        public int x;
        public int y;
        public string tileType;
    }

    [Serializable]
    public class FacilitySaveData
    {
        public string facilityType;
        public int id;
        public float posX, posY;
        public int resourceAmount;
        public int storedProductAmount;
        public int callCount;
        public float productivityMultiplier;
        public List<MaterialAmount> inputInventory;
    }

    [Serializable]
    public class WarehouseSaveData
    {
        public int id;
        public int posX, posY;
        public List<MaterialAmount> needs;
        public List<MaterialAmount> inventory;
    }

    [Serializable]
    public class VehicleSaveData
    {
        public string vehicleType;
        public int id;
        public int posX, posY;
        public float speed;
        public int age;
        public List<Vector3IntSaveData> route;
        public List<Vector3IntSaveData> stopRoute;

        public string materialType;
        public int carryingAmount;
        public int maxCarryingAmount;

        public List<Vector3IntSaveData> shuttleRouteForward;
        public List<Vector3IntSaveData> shuttleRouteBackward;
        public bool useShuttleRoute;
        public bool nextShuttleLegIsForward;

        public List<RouteLeg> loopRouteLegs = new List<RouteLeg>();
        public bool useLoopRoute;
        public int nextLoopLegIndex;
        public bool hasStartedLooping;
    }

    [Serializable]
    public class Vector3IntSaveData
    {
        public int x, y, z;
    }

    [Serializable]
    public class RouteLeg
    {
        public List<Vector3IntSaveData> cells = new List<Vector3IntSaveData>();
    }
}