using System;
using UnityEngine;

public class Triggerable : MonoBehaviour
{
    [Header("Settings")]
    public bool isActive = false;

    //[Header("References")]

    //[Header("RSO")]
    //[Header("RSE")]
    //[Header("RSF")]

    public Action OnPlantEnter;
    public Action OnPlantExit;
}