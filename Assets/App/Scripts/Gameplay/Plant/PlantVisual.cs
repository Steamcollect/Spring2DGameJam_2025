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
    [SerializeField] Gradient outlineGradient;
    [SerializeField] Gradient leafGradient;
    [SerializeField] Gradient branchGradient;

    [Space(10)]
    [SerializeField] float plantThickness;
    [SerializeField] float outlineThickness;

    [Header("Leaf")]
    [SerializeField] float distRequireToSpawnLeaf;
    [SerializeField] AnimationCurve leafDistPerPlantDist;
    [SerializeField] AnimationCurve leafSizePerDistance;

    [SerializeField] SpriteRenderer leafPrefab;

    Queue<SpriteRenderer> leafsQueue = new();
    List<Leaf> leafs = new();

    float lastLeafDist;

    [System.Serializable]
    public struct Leaf
    {
        public SpriteRenderer graphics;
        public float distFromStart;

        public Leaf(SpriteRenderer g, float dist)
        {
            graphics = g;
            distFromStart = dist;
        }
    }

    [Header("Branch")]
    
    [SerializeField] BranchVisual branchPrefab;
    Queue<Branch> branchQueue = new();
    [Space(10)]
    [SerializeField] Vector2 branchSpacing;
    float _branchSpacing;
    [SerializeField, Range(0, 1)] float branchGrowthPercentage = .3f;

    List<Branch> branchs = new();
    Material branchMat;

    class Branch
    {
        public BranchVisual branch;
        public float positionFromMaxDistance;

        public Branch(BranchVisual branch, float positionFromMaxDistance)
        {
            this.branch = branch;
            this.positionFromMaxDistance = positionFromMaxDistance;
        }
    }

    float currentDistPercentage;

    [Header("References")]
    [SerializeField] LineRenderer mainRenderer;
    [SerializeField] LineRenderer outlineRenderer;

    [Header("RSO")]
    [SerializeField] RSO_PlantDistance rsoPlantDist;

    private void Start()
    {
        mainRenderer.material.color = plantGradient.Evaluate(0f);
        outlineRenderer.material.color = outlineGradient.Evaluate(0f);

        mainRenderer.widthMultiplier = plantThickness;
        outlineRenderer.widthMultiplier = outlineThickness;

        mainRenderer.positionCount = 1;
        mainRenderer.SetPosition(0, startingPoint.position);

        branchMat = Instantiate(mainRenderer.material);

        if (lastLeafDist < .5f) lastLeafDist = .5f;

        for (int i = 0; i < 30; i++)
        {
            branchQueue.Enqueue(CreateBranch());
            //leafsQueue.Enqueue(CreateLeaf());
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

        currentDistPercentage = rsoPlantDist.Value / maxDistance;
        mainRenderer.material.color = plantGradient.Evaluate(currentDistPercentage);
        outlineRenderer.material.color = outlineGradient.Evaluate(currentDistPercentage);
        branchMat.color = branchGradient.Evaluate(currentDistPercentage);

        //UpdateLeaf(pathPoints, maxDistance);
        UpdateBranches(positions, maxDistance);
    }

    void UpdateLeaf(Vector3[] pathPoints, float maxDistance)
    {
        if (pathPoints.Length <= 1) return;

        if(rsoPlantDist.Value > lastLeafDist + distRequireToSpawnLeaf)
        {
            lastLeafDist = rsoPlantDist.Value;

            int i = Random.Range(0, pathPoints.Length - 1);
            Vector2 leafPos = pathPoints[i] + (Vector3)Random.insideUnitCircle * leafDistPerPlantDist.Evaluate(maxDistance);

            SpriteRenderer g;
            if (leafsQueue.Count <= 0) g = CreateLeaf();
            else g = leafsQueue.Dequeue();

            g.gameObject.SetActive(true);

            g.transform.position = leafPos;
            g.transform.position = g.transform.position + Vector3.back * 10;

            g.transform.localScale = Vector3.zero;
            float distFromStart = rsoPlantDist.Value;

            Leaf leaf = new Leaf(g, distFromStart);
            leafs.Add(leaf);
        }
        else if(rsoPlantDist.Value < lastLeafDist)
        {
            lastLeafDist = rsoPlantDist.Value;
            if (lastLeafDist < .5f) lastLeafDist = .5f;
        }

        List<Leaf> newLeafs = new List<Leaf>();
        foreach (var item in leafs)
        {
            if(item.distFromStart > rsoPlantDist.Value)
            {
                item.graphics.gameObject.SetActive(false);
                leafsQueue.Enqueue(item.graphics);
            }
            else
            {
                item.graphics.transform.localScale = Vector3.one * leafSizePerDistance.Evaluate(maxDistance - item.distFromStart);
                item.graphics.color = leafGradient.Evaluate(currentDistPercentage);
                newLeafs.Add(item);
            }
        }
        leafs = newLeafs;
    }

    void UpdateBranches(Vector3[] pathPositions, float maxDistance)
    {
        if (pathPositions.Length > 1)
        {
            if (branchs.Count <= 0)
            {
                SpawnBranch(0, pathPositions[0], Utils.GetPerpendicularBetweenPoints(pathPositions[0], pathPositions[1]));
            }

            if (Vector2.Distance(branchs[^1].branch.transform.position, pathPositions[^1]) >= _branchSpacing)
            {
                SpawnBranch(rsoPlantDist.Value / maxDistance, pathPositions[^1], Utils.GetPerpendicularBetweenPoints(pathPositions[^1], pathPositions[^2]));
            }
        }

        List<Branch> newBranches = new();
        foreach (var branch in branchs)
        {
            if (rsoPlantDist.Value / maxDistance < branch.positionFromMaxDistance)
            {
                branchQueue.Enqueue(branch);
                branch.branch.gameObject.SetActive(false);
            }
            else
            {
                branch.branch.UpdateVisual(Mathf.Clamp(((rsoPlantDist.Value / maxDistance) - branch.positionFromMaxDistance) / branchGrowthPercentage, 0f, 1f));
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
        branch.branch.gameObject.SetActive(true);
        branchs.Add(branch);

        branch.branch.Setup(currentDistPercentage, branchMat);

        branch.branch.transform.position = position;
        branch.branch.transform.position = new Vector3(branch.branch.transform.position.x, branch.branch.transform.position.y, 1);


        if (Random.value < .5f) normal = -normal;
        branch.branch.transform.up = normal;

        return branch;
    }
    Branch CreateBranch()
    {
        BranchVisual branch = Instantiate(branchPrefab, transform);
        branch.gameObject.SetActive(false);

        return new Branch(branch, 0);
    }

    SpriteRenderer CreateLeaf()
    {
        SpriteRenderer g = Instantiate(leafPrefab, transform);
        g.gameObject.SetActive(false);
        return g;
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

    float GetTotalPathDistance(Vector3[] pathPoints)
    {
        float distance = 0f;

        for (int i = 1; i < pathPoints.Length; i++)
        {
            distance += Vector3.Distance(pathPoints[i - 1], pathPoints[i]);
        }

        return distance;
    }
    float GetPathDistanceFromIndex(Vector3[] pathPoints, int startIndex)
    {
        float distance = 0f;

        if (startIndex < 0 || startIndex >= pathPoints.Length - 1)
            return 0f;

        for (int i = startIndex + 1; i < pathPoints.Length; i++)
        {
            distance += Vector3.Distance(pathPoints[i - 1], pathPoints[i]);
        }

        return distance;
    }
}