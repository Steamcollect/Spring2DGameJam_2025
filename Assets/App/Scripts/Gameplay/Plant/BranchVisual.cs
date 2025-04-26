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
    [SerializeField] Vector2 leafRot;
    [SerializeField] float maxLeafSize;
    Leaf[] leafsPoints;


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

        // --- Mise à jour des feuilles ---
        if (leafsPoints != null)
        {
            foreach (var leaf in leafsPoints)
            {
                foreach (var leafGO in leaf.leaves)
                {
                    if (leafGO == null) continue;

                    // Le facteur de croissance de la feuille dépend de sa position sur la branche
                    float leafGrowth = Mathf.InverseLerp(leaf.positionOnGrowth - 0.1f, leaf.positionOnGrowth + 0.1f, currentGrowth);
                    leafGrowth = Mathf.Clamp01(leafGrowth);

                    // Échelle de la feuille, limitée par maxLeafSize
                    leafGO.transform.localScale = Vector3.one * (leafGrowth * maxLeafSize);

                    // Active ou désactive selon la progression
                    leafGO.SetActive(leafGrowth > 0f);
                }
            }
        }
    }



    void GenerateLeafs()
    {
        int leafNumber = Random.Range((int)leafCount.x, (int)leafCount.y + 1);
        leafsPoints = new Leaf[leafNumber];

        for (int i = 0; i < leafNumber; i++)
        {
            Leaf leaf = new Leaf();

            // Génère une position sur la croissance (0 = base, 1 = sommet)
            leaf.positionOnGrowth = Random.Range(0f, 1f);

            // Trouver l'index sur le path
            int pathIndex = Mathf.Clamp(Mathf.RoundToInt(leaf.positionOnGrowth * (pathPoints.Length - 1)), 0, pathPoints.Length - 1);

            // Prend la position correspondante
            Vector3 basePos = pathPoints[pathIndex];
            leaf.position = basePos;

            // Génère un angle aléatoire dans les bornes
            leaf.angle = Random.Range(leafRot.x, leafRot.y);

            // Instancie la feuille
            GameObject leafGO = Instantiate(leafPrefab, transform);
            leafGO.transform.localPosition = basePos;
            leafGO.transform.localRotation = Quaternion.Euler(0, 0, leaf.angle);
            leafGO.SetActive(true);

            // Stocke la feuille dans un tableau pour pouvoir la retrouver après
            leaf.leaves = new GameObject[] { leafGO };

            // Enregistre
            leafsPoints[i] = leaf;
        }
    }


    bool invertWaveDirection = false;
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

            invertWaveDirection = Random.value > .5f;
            float waveOffset = Mathf.Sin(t * frequency * Mathf.PI * 2) * (amplitude * (invertWaveDirection ? -1f : 1f));

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