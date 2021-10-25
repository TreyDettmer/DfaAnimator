using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class State : MonoBehaviour
{

    private bool isAccepting = false;
    private bool isInitial = false;
    public List<(State,string)> paths;
    public List<string> pathsVisualizer;
    public TextMeshProUGUI label;
    public GameObject childLineRendererObject;
    public List<ChildLineRenderer> childLineRenderers;
    public Transform initialArrowTransform;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Initialize(bool isAccepting, bool isInitial)
    {
        this.isAccepting = isAccepting;
        this.isInitial = isInitial;
        paths = new List<(State, string)>();
        pathsVisualizer = new List<string>();
        label.text = name;
        childLineRenderers = new List<ChildLineRenderer>();
        if (isInitial)
        {

            ChildLineRenderer child = Instantiate(childLineRendererObject, transform).GetComponent<ChildLineRenderer>();
            child.name = "initial";
            child.source = initialArrowTransform;
            child.destination = transform;
            child.GetComponent<LineRenderer>().startColor = Color.black;
            child.GetComponent<LineRenderer>().endColor = Color.black;
        }
    }

    public State Read(string character)
    {
        return null;
    }

    public void AddPath(State state, string character)
    {
        paths.Add((state, character));
        pathsVisualizer.Add(character + " -> " + state.name);
        if (state.name != name)
        {
            ChildLineRenderer child = Instantiate(childLineRendererObject, transform).GetComponent<ChildLineRenderer>();
            child.transform.position = transform.position;
            child.name = character + " -> " + state.name;
            child.source = child.transform;
            child.destination = state.transform;
            child.character = character;
            childLineRenderers.Add(child);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
