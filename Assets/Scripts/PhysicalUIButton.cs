using System;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalUIButton : MonoBehaviour
{
    public Action onPress;
    public Action onExit;

    private float currentStaySeconds;
    public Action onHold;

    [Header("Settings")]
    public float requiredStaySeconds;
    [InspectorName("Require Tag?")]
    public bool requireTag;
    public string requiredTag;

    private void OnTriggerEnter(Collider other)
    {
        if ((requireTag & other.CompareTag(requiredTag)) | !requireTag)
        {
            onPress();
            currentStaySeconds = 0;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if ((requireTag & other.CompareTag(requiredTag)) | !requireTag)
        {
            currentStaySeconds += Time.unscaledDeltaTime;
            if (currentStaySeconds > requiredStaySeconds)
                onHold();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((requireTag & other.CompareTag(requiredTag)) | !requireTag)
        {
            onExit();
        }
    }
}
