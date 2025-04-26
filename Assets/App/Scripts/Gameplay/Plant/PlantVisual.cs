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
    float _branchSpacing;
    [SerializeField, Range(0,1)] float branchGrowthPercentage = .3f;

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
        if(pathPositions.Length > 2)
        {
            if(branchs.Count <= 0)
            {
                SpawnBranch(0, pathPositions[0], Utils.GetPerpendicularBetweenPoints(pathPositions[1], pathPositions[2]));
            }
            else if (Vector2.Distance(branchs[^1].leaf.transform.position, pathPositions[^1]) >= _branchSpacing)
            {
                SpawnBranch(rsoPlantDist.Value / maxDistance, pathPositions[^1], Utils.GetPerpendicularBetweenPoints(pathPositions[^1], pathPositions[^2]));
            }
        }

        List<Branch> newBranches = new();
        foreach (var branch in branchs)
        {
            if(rsoPlantDist.Value / maxDistance < branch.positionFromMaxDistance)
            {
                branchQueue.Enqueue(branch);
                branch.leaf.gameObject.SetActive(false);
            }
            else
            {
                branch.leaf.UpdateVisual(Mathf.Clamp(((rsoPlantDist.Value / maxDistance) - branch.positionFromMaxDistance) / branchGrowthPercentage, 0f, 1f));
                newBranches.Add(branch);
            }
        }
        branchs = newBranches;
    }


    Branch SpawnBranch(float positionOnPlant, Vector2 position, Vector2 normal)
    {
        _branchSpacing = Random.Range(branchSpacing.x, branchSpacing.y);

        Branch branch;
        if (branchQueue.Count <= 0)
        {
            branch = CreateBranch();
        }
        else branch = branchQueue.Dequeue();

        branch.positionFromMaxDistance = positionOnPlant;
        branch.leaf.gameObject.SetActive(true);
        branchs.Add(branch);

        branch.leaf.transform.position = position;
        if (Random.value < .5f) normal = -normal;
        branch.leaf.transform.up = normal;

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