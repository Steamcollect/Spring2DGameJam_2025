using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float growSpeed;
    [SerializeField] float ungrowSpeed;

    [Space(5)]
    [SerializeField] float pointsSpacing;
    [SerializeField] float ungrowDetectionDist;

    [Space(10)]
    [SerializeField] float maxDistance;
    float currentDist;

    List<PathPoint> pathPoints = new();
    Vector3 currentPoint;

    [System.Serializable]
    struct PathPoint
    {
        public Vector3 position;
        public float distance;

        public PathPoint(Vector3 position, float distance)
        {
            this.position = position;
            this.distance = distance;
        }
    }

    [Header("Visual")]
    [SerializeField] float zigzagAmplitude = .1f;
    [SerializeField] float zigzagFrequency = 1;
    [SerializeField] float zigzagSpeed = 2;

    [Space(10)]
    [SerializeField] Color startColor;
    [SerializeField] Color endColor;

    [SerializeField] Color outlineColor;

    [Space(10)]
    [SerializeField] float plantThickness;
    [SerializeField] float outlineThickness;

    [Header("References")]
    [SerializeField] LineRenderer mainRenderer;
    [SerializeField] LineRenderer outlineRenderer;

    [Space(10)]
    [SerializeField] Transform startingPoint;

    HashSet<Collider2D> triggers = new();
    HashSet<Collider2D> exitTriggers = new();

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
        mainRenderer.material.color = startColor;
        outlineRenderer.material.color = outlineColor;

        mainRenderer.widthMultiplier = plantThickness;
        outlineRenderer.widthMultiplier = outlineThickness;

        pathPoints.Add(new PathPoint(startingPoint.position, 0));
        mainRenderer.positionCount = 1;
        mainRenderer.SetPosition(0, startingPoint.position);

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

            if (Vector3.Distance(currentPoint, desirePos) >= .1f)
            {
                Vector2 dir = desirePos - (Vector2)currentPoint;
                Vector2 movement = dir.normalized * growSpeed * Time.deltaTime;

                currentPoint += (Vector3)movement + -Vector3.forward * currentDist * .0001f;
                currentDist += movement.sqrMagnitude;
                currentDist = currentDist > maxDistance ? maxDistance : currentDist;

                CheckCollisionEnter();

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

                currentPoint -= (Vector3)movement;
                currentDist -= movement.sqrMagnitude;

                if (Vector2.Distance(currentPoint, targetPoint.position) <= ungrowDetectionDist)
                {
                    pathPoints.RemoveAt(pathPoints.Count - 1);
                    currentPoint = targetPoint.position;
                    currentDist = targetPoint.distance;
                }

                CheckCollisionExit();
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

    void CheckCollisionEnter()
    {
        exitTriggers.Clear();

        Collider2D[] hits = Physics2D.OverlapPointAll(currentPoint);

        foreach (var hit in hits)
        {
            if (!hit.isTrigger) continue;

            if (!triggers.Contains(hit))
            {
                _OnTriggerEnter2D(hit);
                triggers.Add(hit);
            }
        }
    }
    void CheckCollisionExit()
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(currentPoint);

        foreach(var exitTrigger in exitTriggers)
        {
            if (!hits.Contains(exitTrigger))
            {
                _OnTriggerExit2D(exitTrigger);
                triggers.Remove(exitTrigger);
            }
        }

        exitTriggers = hits.ToHashSet();
    }


    void _OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent(out Triggerable triggerable)) triggerable.OnPlantEnter();

        Debug.Log("Trigger ENTER : " + col.name);
    }

    void _OnTriggerExit2D(Collider2D col)
    {
        if (col.TryGetComponent(out Triggerable triggerable)) triggerable.OnPlantExit();

        Debug.Log("Trigger EXIT : " + col.name);
    }

    void UpdatePlantVisual()
    {
        Vector3[] positions = GetZigZagPathPositions();

        outlineRenderer.positionCount = positions.Length + 1;
        mainRenderer.positionCount = positions.Length + 1;

        Vector3 offset = Vector3.forward * 0.0001f; // Petit décalage en Z

        for (int i = 0; i < positions.Length; i++)
        {
            outlineRenderer.SetPosition(i, positions[i] + offset); // décalé
            mainRenderer.SetPosition(i, positions[i]);             // normal
        }

        outlineRenderer.SetPosition(outlineRenderer.positionCount - 1, currentPoint + offset);
        mainRenderer.SetPosition(mainRenderer.positionCount - 1, currentPoint);

        mainRenderer.material.color = Color.Lerp(startColor, endColor, currentDist / maxDistance);
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