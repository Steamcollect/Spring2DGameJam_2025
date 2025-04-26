using System.Collections.Generic;
using UnityEngine;

public class Movable : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float speed;

    [Header("References")]
    [SerializeField] Rigidbody2D rb;

    private List<PlantSnapshot> plantSnapshots = new();
    private float saveInterval = 0.1f; // exemple: toutes les 0.1s
    private float saveTimer = 0f;

    [System.Serializable]
    private struct PlantSnapshot
    {
        public Vector3 position;
        public float plantDistance;

        public PlantSnapshot(Vector3 position, float plantDistance)
        {
            this.position = position;
            this.plantDistance = plantDistance;
        }
    }

    [Header("RSO")]
    [SerializeField] RSO_PlantDistance rsoPlantDist;
    [Header("RSE")]
    [SerializeField] RSE_OnPlantUngrow rseOnPlantUngrow;

    private void OnEnable()
    {
        rseOnPlantUngrow.Action += GoToPlantDistance;
    }
    private void OnDisable()
    {
        rseOnPlantUngrow.Action -= GoToPlantDistance;
}

    private void Update()
    {
        saveTimer += Time.deltaTime;
        if (saveTimer >= saveInterval)
        {
            SavePosition();
            saveTimer = 0f;
        }
    }

    public void Move(Vector2 direction)
    {
        rb.AddForce(direction * speed);
    }

    private void SavePosition()
    {
        float plantDistance = rsoPlantDist.Value;

        plantSnapshots.Add(new PlantSnapshot(transform.position, plantDistance));
    }

    public void GoToPlantDistance()
    {
        if (plantSnapshots.Count == 0) return;

        float targetDistance = rsoPlantDist.Value;

        int closestIndex = 0;
        float minDiff = Mathf.Abs(targetDistance - plantSnapshots[0].plantDistance);

        for (int i = 1; i < plantSnapshots.Count; i++)
        {
            float diff = Mathf.Abs(targetDistance - plantSnapshots[i].plantDistance);
            if (diff < minDiff)
            {
                closestIndex = i;
                minDiff = diff;
            }
        }

        PlantSnapshot closest = plantSnapshots[closestIndex];

        rb.position = closest.position;
        rb.velocity = Vector2.zero;

        if (closestIndex < plantSnapshots.Count - 1)
        {
            plantSnapshots.RemoveRange(closestIndex + 1, plantSnapshots.Count - (closestIndex + 1));
        }
    }

}
