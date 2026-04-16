using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniTransportTycoon
{
    public class MiniMapClick : MonoBehaviour
    {
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private Camera mainCamera;
    private RectTransform rect;

    private Vector2 localPoint;
    private Vector2 normalized;
    private Vector3 worldPoint;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition))
            {
                ClickedOnMinimap();
            }
        }
    }

    void ClickedOnMinimap()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, Input.mousePosition, null, out localPoint);

        normalized = new Vector2((localPoint.x / rect.rect.width) + 0.5f, (localPoint.y / rect.rect.height) + 0.5f);

        worldPoint = minimapCamera.ViewportToWorldPoint(new Vector3(normalized.x, normalized.y, minimapCamera.nearClipPlane));

        worldPoint.z = mainCamera.transform.position.z;
        mainCamera.transform.position = worldPoint;
    }
    }
}
