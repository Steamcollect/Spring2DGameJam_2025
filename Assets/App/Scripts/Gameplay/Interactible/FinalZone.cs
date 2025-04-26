using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalZone : Triggerable
{
    [SerializeField, SceneName] string levelToLoad;

    private void Awake()
    {
        OnPlantEnter += OnEnter;
    }

    public void OnEnter()
    {
        SceneManager.LoadScene(levelToLoad);
    }
}