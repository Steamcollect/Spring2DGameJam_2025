using System;
using UnityEngine;

public class Triggerable : MonoBehaviour
{
    [Header("Settings")]
    public bool isActive = false;

    [Header("References")]
    [SerializeField] ParticleSystem[] destroyParticles;
    [SerializeField] CircleAnim circle;
    [SerializeField] GameObject visual;
    [SerializeField] SoundComponent soundComponent;

    //[Header("RSO")]
    //[Header("RSE")]
    //[Header("RSF")]

    public Action OnPlantEnter;
    public Action OnPlantExit;

    private void Start()
    {
        OnPlantEnter += _OnEnter;
        OnPlantExit += _OnExit;
    }

    void _OnEnter()
    {
        if (soundComponent != null) soundComponent.PlayClip();

        if (visual == null) return;
        foreach (var item in destroyParticles)
        {
            item.Play();
        }
            circle?.Open();
        visual?.SetActive(false);
    }
    void _OnExit()
    {
        if (soundComponent != null) soundComponent.PlayClip();

        if (visual == null) return;

        foreach (var item in destroyParticles)
        {
            item.Play();
        }
            circle?.Close();
        visual?.SetActive(true);
    }
}