using UnityEngine;
using UnityEngine.Events;

public class Activable : MonoBehaviour
{
    bool isActive = false;

    [Header("Settings")]
    [SerializeField] UnityEvent enableEvent;
    [SerializeField] UnityEvent disableEvent;

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
            item.OnPlantExit += CheckTriggerable;
        }
    }

    void CheckTriggerable()
    {
        for (int i = 0; i < triggerables.Length; i++)
        {
            if (!triggerables[i].isActive)
            {
                if(isActive)
                {
                    disableEvent?.Invoke();
                    isActive = false;
                }
                return;
            }
        }
        
        isActive = true;
        enableEvent?.Invoke();
    }

    public void Enable()
    {
        isActive = true;
        enableEvent?.Invoke();
    }

    public void EnablePrintDebug() { print("Enable"); }
    public void DisablePrintDebug() { print("Disable"); }
}