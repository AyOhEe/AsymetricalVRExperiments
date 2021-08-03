using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectUI : MonoBehaviour
{
    public string Button;

    public GameObject pressKeyUI;
    private GameObject pressKeyUIInstance;

    public GameObject spawnUI;
    private GameObject spawnUIInstance;
    public SceneInfo SelectedScene;

    public GameObject TeamSetupUI;
    private GameObject TeamSetupInstance;


    public Canvas uiCanvas;

    public void Update()
    {
        if (Input.GetButtonDown(Button) & !TeamSetupInstance)
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

    public void SelectScene(SceneInfo scene)
    {
        SelectedScene = scene;
        Destroy(spawnUIInstance);
        TeamSetupInstance = Instantiate(TeamSetupUI, uiCanvas.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<NonVRPlayerController>().Player.LocalOwned)
        {
            if (!pressKeyUIInstance & !TeamSetupInstance)
            {
                pressKeyUIInstance = Instantiate(pressKeyUI, uiCanvas.transform);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (pressKeyUIInstance & !TeamSetupInstance)
        {
            Destroy(pressKeyUIInstance);
        }
    }
}
