using UnityEngine;

public class LifeTimePickable : Triggerable
{
    [Header("Settings")]
    [SerializeField] float lifeTimeGiven;

    [Header("References")]
    [SerializeField] GameObject visualGO;
    [SerializeField] ParticleSystem destroyParticle;
    [SerializeField] CircleAnim circleAnim;

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
        visualGO.SetActive(false);
        destroyParticle.Play();
        circleAnim.Open();
    }
    void OnExit()
    {
        PlantManager.instance.ModifyMaxDistance(-lifeTimeGiven);
        visualGO?.SetActive(true);
        destroyParticle?.Play();
        circleAnim.Close();
    }
}