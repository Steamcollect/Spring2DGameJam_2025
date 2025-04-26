using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlantManager : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float growSpeed;
    [SerializeField] float ungrowSpeed;

    [Space(10)]
    [SerializeField] float pointsSpacing;
    [SerializeField] float ungrowDetectionDist;

    [Space(10)]
    [SerializeField] float maxDistance;

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

    [Space(10)]
    [SerializeField] Transform startingPoint;

    HashSet<Collider2D> triggers = new();
    HashSet<Collider2D> exitTriggers = new();

    Camera cam;

    [Header("References")]
    [SerializeField] PlantVisual plantVisual;

    [Header("RSO")]
    [SerializeField] RSO_PlantDistance rsoPlantDist;

    [Header("RSE")]
    [SerializeField] RSE_OnPlantUngrow rseOnPlantUngrow;

    //[Header("RSF")]

    public static PlantManager instance;

    private void Awake()
    {
        instance = this;

        cam = Camera.main;
    }

    private void Start()
    {
        pathPoints.Add(new PathPoint(startingPoint.position, 0));
        rsoPlantDist.Value = 0;

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
            Vector2 desirePos = GetPositionWithCollider(out Movable movableTouch, currentPoint, mousePos);

            if (rsoPlantDist.Value >= maxDistance) return;

            if (Vector2.Distance(currentPoint, desirePos) >= .1f)
            {
                Vector2 dir = desirePos - (Vector2)currentPoint;
                Vector2 movement = dir.normalized * growSpeed * Time.deltaTime;

                currentPoint += (Vector3)movement + -Vector3.forward * rsoPlantDist.Value * .0005f;
                rsoPlantDist.Value = GetTotalPathDistance();
                rsoPlantDist.Value = rsoPlantDist.Value > maxDistance ? maxDistance : rsoPlantDist.Value;

                CheckCollisionEnter();

                if (pathPoints.Count > 0 && Vector2.Distance(pathPoints[^1].position, currentPoint) >= pointsSpacing)
                {
                    pathPoints.Add(new PathPoint(currentPoint, rsoPlantDist.Value));
                }

                plantVisual.UpdatePlantVisual(GetPathPointPositions(), currentPoint, maxDistance);

                isUngrowEnd = false;
            }
            else if(movableTouch != null)
            {
                Vector2 dir = desirePos - (Vector2)currentPoint;
                movableTouch.Move(dir);
            }
        }

    }

    bool isUngrowEnd = false;
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
                rsoPlantDist.Value = GetTotalPathDistance();

                if (Vector2.Distance(currentPoint, targetPoint.position) <= ungrowDetectionDist)
                {
                    pathPoints.RemoveAt(pathPoints.Count - 1);
                    currentPoint = targetPoint.position;
                    rsoPlantDist.Value = targetPoint.distance;
                }

                CheckCollisionExit();

                rseOnPlantUngrow.Call();
            }
            else
            {
                rsoPlantDist.Value = 0;
                currentPoint = startingPoint.position;

                if (!isUngrowEnd)
                {
                    isUngrowEnd = true;
                    rseOnPlantUngrow.Call();
                }
            }
            plantVisual.UpdatePlantVisual(GetPathPointPositions(), currentPoint, maxDistance);
        }

    }

    Vector2 GetPositionWithCollider(out Movable movableTouch, Vector2 a, Vector2 b)
    {
        RaycastHit2D hit = Physics2D.Linecast(a, b);
        movableTouch = null;

        if (hit.collider != null && !hit.collider.isTrigger)
        {
            if (hit.collider.TryGetComponent(out Movable movable)) movableTouch = movable;

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
            if (!hits.Contains(exitTrigger) && exitTrigger.isTrigger)
            {
                _OnTriggerExit2D(exitTrigger);
                triggers.Remove(exitTrigger);
            }
        }

        exitTriggers = hits.ToHashSet();
    }


    void _OnTriggerEnter2D(Collider2D col)
    {
        if (col.TryGetComponent(out Triggerable triggerable))
        {
            triggerable.isActive = true;
            triggerable.OnPlantEnter?.Invoke();
        }
    }

    void _OnTriggerExit2D(Collider2D col)
    {
        if (col.isTrigger && col.TryGetComponent(out Triggerable triggerable))
        {
            triggerable.isActive = false;
            triggerable.OnPlantExit?.Invoke();
        }
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
    float GetTotalPathDistance()
    {
        float totalDistance = 0f;

        for (int i = 1; i < pathPoints.Count; i++)
        {
            totalDistance += Vector3.Distance(pathPoints[i - 1].position, pathPoints[i].position);
        }

        return totalDistance;
    }

    public void ModifyMaxDistance(float distanceGiven)
    {
        maxDistance += distanceGiven;
    }
}