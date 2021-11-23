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
    private bool isOverlappingState = false;
    public Material yellowMaterial;
    public Material redMaterial;
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
        childLineRenderers = new List<CurvedLineRenderer>();
        if (isAccepting)
        {
            GetComponent<MeshFilter>().mesh = acceptingStateMesh;
        }

        if (isInitial)
        {

            CurvedLineRenderer child = Instantiate(curvedLineRendererObject, transform).GetComponent<CurvedLineRenderer>();
            child.name = "initial";
            child.source = initialArrowTransform;
            child.destination = transform;
        }
    }

    public State Read(string word)
    {
        if (word.Equals(""))
        {
            return this;
        }
        foreach ((State,string) path in paths)
        {
            if (path.Item2.Equals(word))
            {
                return path.Item1;
            }
        }
        return null;
    }

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
    /// Returns the curved line renderer to the given state
    /// </summary>
    /// <param name="destinationState">the state being transitioned to</param>
    /// <returns></returns>
    public CurvedLineRenderer GetLineRenderer(State destinationState)
    {
        return childLineRenderers.Find(child => child.name.Contains(destinationState.name));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 8f);
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].gameObject != gameObject)
                {
                    if (!isOverlappingState)
                    {
                        isOverlappingState = true;
                        StartCoroutine(Flash());
                    }
                    break;
                }
            }
        }

    }


    private void OnTriggerExit(Collider other)
    {

        isOverlappingState = false;
        GetComponent<MeshRenderer>().material = yellowMaterial;
        

    }

    IEnumerator Flash()
    {
        while (isOverlappingState)
        {
            GetComponent<MeshRenderer>().material = redMaterial;
            yield return new WaitForSeconds(.25f);
            GetComponent<MeshRenderer>().material = yellowMaterial;
            yield return new WaitForSeconds(.25f);

        }
        yield return null;
    }
}
