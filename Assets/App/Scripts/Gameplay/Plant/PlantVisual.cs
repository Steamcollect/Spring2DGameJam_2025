using System.Collections.Generic;
using UnityEngine;

public class PlantVisual : MonoBehaviour
{
    [SerializeField] Transform startingPoint;

    [Header("Visual")]
    [SerializeField] float zigzagAmplitude = .1f;
    [SerializeField] float zigzagFrequency = 1;
    [SerializeField] float zigzagSpeed = 2;

    [Space(10)]
    [SerializeField] Gradient plantGradient;

    [SerializeField] Color outlineColor;

    [Space(10)]
    [SerializeField] float plantThickness;
    [SerializeField] float outlineThickness;

    [Header("Leaves")]
    [SerializeField] BranchVisual branchPrefab;
    Queue<Branch> branchQueue = new();
    [Space(10)]
    [SerializeField] Vector2 branchSpacing;

    List<Branch> branchs = new();


    class Branch
    {
        public BranchVisual leaf;
        public float positionFromMaxDistance;

        public Branch(BranchVisual leaf, float positionFromMaxDistance)
        {
            this.leaf = leaf;
            this.positionFromMaxDistance = positionFromMaxDistance;
        }
    }

    [Header("References")]
    [SerializeField] LineRenderer mainRenderer;
    [SerializeField] LineRenderer outlineRenderer;

    [Header("RSO")]
    [SerializeField] RSO_PlantDistance rsoPlantDist;

    private void Start()
    {
        mainRenderer.material.color = plantGradient.Evaluate(0f);
        outlineRenderer.material.color = outlineColor;

        mainRenderer.widthMultiplier = plantThickness;
        outlineRenderer.widthMultiplier = outlineThickness;

        mainRenderer.positionCount = 1;
        mainRenderer.SetPosition(0, startingPoint.position);

        for (int i = 0; i < 30; i++)
        {
            branchQueue.Enqueue(CreateBranch());
        }
    }

    public void UpdatePlantVisual(Vector3[] pathPoints, Vector3 currentPoint, float maxDistance)
    {
        Vector3[] positions = GetZigZagPathPositions(pathPoints);

        outlineRenderer.positionCount = positions.Length + 1;
        mainRenderer.positionCount = positions.Length + 1;

        Vector3 offset = Vector3.forward * 0.01f; // Petit décalage en Z

        for (int i = 0; i < positions.Length; i++)
        {
            outlineRenderer.SetPosition(i, positions[i] + offset); // décalé
            mainRenderer.SetPosition(i, positions[i]);             // normal
        }

        outlineRenderer.SetPosition(outlineRenderer.positionCount - 1, currentPoint + offset);
        mainRenderer.SetPosition(mainRenderer.positionCount - 1, currentPoint);

        mainRenderer.material.color = plantGradient.Evaluate(rsoPlantDist.Value / maxDistance);

        UpdateBranches(positions, maxDistance);
    }

    Vector3[] GetZigZagPathPositions(Vector3[] pathPoints)
    {
        Vector3[] basePositions = pathPoints;
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

    void UpdateBranches(Vector3[] pathPositions, float maxDistance)
    {
        float plantRatio = rsoPlantDist.Value / maxDistance;

        // Désactiver et recycler les branches trop loin
        for (int i = branchs.Count - 1; i >= 0; i--)
        {
            Branch b = branchs[i];
            if (b.positionFromMaxDistance > plantRatio)
            {
                b.leaf.gameObject.SetActive(false);
                branchQueue.Enqueue(b);
                branchs.RemoveAt(i);
            }
        }

        // Ajout de nouvelles branches au bon espacement
        float nextSpacing = Random.Range(branchSpacing.x, branchSpacing.y);
        float totalLength = 0f;

        for (int i = 1; i < pathPositions.Length; i++)
        {
            float segmentLength = Vector3.Distance(pathPositions[i - 1], pathPositions[i]);
            totalLength += segmentLength;

            while (totalLength >= nextSpacing)
            {
                float t = (nextSpacing - (totalLength - segmentLength)) / segmentLength;
                Vector3 pos = Vector3.Lerp(pathPositions[i - 1], pathPositions[i], t);

                // Calcul direction locale
                Vector3 dir = (pathPositions[i] - pathPositions[i - 1]).normalized;
                Vector3 perp = new Vector3(-dir.y, dir.x, 0); // perpendiculaire droite

                // Créer la branche
                Branch branch = GetBranch(rsoPlantDist.Value / maxDistance);

                branch.leaf.transform.position = pos;
                branch.leaf.transform.rotation = Quaternion.LookRotation(Vector3.forward, perp); // bien orientée perpendiculaire

                // Préparer le prochain espacement
                nextSpacing += Random.Range(branchSpacing.x, branchSpacing.y);
            }
        }
    }


    Branch GetBranch(float position)
    {
        Branch branch;
        if (branchQueue.Count <= 0)
        {
            branch = CreateBranch();
        }
        else branch = branchQueue.Dequeue();

        branch.positionFromMaxDistance = position;
        branch.leaf.gameObject.SetActive(true);
        branchs.Add(branch);

        return branch;
    }
    Branch CreateBranch()
    {
        BranchVisual branch = Instantiate(branchPrefab, transform);
        branch.gameObject.SetActive(false);
        branch.Setup();

        return new Branch(branch, 0);
    }
}