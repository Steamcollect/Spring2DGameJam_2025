using UnityEngine;
using UnityEngine.SceneManagement;

public class EndZone : Triggerable
{
    [SerializeField, SceneName] string levelToLoad;

    public override void OnPlantEnter()
    {
        SceneManager.LoadScene(levelToLoad);
    }

    public override void OnPlantExit()
    {
        // Do nothing
    }
}