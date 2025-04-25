using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float growSpeed;
    [SerializeField] float ungrowSpeed;

    [Space(5)]
    [SerializeField] float pointsSpacing;
    [SerializeField] float ungrowDetectionDist;

    [Space(10)]
    [SerializeField] float maxDistance;
    float currentDist;

    List<PathPoint> pathPoints = new();
    Vector2 currentPoint;

    [System.Serializable]
    struct PathPoint
    {
        public Vector2 position;
        public float distance;

        public PathPoint(Vector2 position, float distance)
        {
            this.position = position;
            this.distance = distance;
        }
    }

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
        pathPoints.Add(new PathPoint(startingPoint.position, 0));
        lineRenderer.positionCount = 1;
        lineRenderer.SetPosition(0, startingPoint.position);

        currentPoint = startingPoint.position;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

            if(currentDist >= maxDistance) return;

            if (Vector2.Distance(currentPoint, mousePos) >= .01f)
            {
                Vector2 dir = mousePos - currentPoint;
                Vector2 movement = dir.normalized * growSpeed * Time.deltaTime;

                currentPoint += movement;
                currentDist += movement.sqrMagnitude;
                currentDist = currentDist > maxDistance ? maxDistance : currentDist;

                if (pathPoints.Count > 0 && Vector2.Distance(pathPoints[^1].position, currentPoint) >= pointsSpacing)
                {
                    pathPoints.Add(new PathPoint(currentPoint, currentDist));
                }

                UpdatePlantVisual();
            }
        }
        if (Input.GetKey(KeyCode.Mouse1))
        {
            if (pathPoints.Count > 1)
            {
                PathPoint targetPoint = pathPoints[^1];

                Vector2 dir = currentPoint - targetPoint.position;
                Vector2 movement = dir.normalized * ungrowSpeed * Time.deltaTime;

                currentPoint -= movement;
                currentDist -= movement.sqrMagnitude;

                if (Vector2.Distance(currentPoint, targetPoint.position) <= ungrowDetectionDist)
                {
                    pathPoints.RemoveAt(pathPoints.Count - 1);
                    currentPoint = targetPoint.position;
                    currentDist = targetPoint.distance;
                }
            }
            else
            {
                currentDist = 0;
                currentPoint = startingPoint.position;

            }

            UpdatePlantVisual();
        }
    }

    void UpdatePlantVisual()
    {
        lineRenderer.positionCount = pathPoints.Count + 1;
        lineRenderer.SetPositions(GetPathPointPositions());
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentPoint);
    }

    Vector3[] GetPathPointPositions()
    {
        Vector3[] positions = new Vector3[pathPoints.Count];
        for (int i = 0; i < pathPoints.Count; i++)
        {
            positions[i] = pathPoints[i].position;
        }
        return positions;
    }
}