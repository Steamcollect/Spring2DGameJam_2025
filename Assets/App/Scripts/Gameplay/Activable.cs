using UnityEngine;
using UnityEngine.Events;

public class Activable : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] UnityEvent _event;

    [Header("References")]
    [SerializeField] Triggerable[] triggerables;

    //[Header("RSO")]
    //[Header("RSE")]
    //[Header("RSF")]

    private void Awake()
    {
        foreach (var item in triggerables)
        {
            item.OnPlantEnter += CheckTriggerable;
        }
    }

    void CheckTriggerable()
    {
        for (int i = 0; i < triggerables.Length; i++)
        {
            if (!triggerables[i].isActive) return;
        }
        
        _event?.Invoke();
    }
}