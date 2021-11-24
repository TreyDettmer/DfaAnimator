using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class State : MonoBehaviour
{

    public bool isAccepting = false;
    public bool isInitial = false;
    public List<(State,string)> paths;
    public List<string> pathsVisualizer;
    public TextMeshProUGUI label;
    public GameObject curvedLineRendererObject;
    public List<CurvedLineRenderer> childLineRenderers;
    public Transform initialArrowTransform;
    public Mesh acceptingStateMesh;
   

    /// <summary>
    /// Initializes the state
    /// </summary>
    /// <param name="isAccepting">whether this state is an accepting state</param>
    /// <param name="isInitial">whether this state is a rejecting state</param>
    public void Initialize(bool isAccepting, bool isInitial)
    {
        this.isAccepting = isAccepting;
        this.isInitial = isInitial;
        paths = new List<(State, string)>();
        pathsVisualizer = new List<string>();
        label.text = name;
        childLineRenderers = new List<CurvedLineRenderer>();

        if (isAccepting)
        {
            GetComponent<MeshFilter>().mesh = acceptingStateMesh;
        }

        if (isInitial)
        {
            // add the path going into the initial state
            CurvedLineRenderer child = Instantiate(curvedLineRendererObject, transform).GetComponent<CurvedLineRenderer>();
            child.name = "initial";
            child.source = initialArrowTransform;
            child.destination = transform;
        }
    }

    /// <summary>
    /// Reads a given character and returns the state to transition to
    /// </summary>
    /// <param name="character">the character to read</param>
    /// <returns>the state to transition to</returns>
    public State Read(string character)
    {
        if (character.Equals(""))
        {
            return this;
        }
        foreach ((State,string) path in paths)
        {
            if (path.Item2.Equals(character))
            {
                return path.Item1;
            }
        }
        return null;
    }

    /// <summary>
    /// Adds a transition from this state to the provided state on the provided character
    /// </summary>
    /// <param name="state">the state to transition to</param>
    /// <param name="character">the character for this transition</param>
    public void AddPath(State state, string character)
    {
        paths.Add((state, character));
        pathsVisualizer.Add(character + " -> " + state.name);


        // If a child line renderer already exists to this state, add the new character to that path's character list.
        // Otherwise, create a new child line renderer

        int curvedLineRendererIndex = childLineRenderers.FindIndex(child => child.name.Contains(state.name));
        if (curvedLineRendererIndex != -1)
        {
            childLineRenderers[curvedLineRendererIndex].UpdateCharacters(character);
        }
        else
        {
            CurvedLineRenderer child = Instantiate(curvedLineRendererObject, transform).GetComponent<CurvedLineRenderer>();
            child.transform.position = transform.position;
            child.name = character + " -> " + state.name;
            child.source = child.transform;
            child.destination = state.transform;
            if (state == this)
            {
                // add points for self loop path
                child.AddCurvedLinePoint();
                child.AddCurvedLinePoint();
            }
            child.UpdateCharacters(character);
            childLineRenderers.Add(child);
        }

        

    }
    /// <summary>
    /// Returns the path to the given state
    /// </summary>
    /// <param name="destinationState">the state being transitioned to</param>
    /// <returns>the path to the given state</returns>
    public CurvedLineRenderer GetPathToState(State destinationState)
    {
        return childLineRenderers.Find(child => child.name.Contains(destinationState.name));
    }


}
