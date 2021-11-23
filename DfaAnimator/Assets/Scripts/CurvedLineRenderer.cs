using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

// The base for this code is from from https://forum.unity.com/threads/easy-curved-line-renderer-free-utility.391219/

[RequireComponent(typeof(LineRenderer))]
public class CurvedLineRenderer : MonoBehaviour
{
	//PUBLIC
	public float lineSegmentSize = 0.15f;
	public float lineWidth = 0.1f;
	public Transform source;
	public Transform destination;
	public List<string> characters;
	public TextMeshPro textMesh;
	public GameObject curvedLinePointPrefab;
	public Transform arrowheadTransform;
	[Header("Gizmos")]
	public bool showGizmos = true;
	public float gizmoSize = 0.1f;
	public Color gizmoColor = new Color(1, 0, 0, 0.5f);
	public Vector3[] smoothedPoints;
	//PRIVATE
	private CurvedLinePoint[] linePoints = new CurvedLinePoint[0];
	private Vector3[] linePositions = new Vector3[0];
	private Vector3[] linePositionsOld = new Vector3[0];

	void Start()
	{
		characters = new List<string>();
	}

	// Update is called once per frame
	public void Update()
	{
		GetPoints();
		SetPointsToLine();
		if (name == "initial")
        {
			return;
        }
		else if (source.parent.name != destination.name)
		{
			Vector3 midPoint3D = Vector3.Lerp(source.position, destination.position, 0.5f);
			Vector2 midPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, midPoint3D);
			Vector2 sourcePoint = RectTransformUtility.WorldToScreenPoint(Camera.main, source.position);
			Vector2 destinationPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, destination.position);

			Vector2 textOffset;
			float textAngle;
			if (destinationPoint.x > sourcePoint.x)
			{
				textAngle = (Mathf.Atan2(destinationPoint.y - sourcePoint.y, destinationPoint.x - sourcePoint.x)) * 180f / Mathf.PI;
				textOffset = (Vector3.Cross(sourcePoint - destinationPoint, Vector3.forward).normalized * -9);

			}
			else
			{
				textAngle = ((Mathf.Atan2(destinationPoint.y - sourcePoint.y, destinationPoint.x - sourcePoint.x)) * 180f / Mathf.PI) + 180;
				textOffset = (Vector3.Cross(destinationPoint - sourcePoint, Vector3.forward).normalized * 10);
			}

