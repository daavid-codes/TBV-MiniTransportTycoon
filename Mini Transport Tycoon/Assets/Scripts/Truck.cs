using System.Collections.Generic;
using UnityEngine;

namespace MiniTransportTycoon
{
    public class Truck : Vehicle
    {
    private readonly List<List<Vector3Int>> loopRouteLegs = new List<List<Vector3Int>>();
    private bool useLoopRoute;
    private int nextLoopLegIndex;
    private bool hasStartedLoopLeg;
    private GameData gameData;
    private GameController gameController;
    [SerializeField] private Materials materialType;
    [SerializeField] private int carryingAmount;
    [SerializeField] private int maxCarryingAmount = 500;

    private void Awake()
    {
        type = CarType.Truck;
    }

    protected override void Start()
    {
        base.Start();
        gameData = FindObjectOfType<GameData>();
        gameController = FindObjectOfType<GameController>();
    }

    protected override void Update()
    {
        base.Update();

        if (!useLoopRoute || IsMoving)
            return;

        if (loopRouteLegs.Count == 0)
        {
            useLoopRoute = false;
            return;
        }

        HandleStopArrival();
        StartNextLoopLeg();
    }

    public override void SetRoute(List<Vector3Int> newRoute)
    {
        useLoopRoute = false;
        loopRouteLegs.Clear();
        nextLoopLegIndex = 0;
        base.SetRoute(newRoute);
    }

    public void SetShuttleRoute(List<Vector3Int> fullRoadPath)
    {
        List<List<Vector3Int>> shuttleLegs = new List<List<Vector3Int>>
        {
            BuildLoopLeg(fullRoadPath, reverse: false),
            BuildLoopLeg(fullRoadPath, reverse: true)
        };

        SetLoopRoute(shuttleLegs);
    }

    public void SetLoopRoute(List<List<Vector3Int>> newLoopLegs)
    {
        useLoopRoute = false;
        loopRouteLegs.Clear();
        nextLoopLegIndex = 0;
        hasStartedLoopLeg = false;

        if (newLoopLegs == null)
        {
            base.SetRoute(null);
            return;
        }

        for (int i = 0; i < newLoopLegs.Count; i++)
        {
            List<Vector3Int> leg = TrimLegStart(newLoopLegs[i]);

            if (leg.Count == 0)
                continue;

            loopRouteLegs.Add(leg);
        }

        if (loopRouteLegs.Count == 0)
        {
            base.SetRoute(null);
            return;
        }

        useLoopRoute = true;
        StartNextLoopLeg();
    }

    public void SetMaterialType(Materials material)
    {
        materialType = material;
    }

    public int LoadMaterial(int amount)
    {
        if (amount <= 0)
            return 0;

        int availableCapacity = Mathf.Max(0, maxCarryingAmount - carryingAmount);
        int loadedAmount = Mathf.Min(amount, availableCapacity);
        carryingAmount += loadedAmount;
        return loadedAmount;
    }

    public int UnloadMaterial(int amount)
    {
        if (amount <= 0)
            return 0;

        int unloadedAmount = Mathf.Min(amount, carryingAmount);
        carryingAmount -= unloadedAmount;
        return unloadedAmount;
    }

    public void Maintain()
    {
        gameData.Money -= maintenanceCost;
    }

    private void Reset()
    {
        type = CarType.Truck;
    }

    private void OnValidate()
    {
        type = CarType.Truck;
        maxCarryingAmount = Mathf.Max(0, maxCarryingAmount);
        carryingAmount = Mathf.Clamp(carryingAmount, 0, maxCarryingAmount);
    }

    private void StartNextLoopLeg()
    {
        if (loopRouteLegs.Count == 0)
            return;

        List<Vector3Int> nextRoute = loopRouteLegs[nextLoopLegIndex];

        base.SetRoute(nextRoute);
        nextLoopLegIndex = (nextLoopLegIndex + 1) % loopRouteLegs.Count;
        hasStartedLoopLeg = true;
    }

    private void HandleStopArrival()
    {
        if (!hasStartedLoopLeg)
            return;

        if (stopRoute == null || stopRoute.Count == 0 || garageTilemap == null)
            return;

        int reachedStopIndex = nextLoopLegIndex % stopRoute.Count;
        Vector3Int reachedStopCell = stopRoute[reachedStopIndex];

        if (garageTilemap.HasTile(reachedStopCell))
        {
            Maintain();
        }

        HandleMaterialTransferAtStop(reachedStopCell);
    }

    private void HandleMaterialTransferAtStop(Vector3Int reachedStopCell)
    {
        if (gameController == null)
            return;

        if (gameController.TryGetWarehouseAtRoutePoint(reachedStopCell, out Warehouse warehouse) && warehouse != null)
        {
            int unloadedAmount = UnloadMaterial(carryingAmount);

            if (unloadedAmount > 0)
            {
                int currentWarehouseAmount = warehouse.GetInventoryAmount(materialType);
                warehouse.SetInventoryAmount(materialType, currentWarehouseAmount + unloadedAmount);
                Debug.Log("Truck unloaded " + unloadedAmount + " of " + materialType + " to warehouse " + warehouse.Id + ".");
            }

            return;
        }

        if (!gameController.TryGetFacilityAtRoutePoint(reachedStopCell, out Facility facility) || facility == null)
            return;

        if (carryingAmount > 0 && facility.RequiresInputMaterial(materialType))
        {
            int unloadedAmount = UnloadMaterial(carryingAmount);

            if (unloadedAmount > 0)
            {
                facility.AddInputMaterial(materialType, unloadedAmount);
                Debug.Log("Truck unloaded " + unloadedAmount + " of " + materialType + " to factory " + facility.Id + ".");
            }
        }

        if (facility.ProducedMaterialType != materialType)
            return;

        int availableCapacity = Mathf.Max(0, maxCarryingAmount - carryingAmount);

        if (availableCapacity <= 0)
            return;

        int takenAmount = facility.TakeProducedMaterial(availableCapacity);
        int loadedAmount = LoadMaterial(takenAmount);

        if (loadedAmount > 0)
        {
            Debug.Log("Truck loaded " + loadedAmount + " of " + materialType + " from factory " + facility.Id + ".");
        }
    }

    private List<Vector3Int> BuildLoopLeg(List<Vector3Int> fullRoadPath, bool reverse)
    {
        List<Vector3Int> leg = fullRoadPath != null ? new List<Vector3Int>(fullRoadPath) : new List<Vector3Int>();

        if (reverse)
        {
            leg.Reverse();
        }

        return TrimLegStart(leg);
    }

    private List<Vector3Int> TrimLegStart(List<Vector3Int> leg)
    {
        List<Vector3Int> trimmedLeg = leg != null ? new List<Vector3Int>(leg) : new List<Vector3Int>();

        if (trimmedLeg.Count > 0)
            trimmedLeg.RemoveAt(0);

        return trimmedLeg;
    }

    public Materials MaterialType => materialType;
    public int CarryingAmount => carryingAmount;
    public int MaxCarryingAmount => maxCarryingAmount;
    }
}
