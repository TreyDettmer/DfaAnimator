using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this code is taken from https://forum.unity.com/threads/implement-a-drag-and-drop-script-with-c.130515/

[RequireComponent(typeof(Renderer))]
class DragTransform : MonoBehaviour
{
    private Color mouseOverColor = Color.blue;
    private Color originalColor = Color.yellow;
    private bool dragging = false;
    private float distance;
    private Vector3 startDist;

    void OnMouseDown()
    {
        distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        dragging = true;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 rayPoint = ray.GetPoint(distance);
        startDist = transform.position - rayPoint;
        GetComponent<Renderer>().material.color = mouseOverColor;
    }

    void OnMouseUp()
    {
        dragging = false;
        GetComponent<Renderer>().material.color = originalColor;
    }

    void Update()
    {
        if (dragging)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 rayPoint = ray.GetPoint(distance);
            transform.position = rayPoint + startDist;
        }
    }
}