			textMesh.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, textAngle);
			textMesh.GetComponent<RectTransform>().position = midPoint3D + new Vector3(textOffset.x,textOffset.y,0);
		}
		else
        {
			Vector3 midPoint3D = linePoints[2].transform.position + new Vector3(7,0,0);
			Vector2 midPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, midPoint3D);
			Vector2 linePoint1 = RectTransformUtility.WorldToScreenPoint(Camera.main, linePoints[1].transform.position);
			Vector2 linePoint3 = RectTransformUtility.WorldToScreenPoint(Camera.main, linePoints[3].transform.position);

			float textAngle = ((Mathf.Atan2(linePoint3.y - linePoint1.y, linePoint3.x - linePoint1.x)) * 180f / Mathf.PI) + 90;
			textMesh.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, textAngle);
			textMesh.GetComponent<RectTransform>().position = midPoint3D;
		}
	}

	void GetPoints()
	{
		//find curved points in children
		linePoints = GetComponentsInChildren<CurvedLinePoint>();
		if (name == "initial")
		{
			linePoints[0].transform.position = source.position;
			linePoints[1].transform.position = Vector3.Lerp(source.position,destination.position,.5f);
			linePoints[2].transform.position = destination.position;
			Vector3 arrowHeadOffset = (linePoints[2].transform.position - linePoints[1].transform.position).normalized * -8;
			arrowheadTransform.position = linePoints[2].transform.position + arrowHeadOffset + (Vector3.back * 10);
			arrowheadTransform.rotation = Quaternion.Euler(0, 0, 180);
		}
		else if (source.parent.name == destination.name)
        {
			linePoints[0].transform.position = source.position;
			linePoints[1].transform.position = source.position + new Vector3(5, 8, 0);
			linePoints[2].transform.position = source.position + new Vector3(15, 0, 0);
			linePoints[3].transform.position = source.position + new Vector3(5, -8, 0);
			linePoints[4].transform.position = source.position;
			Vector3 arrowHeadOffset = (linePoints[4].transform.position - linePoints[3].transform.position).normalized * -8;
			Vector2 linePoint3 = RectTransformUtility.WorldToScreenPoint(Camera.main, linePoints[1].transform.position);
			Vector2 linePoint4 = RectTransformUtility.WorldToScreenPoint(Camera.main, linePoints[2].transform.position);
			float angle = ((Mathf.Atan2(linePoint4.y - linePoint3.y, linePoint4.x - linePoint3.x)) * 180f / Mathf.PI);

			arrowheadTransform.position = linePoints[4].transform.position + arrowHeadOffset + (Vector3.back * 10);
			arrowheadTransform.rotation = Quaternion.Euler(0, 0, angle);
		}
		else
        {
			linePoints[0].transform.position = source.position;


			Vector3 offset = Vector3.Cross(destination.position - source.position, Vector3.forward).normalized * 5f;


			linePoints[1].transform.position = Vector3.Lerp(source.position, destination.position, 0.5f) + offset;
			linePoints[2].transform.position = destination.position;
			Vector3 arrowHeadOffset = (linePoints[2].transform.position - linePoints[1].transform.position).normalized * -8;
			Vector2 linePoint1 = RectTransformUtility.WorldToScreenPoint(Camera.main, linePoints[1].transform.position);
			Vector2 linePoint2 = RectTransformUtility.WorldToScreenPoint(Camera.main, linePoints[2].transform.position);
			
			float angle = ((Mathf.Atan2(linePoint2.y - linePoint1.y, linePoint2.x - linePoint1.x)) * 180f / Mathf.PI) - 180;
			arrowheadTransform.position = linePoints[2].transform.position + arrowHeadOffset + (Vector3.back * 10);
			arrowheadTransform.rotation = Quaternion.Euler(0, 0, angle);

		}
		//add positions
		linePositions = new Vector3[linePoints.Length];
		for (int i = 0; i < linePoints.Length; i++)
		{
			linePositions[i] = linePoints[i].transform.position;
		}
	}

	void SetPointsToLine()
	{
		//create old positions if they dont match
		if (linePositionsOld.Length != linePositions.Length)
		{
			linePositionsOld = new Vector3[linePositions.Length];
		}

		//check if line points have moved
		bool moved = false;
		for (int i = 0; i < linePositions.Length; i++)
		{
			//compare
			if (linePositions[i] != linePositionsOld[i])
			{
				moved = true;
			}
		}

		//update if moved
		if (moved == true)
		{
			LineRenderer line = this.GetComponent<LineRenderer>();

			//get smoothed values
			smoothedPoints = LineSmoother.SmoothLine(linePositions, lineSegmentSize);

			//set line settings
			line.positionCount = smoothedPoints.Length;
			

			line.SetPositions(smoothedPoints);
			line.startWidth = lineWidth;
			line.endWidth = lineWidth;
		}
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

	public void AddCurvedLinePoint()
    {
		Instantiate(curvedLinePointPrefab, transform);	
    }

	//void OnDrawGizmosSelected()
	//{
	//	Update();
	//}

	//void OnDrawGizmos()
	//{
	//	if (linePoints.Length == 0)
	//	{
	//		GetPoints();
	//	}

	//	//settings for gizmos
	//	foreach (CurvedLinePoint linePoint in linePoints)
	//	{
	//		linePoint.showGizmo = showGizmos;
	//		linePoint.gizmoSize = gizmoSize;
	//		linePoint.gizmoColor = gizmoColor;
	//	}
	//}
}
