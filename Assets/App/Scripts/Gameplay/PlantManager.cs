using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float growSpeed;
    [SerializeField] float pointsSpacing;

    public List<Vector3> pathPoints = new();
    Vector3 currentPoint;

    [Header("References")]
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform startingPoint;

    Camera cam;

    //[Header("RSO")]
    //[Header("RSE")]
    //[Header("RSF")]

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if(lineRenderer.positionCount <= 0)
        {
            pathPoints.Add(startingPoint.position);
            lineRenderer.positionCount = 1;
            lineRenderer.SetPosition(0, startingPoint.position);
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            if (Vector3.Distance(currentPoint, mousePos) >= pointsSpacing)
            {
                Vector3 dir = mousePos - currentPoint;
                currentPoint += dir.normalized * growSpeed * Time.deltaTime;

                if (pathPoints.Count > 0 && Vector3.Distance(pathPoints[^1], currentPoint) >= pointsSpacing)
                {
                    pathPoints.Add(currentPoint);
                }

                lineRenderer.positionCount = pathPoints.Count;
                lineRenderer.SetPositions(pathPoints.ToArray());
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentPoint);
            }
        }
    }
}