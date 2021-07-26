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
    public List<SceneInfo> loadableScenes;
 
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
        _Connect();
    }

    private MultiClient _Connect()
    {
        Debug.Log("Connecting " + inputMethod.ToString() + " at " + IP);
        //make client and connect
        GameObject client = Instantiate(gameClientPrefab);
        MultiClient multiClient = client.GetComponent<MultiClient>();
        multiClient.ConnectionIP = IP;
        multiClient.inputMethod = inputMethod;
        multiClient.spawnableObjects = spawnableObjects;
        multiClient.possibleScenes = loadableScenes;
        multiClient.ConnectToTcpServer();

        //keep client between scenes
        DontDestroyOnLoad(client);
        
        return multiClient;
    }

    public void Host()
    {
        StartCoroutine(_Host());
    }

    private IEnumerator _Host()
    {
        Debug.Log("Hosting " + inputMethod.ToString() + " at " + IP);
        GameObject server = Instantiate(gameServerPrefab);
        server.GetComponent<MultiServer>().IPString = IP;
        server.GetComponent<MultiServer>().StartServer();
        yield return new WaitForSeconds(1);
        _Connect().ChangeScene(0);
        DontDestroyOnLoad(server);
    }

    public void Awake()
    {
        //disable vr
        UnityEngine.XR.XRSettings.enabled = false;
    }
}
