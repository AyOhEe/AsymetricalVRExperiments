using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMessage : MonoBehaviour
{
    public string message;

    public void Send_Message()
    {
        Debug.Log(message);
    }
}
