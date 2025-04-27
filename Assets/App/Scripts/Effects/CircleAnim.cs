using DG.Tweening;
using UnityEditor;
using UnityEngine;

public class CircleAnim : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] Gradient color;

    [SerializeField] float startSize;
    [SerializeField] float endSize;

    [SerializeField] float openDelay;
    [SerializeField] float closeDelay;

    [Header("References")]
    [SerializeField] SpriteRenderer graphics;

    //[Header("RSO")]
    //[Header("RSE")]
    //[Header("RSF")]

    private void Start()
    {
        graphics.transform.localScale = Vector3.one * startSize;
        graphics.color = color.Evaluate(0);
    }

    public void Open()
    {
        graphics.DOKill();
        graphics.transform.DOKill();
        graphics.transform.DOScale(endSize, openDelay);
        DOTween.To(() => 0f, t => {
            graphics.color = color.Evaluate(t);
        }, 1f, openDelay);
    }

    public void Close()
    {
        graphics.DOKill();
        graphics.transform.DOKill();
        graphics.transform.DOScale(startSize, closeDelay);
        DOTween.To(() => 0f, t => {
            graphics.color = color.Evaluate(1f - t);
        }, 1f, closeDelay);
    }
}