using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MiniTransportTycoon
{
    public class CameraDragController : MonoBehaviour
    {

    private float dragSpeed = 2.0f;
    private Vector3 dragOrigin;

    private float zoomSpeed = 4.0f;
    private float minZoom = 2.0f;
    private float maxZoom = 10.0f;
    private float scroll;

    private float minX = -80f;
    private float maxX = 80f;
    private float minY = -48f;
    private float maxY = 40f;
    private float clampedX;
    private float clampedY;

    // Update is called once per frame
    void Update()
    {
        HandleDrag();
        HandleZoom();
        ClampCamera();
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = (Camera.main.ScreenToWorldPoint(dragOrigin) - Camera.main.ScreenToWorldPoint(Input.mousePosition)) * dragSpeed;

            transform.position += difference;
            dragOrigin = Input.mousePosition;
        }
    }

    void HandleZoom()
    {
        scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize -= scroll * zoomSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
    }

    void ClampCamera() {
        clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        clampedY = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
    }
}
