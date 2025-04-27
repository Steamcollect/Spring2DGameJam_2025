using UnityEngine;
using UnityEngine.SceneManagement;

public class FadePanel : MonoBehaviour
{
    public Animator anim;

    public SoundComponent wooshSound;

    public static FadePanel instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        wooshSound.PlayClip();
    }

    public void LoadNextScene(string sceneName)
    {
        anim.SetTrigger("FadeOut");
        Utils.Delay(this, () =>
        {
            SceneManager.LoadScene(sceneName);
        }, 1.2f);
    }
}