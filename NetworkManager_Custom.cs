using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System;

public class NetworkManager_Custom : NetworkManager 
{
    private bool onlyOne = true;
    //Script used to override UNet's base NetworkManager
    //Beyond regular functionality, contains some additional functions for my game's use of networking
    private void Start()
    {
        GameObject[] managers = GameObject.FindGameObjectsWithTag("MatchManager");
        for (int i = 0; i < managers.Length; i++)
        {
            if (managers[i] != this.gameObject)
                Destroy(managers[i]);
        }
    }

    //Function checks if room code in textbox is a valid roomcode
    private bool RoomCodeValid()
    {
        try
        {
            string TextboxText = this.GetComponent<RoomHandlerCode>().CodeTextbox.text;
            TextboxText = TextboxText.ToUpper().Replace(" ", "").Replace("\n", "");
            if (!TextboxText.Contains("-"))
                return false;
            if (!Regex.IsMatch(TextboxText, @"^[A-Z-]+$"))
                return false;
            string ipAddress = this.GetComponent<RoomHandlerCode>().RoomcodeToIp(TextboxText);//TODO - Could get IP from somewhere else
            return true;
        }
        catch
        {
            return false;
        }
    }

    void SetIPAddress()
    {
        string ipAddress = this.GetComponent<RoomHandlerCode>().RoomcodeToIp(this.GetComponent<RoomHandlerCode>().CodeTextbox.text);//TODO - Could get IP from somewhere else
        NetworkManager.singleton.networkAddress = ipAddress;
    }

    void SetPort()
    {
        NetworkManager.singleton.networkPort = 7777;//TODO - Could get port from roomCode
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        //THIS FUNCTION IS CALLED WHEN CLIENT DISCONNECTS
        try
        {
            base.OnClientDisconnect(conn);
        }
        catch
        {
            Debug.Log("Exception disconnecting.");
        }
    }

    private void InitializeNetwork()
    {
        Debug.Log("Began initializeNetwork");
        ConnectionConfig myConfig = new ConnectionConfig();
        myConfig.AddChannel(QosType.Reliable);
        myConfig.AddChannel(QosType.Unreliable);
        myConfig.AddChannel(QosType.StateUpdate);
        myConfig.AddChannel(QosType.AllCostDelivery);
        myConfig.NetworkDropThreshold = 95; //95% packets that need to be dropped before connection is dropped 
        myConfig.OverflowDropThreshold = 30; //30% packets that need to be dropped before sendupdate timeout is increased
        myConfig.InitialBandwidth = 0;
        myConfig.MinUpdateTimeout = 10;
        myConfig.ConnectTimeout = 2000; // timeout before re-connect attempt will be made 
        myConfig.PingTimeout = 1500; // should have more than 3 pings per disconnect timeout, and more than 5 messages per ping 
        myConfig.DisconnectTimeout = 6000; // with a ping of 500 a disconnectimeout of 2000 is supposed to work well
        myConfig.PacketSize = 1470;
        myConfig.SendDelay = 2;
        myConfig.FragmentSize = 1300;
        myConfig.AcksType = ConnectionAcksType.Acks128;
        myConfig.MaxSentMessageQueueSize = 256;
        myConfig.AckDelay = 1;
        HostTopology myTopology = new HostTopology(myConfig, 4); //up to 4 connection allowed
        NetworkServer.Configure(myTopology);
        Debug.Log("Finished initializeNetwork");
    }

    public void StartupHost()
    {
        SetPort();
        Debug.Log("Hosting match at address " + NetworkManager.singleton.networkAddress + ":" + NetworkManager.singleton.networkPort);

        NetworkManager.singleton.StartHost();
        NetworkServer.SpawnObjects();
    }

    public void JoinGame()
    {
        try
        {
            NetworkManager.singleton.StartClient();
            NetworkServer.SpawnObjects();
        }
        catch (Exception ex)
        {
            Debug.Log("Failed to join game");
        }
    }
}
