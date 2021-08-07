using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (Input.GetButtonDown(Button))
        {
            if (pressKeyUIInstance & !spawnUIInstance & !TeamSetupInstance)
            {
                spawnUIInstance = Instantiate(spawnUI, uiCanvas.transform);
            }
            else if (spawnUIInstance)
            {
                Destroy(spawnUIInstance);
            }
            else if (TeamSetupInstance)
            {
                Destroy(TeamSetupInstance);
            }
        }
    }

    public void SelectScene(SceneInfo scene)
    {
        SelectedScene = scene;
        Destroy(spawnUIInstance);
        TeamSetupInstance = Instantiate(TeamSetupUI, uiCanvas.transform);
    }

    public void LoadSelectedScene()
    {
        SceneManager.LoadScene(SelectedScene.SceneName);
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
