using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildLineRenderer : MonoBehaviour
{
    LineRenderer lineRenderer;
    public Transform destination;
    public Transform source;
    public string character;
    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPosition(0, source.position);
        lineRenderer.SetPosition(1, destination.position);
    }
}
