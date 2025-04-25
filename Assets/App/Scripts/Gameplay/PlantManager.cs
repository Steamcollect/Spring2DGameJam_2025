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

    [Space(10)]
    [SerializeField] float zigzagAmplitude = .1f;
    [SerializeField] float zigzagFrequency = 1;
    [SerializeField] float zigzagSpeed = 2;

    [Header("References")]
    [SerializeField] LineRenderer lineRenderer;
    [SerializeField] Transform startingPoint;

    HashSet<Collider2D> activeTriggers = new();

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
        MouseLeftClick();
        MouseRightClick();
    }

    void MouseLeftClick()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 desirePos = GetPositionWithCollider(currentPoint, mousePos);

            if (currentDist >= maxDistance) return;

            if (Vector2.Distance(currentPoint, desirePos) >= .01f)
            {
                Vector2 dir = desirePos - currentPoint;
                Vector2 movement = dir.normalized * growSpeed * Time.deltaTime;

                currentPoint += movement;
                currentDist += movement.sqrMagnitude;
                currentDist = currentDist > maxDistance ? maxDistance : currentDist;

                CheckCollision();

                if (pathPoints.Count > 0 && Vector2.Distance(pathPoints[^1].position, currentPoint) >= pointsSpacing)
                {
                    pathPoints.Add(new PathPoint(currentPoint, currentDist));
                }

                UpdatePlantVisual();
            }
        }

    }
    void MouseRightClick()
    {
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

    Vector2 GetPositionWithCollider(Vector2 a, Vector2 b)
    {
        RaycastHit2D hit = Physics2D.Linecast(a, b);

        if (hit.collider != null && !hit.collider.isTrigger)
        {
            return hit.point + hit.normal * .05f;
        }

        return b;
    }

    void CheckCollision()
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(currentPoint);
        HashSet<Collider2D> newTriggers = new();

        foreach (var hit in hits)
        {
            if (!hit.isTrigger) continue;

            newTriggers.Add(hit);

            if (!activeTriggers.Contains(hit))
            {
                _OnTriggerEnter2D(hit);
            }
        }

        foreach (var oldTrigger in activeTriggers)
        {
            if (!newTriggers.Contains(oldTrigger))
            {
                _OnTriggerExit2D(oldTrigger);
            }
        }

        activeTriggers = newTriggers;
    }

    void _OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Trigger ENTER : " + col.name);
    }

    void _OnTriggerExit2D(Collider2D col)
    {
        Debug.Log("Trigger EXIT : " + col.name);
    }

    void UpdatePlantVisual()
    {
        lineRenderer.positionCount = pathPoints.Count + 1;
        lineRenderer.SetPositions(GetZigZagPathPositions());
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

    Vector3[] GetZigZagPathPositions()
    {
        Vector3[] basePositions = GetPathPointPositions();
        Vector3[] zigzagPositions = new Vector3[basePositions.Length];

        float time = Time.time * zigzagSpeed; // <- on anime avec le temps ici

        for (int i = 0; i < basePositions.Length; i++)
        {
            Vector3 pos = basePositions[i];

            if (i > 0)
            {
                Vector2 dir = (basePositions[i] - basePositions[i - 1]).normalized;
                Vector2 normal = new Vector2(-dir.y, dir.x); // perpendiculaire

                // Décalage dynamique animé avec le temps
                float offset = Mathf.Sin(i * zigzagFrequency + time) * zigzagAmplitude;

                pos += (Vector3)(normal * offset);
            }

            zigzagPositions[i] = pos;
        }

        return zigzagPositions;
    }
}