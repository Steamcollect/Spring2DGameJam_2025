using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BranchVisual : MonoBehaviour
{
    [SerializeField, Range(0, 1)] float growth;

    [Header("Branche")]
    [SerializeField] Vector2 length;
    [SerializeField] Vector2Int pointsCount;
    [SerializeField] Vector2 amplitude;
    [SerializeField] Vector2 frequency;

    [Space(5)]
    [SerializeField] Vector2 whidth;

    Vector3[] pathPoints;

    [System.Serializable]
    public struct Leaf
    {
        public Vector3 position;
        public float angle;

        public float positionOnGrowth;

        public GameObject[] leaves;
    }

    [Header("Leafs")]
    [SerializeField] GameObject leafPrefab;

    [Header("References")]
    [SerializeField] LineRenderer branchRenderer;

    public void Setup(float heightFactor, Material mat)
    {
        GenerateWavyPath(heightFactor);
        branchRenderer.material = mat;
    }

    public void UpdateVisual(float currentGrowth)
    {
        int totalPoints = pathPoints.Length;
        int visiblePoints = Mathf.Max(1, Mathf.RoundToInt(totalPoints * currentGrowth));

        branchRenderer.widthMultiplier = Mathf.Lerp(whidth.x, whidth.y, growth);
        branchRenderer.positionCount = visiblePoints;

        for (int i = 0; i < visiblePoints; i++)
        {
            branchRenderer.SetPosition(i, pathPoints[i]);
        }
    }

    bool invertWaveDirection = false;
    void GenerateWavyPath(float heightFactor)
    {
        float totalLength = Mathf.Lerp(length.y, length.x, heightFactor); // Plus on est haut, plus c'est petit
        int pointsCount = Random.Range(this.pointsCount.x, this.pointsCount.y);
        float amplitude = Random.Range(this.amplitude.x, this.amplitude.y);
        float frequency = Random.Range(this.frequency.x, this.frequency.y);

        Vector3[] path = new Vector3[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            float t = (float)i / (pointsCount - 1);
            float distanceAlong = t * totalLength;

            Vector3 basePos = Vector2.up * distanceAlong;

            invertWaveDirection = Random.value > .5f;
            float waveOffset = Mathf.Sin(t * frequency * Mathf.PI * 2) * (amplitude * (invertWaveDirection ? -1f : 1f));

            path[i] = basePos + Vector3.right * waveOffset;
        }

        pathPoints = path;
    }
}