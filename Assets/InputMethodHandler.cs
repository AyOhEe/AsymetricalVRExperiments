using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputMethodHandler : MonoBehaviour
{
    public enum InputMethod
    {
        VR,
        NonVR
    }

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
        server.GetComponent<GameServer>().keepListening = true;
        //keep server between scenes
        DontDestroyOnLoad(server);
    }
}
