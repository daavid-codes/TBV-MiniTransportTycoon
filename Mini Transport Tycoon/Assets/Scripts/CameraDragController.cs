using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDragController : MonoBehaviour
{

    /*[SerializeField], ha azt akarod, hogy csak ez a class lássa, és látható legyen inspectorban; egyébként mindenki láthatja*/
    private float dragSpeed = 2.0f;
    private Vector3 dragOrigin;

    private float zoomSpeed = 4.0f;
    private float minZoom = 2.0f;
    private float maxZoom = 10.0f;
    private float scroll;

    // Update is called once per frame
    void Update()
    {
        HandleDrag();
        HandleZoom();
    }

    void HandleDrag()
    {
        if (Input.GetMouseButtonDown(2))//a "2" a görgetögomb; ha a jobbklikk kell, akkor "1"
        {
            dragOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButton(2))//amíg le van nyomva
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
}
