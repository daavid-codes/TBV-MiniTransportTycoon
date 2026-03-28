using System.Collections.Generic;
using UnityEngine;

public class Car : Vehicle
{
    [Header("Example Route")]
    [SerializeField] private bool useExampleRouteOnStart = true;
    [SerializeField] private List<Vector3Int> exampleRoute = new List<Vector3Int>();

    private void Awake()
    {
        type = CarType.Car;
    }

    protected override void Start()
    {
        base.Start();
        EnsureExampleRoute();

        if (useExampleRouteOnStart)
        {
            SetRoute(exampleRoute);
        }
    }

    private void Reset()
    {
        type = CarType.Car;
        SetDefaultExampleRoute();
    }

    private void OnValidate()
    {
        type = CarType.Car;
        EnsureExampleRoute();
    }

    [ContextMenu("Load Example Route")]
    private void LoadExampleRoute()
    {
        EnsureExampleRoute();
        SetRoute(exampleRoute);
    }

    private void EnsureExampleRoute()
    {
        if (exampleRoute == null)
        {
            exampleRoute = new List<Vector3Int>();
        }

        if (exampleRoute.Count == 0)
        {
            SetDefaultExampleRoute();
        }
    }

    private void SetDefaultExampleRoute()
    {
        exampleRoute = new List<Vector3Int>
        {
            new Vector3Int(0, 7, 0),
            new Vector3Int(1, 7, 0),
            new Vector3Int(2, 7, 0),
            new Vector3Int(3, 7, 0),
            new Vector3Int(4, 7, 0)
        };
    }
}
