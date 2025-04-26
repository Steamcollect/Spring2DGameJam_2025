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
    }

    public void UpdatePlantVisual(Vector3[] pathPoints, Vector3 currentPoint, float maxDistance)
    {
        Vector3[] positions = GetZigZagPathPositions(pathPoints);

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

        mainRenderer.material.color = plantGradient.Evaluate(rsoPlantDist.Value / maxDistance);
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
}