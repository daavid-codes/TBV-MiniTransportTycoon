using System.Collections.Generic;
using UnityEngine;

namespace MiniTransportTycoon
{
    public class Bus : Vehicle
    {
    private List<Vector3Int> shuttleRouteForward = new List<Vector3Int>();
    private List<Vector3Int> shuttleRouteBackward = new List<Vector3Int>();
    private bool useShuttleRoute;
    private bool nextShuttleLegIsForward;

    private void Awake()
    {
        type = CarType.Bus;
    }

    protected override void Start()
    {
        base.Start();

    }

    protected override void Update()
    {
        base.Update();

        if (!useShuttleRoute || IsMoving)
            return;

        if (Route == null || Route.Count == 0)
        {
            useShuttleRoute = false;
            return;
        }

        StartNextShuttleLeg();
    }

    public override void SetRoute(List<Vector3Int> newRoute)
    {
        useShuttleRoute = false;
        shuttleRouteForward = new List<Vector3Int>();
        shuttleRouteBackward = new List<Vector3Int>();
        base.SetRoute(newRoute);
    }

    public void SetShuttleRoute(List<Vector3Int> fullRoadPath)
    {
        shuttleRouteForward = BuildShuttleLeg(fullRoadPath, reverse: false);
        shuttleRouteBackward = BuildShuttleLeg(fullRoadPath, reverse: true);
        useShuttleRoute = shuttleRouteForward.Count > 0 && shuttleRouteBackward.Count > 0;
        nextShuttleLegIsForward = false;

        if (!useShuttleRoute)
        {
            base.SetRoute(shuttleRouteForward);
            return;
        }

        base.SetRoute(shuttleRouteForward);
    }

    private void Reset()
    {
        type = CarType.Bus;
    }

    private void OnValidate()
    {
        type = CarType.Bus;
    }

    private void StartNextShuttleLeg()
    {
        List<Vector3Int> nextRoute = nextShuttleLegIsForward ? shuttleRouteForward : shuttleRouteBackward;

        if (nextRoute == null || nextRoute.Count == 0)
        {
            useShuttleRoute = false;
            return;
        }

        base.SetRoute(nextRoute);
        nextShuttleLegIsForward = !nextShuttleLegIsForward;
    }

    private List<Vector3Int> BuildShuttleLeg(List<Vector3Int> fullRoadPath, bool reverse)
    {
        List<Vector3Int> leg = fullRoadPath != null ? new List<Vector3Int>(fullRoadPath) : new List<Vector3Int>();

        if (reverse)
        {
            leg.Reverse();
        }

        if (leg.Count > 0)
        {
            leg.RemoveAt(0);
        }

        return leg;
    }
    }
}
