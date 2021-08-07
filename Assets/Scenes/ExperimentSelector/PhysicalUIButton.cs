using System;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalUIButton : MonoBehaviour
{
    public Action onPress;
    public Action onExit;

    public float requiredStaySeconds;
    private float currentStaySeconds;
    public Action onHold;

    private void OnTriggerEnter(Collider other)
    {
        onPress();
        currentStaySeconds = 0;
    }

    private void OnTriggerStay(Collider other)
    {
        currentStaySeconds += Time.unscaledDeltaTime;
        if (currentStaySeconds > requiredStaySeconds)
            onHold();
    }

    private void OnTriggerExit(Collider collision)
    {
        onExit();
    }
}
