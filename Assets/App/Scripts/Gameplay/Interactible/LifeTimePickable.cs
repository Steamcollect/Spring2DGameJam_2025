using UnityEngine;

public class LifeTimePickable : Triggerable
{
    [Header("Settings")]
    [SerializeField] float lifeTimeGiven;

    //[Header("References")]

    //[Header("RSO")]
    //[Header("RSE")]
    //[Header("RSF")]

    private void Awake()
    {
        OnPlantEnter += OnEnter;
        OnPlantExit += OnExit;
    }

    void OnEnter()
    {
        PlantManager.instance.ModifyMaxDistance(lifeTimeGiven);
    }
    void OnExit()
    {
        PlantManager.instance.ModifyMaxDistance(-lifeTimeGiven);
    }
}