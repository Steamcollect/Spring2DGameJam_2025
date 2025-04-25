using System.Collections.Generic;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float growSpeed;
    [SerializeField] float pointsSpacing;

    float currentDist = 0;

    List<Vector3> pathPoints = new();
    Vector2 currentPoint;

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

    private void Start()
    {
        pathPoints.Add(startingPoint.position);
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, startingPoint.position);

        currentPoint = startingPoint.position;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            if (Vector2.Distance(currentPoint, mousePos) >= pointsSpacing)
            {
                Vector2 dir = mousePos - currentPoint;
                currentPoint += dir.normalized * growSpeed * Time.deltaTime;

                if (pathPoints.Count > 0 && Vector2.Distance(pathPoints[^1], currentPoint) >= pointsSpacing)
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