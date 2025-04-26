using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BranchVisual : MonoBehaviour
{
    [SerializeField, Range(0, 1), ContextMenuItem("Generate new path", "GenerateNewPath")] float growth;

    [Header("Branche")]
    [SerializeField] Vector2 length;
    [SerializeField] Vector2Int pointsCount;
    [SerializeField] Vector2 amplitude;
    [SerializeField] Vector2 frequency;

    [Space(5)]
    [SerializeField] Vector2 whidth;

    Vector3[] pathPoints;

    [Header("Branche")]
    [SerializeField] Vector2 leafCount;
    [SerializeField] Vector2 leavesCount;
    [SerializeField] Vector2 branchRot;
    public Leaf[] leafsPoints;

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

    void Start()
    {
        Material mat = Instantiate(branchRenderer.material);
        branchRenderer.material = mat;
    }

    public void Setup()
    {
        GenerateWavyPath();
        GenerateLeafs();
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

    void GenerateLeafs()
    {
        if (pathPoints == null || pathPoints.Length == 0)
            return;

        int leafCountValue = Random.Range((int)leafCount.x, (int)leafCount.y);
        List<Leaf> generatedLeafs = new List<Leaf>();

        float currentGrowth = 0f;
        float growthStep = 1f / leafCountValue;

        for (int i = 0; i < leafCountValue; i++)
        {
            Leaf leaf = new Leaf();

            // Espacement
            float spacingMultiplier = Mathf.Lerp(1f, 2f, currentGrowth);
            currentGrowth += growthStep * spacingMultiplier;
            currentGrowth = Mathf.Clamp01(currentGrowth);

            leaf.positionOnGrowth = currentGrowth;

            // Trouver la position sur les pathPoints
            float targetDistance = currentGrowth * (pathPoints.Length - 1);
            int startIndex = Mathf.FloorToInt(targetDistance);
            int endIndex = Mathf.Clamp(startIndex + 1, 0, pathPoints.Length - 1);
            float t = targetDistance - startIndex;

            Vector3 position = Vector3.Lerp(pathPoints[startIndex], pathPoints[endIndex], t);
            leaf.position = position;

            // Calcul de la direction du chemin
            Vector3 pathDirection = (pathPoints[endIndex] - pathPoints[startIndex]).normalized;

            // Trouver la direction perpendiculaire (droite ou gauche)
            Vector3 perpendicular = new Vector3(-pathDirection.y, pathDirection.x, 0);

            // Random droite/gauche
            int side = Random.value < 0.5f ? -1 : 1;
            perpendicular *= side;

            // Calcul de l'angle en degrés
            float angle = Mathf.Atan2(perpendicular.y, perpendicular.x) * Mathf.Rad2Deg;

            // Ajouter un petit random avec branchRot
            float randomRot = Random.Range(branchRot.x, branchRot.y);
            leaf.angle = angle + randomRot;

            generatedLeafs.Add(leaf);

            if (currentGrowth >= 1f)
                break;
        }

        leafsPoints = generatedLeafs.ToArray();
    }

    void GenerateWavyPath()
    {
        float totalLength = Random.Range(length.x, length.y);
        int pointsCount = Random.Range(this.pointsCount.x, this.pointsCount.y);
        float amplitude = Random.Range(this.amplitude.x, this.amplitude.y);
        float frequency = Random.Range(this.frequency.x, this.frequency.y);

        Vector3[] path = new Vector3[pointsCount];

        for (int i = 0; i < pointsCount; i++)
        {
            float t = (float)i / (pointsCount - 1);
            float distanceAlong = t * totalLength;

            Vector3 basePos = Vector2.up * distanceAlong;

            float waveOffset = Mathf.Sin(t * frequency * Mathf.PI * 2) * amplitude;

            path[i] = basePos + Vector3.right * waveOffset;
        }

        pathPoints = path;
    }

    void GenerateNewPath()
    {
        GenerateWavyPath();
        GenerateLeafs();
    }

    private void OnValidate()
    {
        //if(pathPoints.Length > 0 && leafsPoints.Length > 0)
        //{
        //    UpdateVisual(growth);
        //}
    }
}