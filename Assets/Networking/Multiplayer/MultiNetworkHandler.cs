using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MultiNetworkHandler : MonoBehaviour
{
    //the input method being used
    public InputMethod inputMethod;
    //the dropdown for the input method
    public Dropdown inputMethodDropdown;

    //the ip being used
    public string IP;
    //input field for the IP(hosting/connecting)
    public InputField IPInput;

    //prefabs
    public GameObject gameClientPrefab;
    public GameObject gameServerPrefab;

    //list of spawnable objects
    public List<GameObject> spawnableObjects;
    //list of loadable scenese
    public List<Scene> loadableScenes;

    //updates the ip to the value in IPInput
    public void UpdateIP()
    {
        IP = IPInput.text;
    }

    //updates the input method to the value in inputMethodDropdown
    public void UpdateInputMethod()
    {
        inputMethod = (InputMethod)inputMethodDropdown.value;
    }

    public void Connect()
    {
        Debug.Log("Connecting " + inputMethod.ToString() + " at " + IP);
        //make client and connect
        GameObject client = Instantiate(gameClientPrefab);
        client.GetComponent<MultiClient>().ConnectionIP = IP;
        client.GetComponent<MultiClient>().inputMethod = inputMethod;
        //client.GetComponent<GameClient>().spawnableObjects = spawnableObjects;
        client.GetComponent<MultiClient>().possibleScenes = loadableScenes;
        client.GetComponent<MultiClient>().ConnectToTcpServer();
        //keep client between scenes
        DontDestroyOnLoad(client);
    }

    public void Host()
    {
        Debug.Log("Hosting " + inputMethod.ToString() + " at " + IP);
        Connect();
        GameObject server = Instantiate(gameServerPrefab);
        DontDestroyOnLoad(server);
    }

    public void Awake()
    {
        //disable vr
        UnityEngine.XR.XRSettings.enabled = false;
    }
}
