using System.Collections.Generic;
using UnityEngine;

namespace MiniTransportTycoon
{
    public class Bus : Vehicle
    {
    private readonly List<List<Vector3Int>> loopRouteLegs = new List<List<Vector3Int>>();
    private bool useLoopRoute;
    private int nextLoopLegIndex;
    private bool hasStartedLoopLeg;
    private GameData gameData;
    [SerializeField] private int carryingAmount;
    [SerializeField] private int maxCarryingAmount = 50;

    private void Awake()
    {
        type = CarType.Bus;
    }

    protected override void Start()
    {
        base.Start();
        gameData = FindObjectOfType<GameData>();
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
            fullRoadPath != null ? new List<Vector3Int>(fullRoadPath) : new List<Vector3Int>(),
            BuildReversedLeg(fullRoadPath)
        };

        SetLoopRoute(shuttleLegs);
    }

    private List<Vector3Int> BuildReversedLeg(List<Vector3Int> fullRoadPath)
    {
        List<Vector3Int> reversedLeg = fullRoadPath != null ? new List<Vector3Int>(fullRoadPath) : new List<Vector3Int>();
        reversedLeg.Reverse();
        return reversedLeg;
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

    public void Maintain()
    {

        gameData.Money -= maintenanceCost;
    }

    public void SetMaxCarryingAmount(int maxAmount)
    {
        maxCarryingAmount = Mathf.Max(0, maxAmount);
        carryingAmount = Mathf.Clamp(carryingAmount, 0, maxCarryingAmount);
    }

    public void SetCost(int cost)
    {
        this.cost = Mathf.Max(0, cost);
    }

    private void Reset()
    {
        type = CarType.Bus;
    }

    private void OnValidate()
    {
        type = CarType.Bus;
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

    public int CarryingAmount => carryingAmount;
    public int MaxCarryingAmount => maxCarryingAmount;
    }
}
