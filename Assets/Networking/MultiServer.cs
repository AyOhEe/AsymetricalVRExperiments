using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using MessagePack;

public class MultiServer : MonoBehaviour
{
    //the ip being used
    public string IPString;
    public IPAddress ip;

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
        byte[] bytes = null;

        //if this client is supposed to be host, tell them
        MultiBaseRequest baseRequest_HAC = new MultiBaseRequest(MultiPossibleRequest.MultiHostAuthChange, null);
        if (listenerThreads[threadKey].Item3)
        {
            SendMessageToClient(MessagePackSerializer.Serialize(baseRequest_HAC), threadKey);
        }

        //now that we definitely have a host present, do all the stuff for a new connection
        if (threadKey != 0)
        {
            MultiNewConnection newConnection = new MultiNewConnection(threadKey);
            MultiBaseRequest baseRequest_NC = new MultiBaseRequest(MultiPossibleRequest.MultiNewConnection, MessagePackSerializer.Serialize(newConnection));
            SendMessageToClient(MessagePackSerializer.Serialize(baseRequest_NC), listenerThreads.Keys.ElementAt(0));
        }

        //listen only while connected
        while (handler.Connected)
        {
            bytes = new byte[2048];
            int bytesRec = handler.Receive(bytes);
            messageReceivedListener(bytes, threadKey);//Encoding.ASCII.GetString(Convert.FromBase64String(data)), threadKey);
        }

        //shutdown the socket once the client disconnects
        handler.Shutdown(SocketShutdown.Both);
        handler.Close();

        //remove ourselves from the thread list once we've disconnected
        listenerThreads.Remove((int)threadKey);

        //change host authority to the next client in the dict(not necessarily the oldest!)
        int nextHostIndex = listenerThreads.Keys.ElementAt<int>(0);
        SendMessageToClient(MessagePackSerializer.Serialize(baseRequest_HAC), nextHostIndex);
        listenerThreads[nextHostIndex] = new Tuple<Thread, Socket, bool>(
            listenerThreads[nextHostIndex].Item1, 
            listenerThreads[nextHostIndex].Item2, 
            true);
    }

    //sends _message to the client if there is a stream present
    public void SendMessageToClient(byte[] _message, int clientID)
    {
        //test if the client connection at the connection id exists
        if (listenerThreads.TryGetValue(clientID, out _))
        {
            if(listenerThreads[clientID].Item2 == null)
                return;

            //it does, send the message
            Debug.Log(String.Format("<<<Thread {0}>>>: sent \"{1}\"", clientID, MessagePackSerializer.ConvertToJson(_message)));
            listenerThreads[clientID].Item2.Send(_message);
        }
        else
        {
            Debug.LogError(String.Format("<<<Thread {0}>>>: Attempted to send message to non-existent client {0}", clientID));
        }
    }

    //listen for requests
    void messageReceivedListener(byte[] _message, int clientID)
    {
        Debug.Log(String.Format("<<<Thread {0}>>>: Recieved \"{1}\"", clientID, MessagePackSerializer.ConvertToJson(_message)));

        //test if we've recieved a sceneObjects request
        if ((MultiPossibleRequest)MessagePackSerializer.Deserialize<MultiBaseRequest>(_message).RT == MultiPossibleRequest.MultiInitialData)
        {
            //we have, relay it to the thread requesting it(the most recent connection)
            MultiBaseRequest baseRequest = MessagePackSerializer.Deserialize<MultiBaseRequest>(_message);
            MultiInitialData sceneObjects = MessagePackSerializer.Deserialize<MultiInitialData>(baseRequest.R);
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
        if (instantStart)
        {
            StartServer();
        }
    }

    private void OnDestroy()
    {
        foreach (Tuple<Thread, Socket, bool> t in listenerThreads.Values)
        {
            t.Item1.Abort();
        }
    }
}
