using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

public class MultiServer : MonoBehaviour
{
    //the ip being used
    public string IPString;
    public IPAddress ip;

    //delegate type for message received
    public delegate void MessageReceiveEvent(string _messsage, int clientID);
    //called when a message is received;
    public event MessageReceiveEvent MessageReceived;

    //dict of listenerThreads
    public Dictionary<int, Tuple<Thread, Socket, bool>> listenerThreads = new Dictionary<int, Tuple<Thread, Socket, bool>>();
    //main socket
    Socket listener;

    //queue of actions to be done on the main thread
    private Queue<Action> actions = new Queue<Action>();

    private int mostRecentConnection = 0;

    public bool instantStart = false;

    // Update is called once per frame
    void Update()
    {
        //run all actions queued
        if (actions.Count != 0)
        {
            lock (actions)
            {
                while (actions.Count != 0) actions.Dequeue().Invoke();
            }
        }
    }

    // Use this for initialization
    public void StartServer()
    {
        // Get Host IP Address that is used to establish a connection  
        // In this case, we get one IP address of localhost that is IP : 127.0.0.1  
        // If a host has multiple addresses, you will get a list of addresses  
        //IPHostEntry host = Dns.GetHostEntry("localhost");
        //IPAddress ipAddress = host.AddressList[0];

        ip = IPAddress.Parse(IPString);
        IPEndPoint localEndPoint = new IPEndPoint(ip, 25565);

        // Create a Socket that will use Tcp protocol      
        Debug.Log("<<<Server>>>: is Listening");
        listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        // A Socket must be associated with an endpoint using the Bind method  
        listener.Bind(localEndPoint);
        // Specify how many requests a Socket can listen before it gives Server busy response.  
        // We will listen 10 requests at a time  
        listener.Listen(10);

        //create and start the initial listener thread 
        Thread initialThread = new Thread(ClientListenerThread);
        //the initial stored socket is null, but will be filled in by the thread once it is available
        listenerThreads.Add(0, new Tuple<Thread, Socket, bool>(initialThread, null, true));
        initialThread.Start(0);
    }

    /// <summary> 	
    /// Runs in background TcpServerThread; Handles incomming TcpClient requests and sets them up with a handler thread	
    /// </summary> 	
    private void ClientListenerThread(object nThread)
    {
        //get the thread key for the dictionary
        int threadKey = (int)nThread;

        //wait for an incomming connection
        Debug.Log(String.Format("<<<Thread {0}>>>: Waiting for a connection...", threadKey));
        Socket handler = listener.Accept();
        //Found!
        Debug.Log(String.Format("<<<Thread {0}>>>: Connection Accepted", threadKey));
        //now that we have our socket, replace the null value in our dictionary tuple
        listenerThreads[threadKey] = new Tuple<Thread, Socket, bool>(listenerThreads[threadKey].Item1, handler, listenerThreads[threadKey].Item3);
        mostRecentConnection = threadKey;

        //create a new thread to listen out for a new connection once we've connected
        Thread newThread = new Thread(ClientListenerThread);
        listenerThreads.Add(threadKey + 1, new Tuple<Thread, Socket, bool>(newThread, null, false));
        newThread.Start(threadKey + 1);

        // Incoming data from the client.    
        string data = null;
        byte[] bytes = null;

        //if this client is supposed to be host, tell them
        MultiBaseRequest baseRequest_HAC = new MultiBaseRequest(MultiPossibleRequest.MultiHostAuthChange, "");
        if (listenerThreads[threadKey].Item3)
        {
            SendMessageToClient(JsonUtility.ToJson(baseRequest_HAC), threadKey);
        }

        //now that we definitely have a host present, do all the stuff for a new connection
        MultiNewConnection newConnection = new MultiNewConnection(threadKey);
        MultiBaseRequest baseRequest_NC = new MultiBaseRequest(MultiPossibleRequest.MultiNewConnection, JsonUtility.ToJson(newConnection));
        SendMessageToClient(JsonUtility.ToJson(baseRequest_NC), listenerThreads.Keys.ElementAt(0));
        
        MultiInitialData initialData = new MultiInitialData(threadKey);
        MultiBaseRequest baseRequest_Init = new MultiBaseRequest(MultiPossibleRequest.MultiInitialData, JsonUtility.ToJson(initialData));
        SendMessageToClient(JsonUtility.ToJson(baseRequest_Init), listenerThreads.Keys.ElementAt(threadKey));

        //listen only while connected
        while (handler.Connected)
        {
            bytes = new byte[2048];
            int bytesRec = handler.Receive(bytes);
            data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
            if (data.Count(f => f == '{') == data.Count(f => f == '}') & data.Count(f => f == '{') != 0)
            {
                messageReceivedListener(data, threadKey);//Encoding.ASCII.GetString(Convert.FromBase64String(data)), threadKey);
                data = null;
            }
        }

        //shutdown the socket once the client disconnects
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();

        //remove ourselves from the thread list once we've disconnected
        listenerThreads.Remove((int)threadKey);

        //change host authority to the next client in the dict(not necessarily the oldest!)
        int nextHostIndex = listenerThreads.Keys.ElementAt<int>(0);
        SendMessageToClient(JsonUtility.ToJson(baseRequest_HAC), nextHostIndex);
        listenerThreads[nextHostIndex] = new Tuple<Thread, Socket, bool>(
            listenerThreads[nextHostIndex].Item1, 
            listenerThreads[nextHostIndex].Item2, 
            true);
    }

    //sends _message to the client if there is a stream present
    public void SendMessageToClient(string _message, int clientID)
    {
        //test if the client connection at the connection id exists
        if (listenerThreads.TryGetValue(clientID, out _))
        {
            if(listenerThreads[clientID].Item2 == null)
                return;

            //it does, send the message
            Debug.Log(String.Format("<<<Thread {0}>>>: sent \"{1}\"", clientID, _message));
            byte[] messageBytes = Encoding.ASCII.GetBytes(_message + "\0");//Convert.ToBase64String(Encoding.ASCII.GetBytes(_message)) + "\0");
            listenerThreads[clientID].Item2.Send(messageBytes);
        }
        else
        {
            Debug.LogError(String.Format("<<<Thread {0}>>>: Attempted to send message to non-existent client {0}", clientID));
        }
    }

    //listen for requests
    void messageReceivedListener(string _message, int clientID)
    {
        Debug.Log(String.Format("<<<Thread {0}>>>: Recieved \"{1}\"", clientID, _message));

        //test if we've recieved a sceneObjects request
        if ((MultiPossibleRequest)JsonUtility.FromJson<MultiBaseRequest>(_message).RT == MultiPossibleRequest.MultiSceneObjects)
        {
            //we have, relay it to the thread requesting it(the most recent connection)
            MultiBaseRequest baseRequest = JsonUtility.FromJson<MultiBaseRequest>(_message);
            MultiSceneObjects sceneObjects = JsonUtility.FromJson<MultiSceneObjects>(baseRequest.R);
            SendMessageToClient(_message, sceneObjects.tN);
            return;
        }
        //relay the message to all other clients
        foreach (int key in listenerThreads.Keys)
        {
            if (key != clientID)
            {
                SendMessageToClient(_message, key);
            }
        }
    }

    private void Start()
    {
        //add our message received listener to the message received event
        MessageReceived += messageReceivedListener;
        if (instantStart)
        {
            StartServer();
        }
    }
}
