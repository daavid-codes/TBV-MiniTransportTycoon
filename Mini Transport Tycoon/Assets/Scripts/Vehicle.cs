using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Vehicle : MonoBehaviour
{
    [SerializeField] protected TileBase carSprite;
    [SerializeField] protected float speed;
    [SerializeField] protected int age;
    [SerializeField] protected int maintenanceCost;
    [SerializeField] protected CarType type;

    [Header("Road")]
    [SerializeField] protected Tilemap roadTilemap;

    [Header("Directional Sprites")]
    [SerializeField] protected Sprite upSprite;
    [SerializeField] protected Sprite downSprite;
    [SerializeField] protected Sprite leftSprite;
    [SerializeField] protected Sprite rightSprite;

    protected SpriteRenderer spriteRenderer;

    protected List<Vector3Int> route;
    protected int currentRouteIndex;
    protected float movementProgress;
    protected Vector3 currentPosition;
    protected Vector3 targetPosition;
    protected bool isMoving;

    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        route = new List<Vector3Int>();
        currentRouteIndex = 0;
        movementProgress = 0f;
        isMoving = false;
    }

    protected virtual void Update()
    {
        if (isMoving && route.Count > 0)
        {
            MoveAlongRoute();
            UpdateDirectionSprite();
        }
    }

    public void SetRoute(List<Vector3Int> newRoute)
    {
        route = new List<Vector3Int>();

        foreach (var tile in newRoute)
        {
            if (roadTilemap != null && roadTilemap.HasTile(tile))
            {
                route.Add(tile);
            }
        }

        currentRouteIndex = 0;
        movementProgress = 0f;

        if (route.Count > 0)
        {
            currentPosition = transform.position;
            targetPosition = GetWorldPosition(route[0]);
            isMoving = true;
        }
    }

    protected Vector3 GetWorldPosition(Vector3Int cell)
    {
        return new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f);
    }

    protected virtual void MoveAlongRoute()
    {
        if (currentRouteIndex >= route.Count)
        {
            isMoving = false;
            return;
        }

        float tileSpeed = speed * Time.deltaTime;
        movementProgress += tileSpeed;

        if (movementProgress >= 1f)
        {
            currentRouteIndex++;
            movementProgress = 0f;

            if (currentRouteIndex < route.Count)
            {
                if (roadTilemap != null && !roadTilemap.HasTile(route[currentRouteIndex]))
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
        Vector3 direction = (targetPosition - transform.position).normalized;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Horizontal
            if (direction.x > 0)
                spriteRenderer.sprite = rightSprite;
            else
                spriteRenderer.sprite = leftSprite;
        }
        else
        {
            // Vertical
            if (direction.y > 0)
                spriteRenderer.sprite = upSprite;
            else
                spriteRenderer.sprite = downSprite;
        }
    }

    public void Stop()
    {
        isMoving = false;
        route.Clear();
        currentRouteIndex = 0;
        movementProgress = 0f;
    }

    public TileBase CarSprite => carSprite;
    public float Speed => speed;
    public int MaintenanceCost => maintenanceCost;
    public CarType Type => type;
    public List<Vector3Int> Route => route;
    public bool IsMoving => isMoving;
}
