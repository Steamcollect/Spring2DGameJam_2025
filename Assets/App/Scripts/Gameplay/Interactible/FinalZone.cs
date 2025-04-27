using Unity.Properties;
using UnityEngine;

public class FinalZone : Triggerable
{
    [SerializeField, SceneName] string levelToLoad;

    bool canLoad = true;

    private void Awake()
    {
        OnPlantEnter += OnEnter;
    }

    public void OnEnter()
    {
        if (!canLoad) return;
        FadePanel.instance.LoadNextScene(levelToLoad);
        canLoad = false;
    }
}