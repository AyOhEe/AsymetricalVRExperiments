using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum InputMethod
{
    VR,
    NonVR
}

public class NetworkHandler : MonoBehaviour
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
        client.GetComponent<GameClient>().ConnectionIP = IP;
        client.GetComponent<GameClient>().inputMethod = inputMethod;
        //client.GetComponent<GameClient>().spawnableObjects = spawnableObjects;
        client.GetComponent<GameClient>().possibleScenes = loadableScenes;
        client.GetComponent<GameClient>().ConnectToTcpServer();
        //keep client between scenes
        DontDestroyOnLoad(client);
    }

    public void Host()
    {
        Debug.Log("Hosting " + inputMethod.ToString() + " at " + IP);
        //make server and start listening
        GameObject server = Instantiate(gameServerPrefab);
        server.GetComponent<GameServer>().HostIP = IP;
        server.GetComponent<GameServer>().StartServer();
        server.GetComponent<GameServer>().inputMethod = inputMethod;
        //server.GetComponent<GameServer>().spawnableObjects = spawnableObjects;
        server.GetComponent<GameServer>().possibleScenes = loadableScenes;
        server.GetComponent<GameServer>().keepListening = true;
        //keep server between scenes
        DontDestroyOnLoad(server);
    }

    public void Awake()
    {
        //disable vr
        UnityEngine.XR.XRSettings.enabled = false;
    }
}
