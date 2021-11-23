using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChildLineRenderer : MonoBehaviour
{
    LineRenderer lineRenderer;
    public Transform destination;
    public Transform source;
    public List<string> characters;
    public TextMeshProUGUI textMesh;
    // Start is called before the first frame update
    void Start()
    {
        characters = new List<string>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer.SetPosition(0, source.position);
        lineRenderer.SetPosition(1, destination.position);
        Vector3 midPoint3D = Vector3.Lerp(source.position, destination.position, 0.5f);
        Vector2 midPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, midPoint3D);
        Vector2 sourcePoint = RectTransformUtility.WorldToScreenPoint(Camera.main, source.position);
        Vector2 destinationPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, destination.position);
        
        Vector2 textOffset;
        float textAngle;
        if (destinationPoint.x > sourcePoint.x)
        {
            textAngle = (Mathf.Atan2(destinationPoint.y - sourcePoint.y, destinationPoint.x - sourcePoint.x)) * 180f / Mathf.PI;
            textOffset = (Vector3.Cross(sourcePoint - destinationPoint, Vector3.forward).normalized * 20);
            
        }
        else
        {
            textAngle = ((Mathf.Atan2(destinationPoint.y - sourcePoint.y, destinationPoint.x - sourcePoint.x)) * 180f / Mathf.PI) + 180;
            textOffset = (Vector3.Cross(destinationPoint - sourcePoint, Vector3.forward).normalized * 20);
        }

        textMesh.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, textAngle);
        textMesh.GetComponent<RectTransform>().anchoredPosition = midPoint + textOffset;
    }

    public void UpdateCharacters(string newCharacter)
    {
        characters.Add(newCharacter);
        textMesh.text = "";
        for (int i = 0; i < characters.Count; i++)
        {
            if (i < characters.Count - 1)
            {
                textMesh.text += characters[i] + ", ";
            }
            else
            {
                textMesh.text += characters[i];
            }
        }
    }
}
