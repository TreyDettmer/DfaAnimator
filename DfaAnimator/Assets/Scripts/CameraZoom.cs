using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    public float maxSize = 70f;
    public float minSize = 30f;
    public float zoomSpeed = 1f;
    private Camera cam;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        float scrollValue = Input.GetAxis("Mouse ScrollWheel");
        cam.orthographicSize = Mathf.Max(minSize, Mathf.Min(maxSize, cam.orthographicSize + (scrollValue * zoomSpeed)));
    }
}
