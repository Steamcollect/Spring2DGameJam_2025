using TMPro;
using UnityEngine;
using DG.Tweening;

public class TutoManager : MonoBehaviour
{
    //[Header("Settings")]

    [Header("References")]
    [SerializeField] Activable door;
    [SerializeField] TMP_Text growTutoTxt;
    [SerializeField] TMP_Text ungrowTutoTxt;

    [Space(10)]
    [SerializeField] CircleAnim growCircle;
    [SerializeField] CircleAnim ungrowCircle;

    public SoundComponent wooshSound;

    //[Header("RSO")]
    //[Header("RSE")]
    //[Header("RSF")]

    TutoSteps steps;

    enum TutoSteps
    {
        Grow,
        Ungrow,
        End
    }

    private void Start()
    {
        PlantManager.instance.OnGrow += OnGrow;
        PlantManager.instance.OnUnGrow += OnUngrow;
    }
    private void OnDisable()
    {
        PlantManager.instance.OnGrow -= OnGrow;
        PlantManager.instance.OnUnGrow -= OnUngrow;
    }

    void OnGrow()
    {
        if(steps == TutoSteps.Grow)
        {
            steps = TutoSteps.Ungrow;

            Utils.Delay(this, () =>
            {
                growCircle.Open();
                wooshSound.PlayClip();

                growTutoTxt.DOFade(0, 1).OnComplete(() =>
                {
                    ungrowTutoTxt.DOFade(1, 1);
                });
            }, 1.5f);
        }
    }

    void OnUngrow()
    {
        if (steps == TutoSteps.Ungrow)
        {
            steps = TutoSteps.End;

            Utils.Delay(this, () =>
            {
                ungrowCircle.Open();
                wooshSound.PlayClip();

                ungrowTutoTxt.DOFade(0, 1).OnComplete(() =>
                {
                    door.Enable();
                });
            }, 1.5f);
        }
    }
}