using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MiniTransportTycoon
{
    public abstract class Vehicle : MonoBehaviour
    {
    [SerializeField] protected int id;
    [SerializeField] protected TileBase carSprite;
    [SerializeField] protected float speed;
    [SerializeField] protected int age;
    [SerializeField] protected int durability ;
    [SerializeField] protected int maintenanceCost;
    [SerializeField] protected CarType type;

    [Header("Road")]
    [SerializeField] protected Tilemap roadTilemap;
    [SerializeField] protected Tilemap garageTilemap;

    [Header("Directional Sprites")]
    [SerializeField] protected Sprite leftUpSprite;
    [SerializeField] protected Sprite leftDownSprite;
    [SerializeField] protected Sprite rightUpSprite;
    [SerializeField] protected Sprite rightDownSprite;

    protected SpriteRenderer spriteRenderer;

    protected List<Vector3Int> route;
    protected List<Vector3Int> stopRoute;
    protected List<Vector3Int> roadCoordinates;
    protected HashSet<Vector3Int> roadCoordinateLookup;
    protected int currentRouteIndex;
    protected float movementProgress;
    protected Vector3 currentPosition;
    protected Vector3 targetPosition;
    protected bool isMoving;
    protected bool hasAssignedRoute;
    protected virtual float DeltaTime => Time.deltaTime;

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        route ??= new List<Vector3Int>();
        stopRoute ??= new List<Vector3Int>();
        roadCoordinates ??= new List<Vector3Int>();
        roadCoordinateLookup ??= new HashSet<Vector3Int>();
        durability = 100;

        if (VehicleManager.Instance != null)
        {
            id = VehicleManager.Instance.RegisterVehicle(this);
        }
        else
        {
            Debug.LogWarning("VehicleManager do not found!");
        }
    }

    protected virtual void OnDestroy()
    {
        if (VehicleManager.Instance != null)
        {
            VehicleManager.Instance.UnregisterVehicle(this);
        }
    }

    protected virtual void Update()
    {
        if (isMoving && route.Count > 0)
        {
            MoveAlongRoute();
            UpdateDirectionSprite();
        }
    }

    public int GetDurability()
    {
        return durability;
    }

    public void DecreaseDurability()
    {
        if (durability > 0)
        {
            durability--;
        }

        if (durability <= 0)
        {
            Destroy(gameObject);
        }
    }

    public virtual void SetRoute(List<Vector3Int> newRoute)
    {
        route = new List<Vector3Int>();
        currentRouteIndex = 0;
        movementProgress = 0f;
        isMoving = false;
        hasAssignedRoute = true;

        if (newRoute == null)
            return;

        foreach (var tile in newRoute)
        {
            if (IsTrackedRoadCoordinate(tile))
            {
                route.Add(tile);
            }
        }

        if (route.Count > 0)
        {
            currentPosition = transform.position;
            targetPosition = GetWorldPosition(route[0]);
            isMoving = true;
        }
    }

    public void SetStopRoute(List<Vector3Int> newStopRoute)
    {
        stopRoute = newStopRoute != null ? new List<Vector3Int>(newStopRoute) : new List<Vector3Int>();
    }

    public void SetRoadTilemap(Tilemap newRoadTilemap)
    {
        roadTilemap = newRoadTilemap;
    }

    public void SetGarageTilemap(Tilemap newGarageTilemap)
    {
        garageTilemap = newGarageTilemap;
    }

    public void SetRoadCoordinates(List<Vector3Int> newRoadCoordinates)
    {
        roadCoordinates = newRoadCoordinates != null ? new List<Vector3Int>(newRoadCoordinates) : new List<Vector3Int>();
        roadCoordinateLookup = new HashSet<Vector3Int>(roadCoordinates);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = Mathf.Max(0f, newSpeed);
    }

    protected Vector3 GetWorldPosition(Vector3Int cell)
    {
        if (roadTilemap != null)
        {
            Vector3 worldPosition = roadTilemap.GetCellCenterWorld(cell);
            worldPosition.z = transform.position.z;
            return worldPosition;
        }

        return new Vector3(cell.x + 0.5f, cell.y + 0.5f, transform.position.z);
    }

    protected virtual void MoveAlongRoute()
    {
        if (currentRouteIndex >= route.Count)
        {
            isMoving = false;
            return;
        }

        float tileSpeed = speed * DeltaTime;
        movementProgress += tileSpeed;

        if (movementProgress >= 1f)
        {
            currentRouteIndex++;
            movementProgress = 0f;

            if (currentRouteIndex < route.Count)
            {
                if (!IsTrackedRoadCoordinate(route[currentRouteIndex]))
                {
                    Stop();
                    return;
                }

                currentPosition = GetWorldPosition(route[currentRouteIndex - 1]);
                targetPosition = GetWorldPosition(route[currentRouteIndex]);
            }
            else
            {
                isMoving = false;
                return;
            }
        }

        transform.position = Vector3.Lerp(currentPosition, targetPosition, movementProgress);
    }

    protected void UpdateDirectionSprite()
    {
        if (spriteRenderer == null)
            return;

        Vector3 direction = targetPosition - currentPosition;

        if (direction.sqrMagnitude <= Mathf.Epsilon)
            return;

        if (direction.x < 0f)
        {
            ApplyDirectionalSprite(direction.y >= 0f ? leftUpSprite : leftDownSprite,
                direction.y >= 0f ? rightUpSprite : rightDownSprite);
            return;
        }

        ApplyDirectionalSprite(direction.y >= 0f ? rightUpSprite : rightDownSprite,
            direction.y >= 0f ? leftUpSprite : leftDownSprite);
    }

    void ApplyDirectionalSprite(Sprite preferredSprite, Sprite mirroredFallbackSprite)
    {
        if (preferredSprite != null)
        {
            spriteRenderer.flipX = false;
            spriteRenderer.sprite = preferredSprite;
            return;
        }

        if (mirroredFallbackSprite != null)
        {
            spriteRenderer.flipX = true;
            spriteRenderer.sprite = mirroredFallbackSprite;
        }
    }

    public void Stop()
    {
        isMoving = false;
        route?.Clear();
        currentRouteIndex = 0;
        movementProgress = 0f;
    }

    protected bool IsTrackedRoadCoordinate(Vector3Int cell)
    {
        if (roadCoordinateLookup != null && roadCoordinateLookup.Count > 0)
            return roadCoordinateLookup.Contains(cell);

        return roadTilemap != null && roadTilemap.HasTile(cell);
    }

    public int Id => id;
    public int Age => age;
    public int Durability => durability;
    public TileBase CarSprite => carSprite;
    public float Speed => speed;
    public int MaintenanceCost => maintenanceCost;
    public CarType Type => type;
    public List<Vector3Int> Route => route;
    public List<Vector3Int> StopRoute => stopRoute;
    public bool IsMoving => isMoving;
    public bool HasAssignedRoute => hasAssignedRoute;
    }
}
