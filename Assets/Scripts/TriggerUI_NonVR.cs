using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerUI_NonVR : MonoBehaviour
{
    public string Button;
    public GameObject pressKeyUI;
    private GameObject pressKeyUIInstance;
    public GameObject spawnUI;
    private GameObject spawnUIInstance;
    public Canvas uiCanvas;

    public void Update()
    {
        if (Input.GetButtonDown(Button))
        {
            if (pressKeyUIInstance & !spawnUIInstance)
            {
                spawnUIInstance = Instantiate(spawnUI, uiCanvas.transform);
            }
            else if(pressKeyUIInstance & spawnUIInstance)
            {
                Destroy(spawnUIInstance);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<NonVRPlayerController>().Player.LocalOwned)
        {
            if (!pressKeyUIInstance)
            {
                pressKeyUIInstance = Instantiate(pressKeyUI, uiCanvas.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (pressKeyUIInstance)
        {
            Destroy(pressKeyUIInstance);
        }
    }
}
