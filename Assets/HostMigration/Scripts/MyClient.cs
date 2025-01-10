using Mirror;
using Mirror.Examples.Tanks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyClient : NetworkBehaviour
{
    //private string _timeOfJoining;
    [ReadOnly, SerializeField] private string _privateInfo;
    [ReadOnly] public uint UniqueClientIdentifier;

    private Color _color;
    public Color Color
    {
        get { return _color; }
        set
        {
            if (_color == value) return;
            _color = value;
            Debug.Log("Attempting to change the color to " + value);
            GetComponent<MeshRenderer>().material.color = value;
        }
    }


    private void Start()
    {
        if (isServer)
        {
            //if you are server and disconnect you want to set this true so you dont initiate a rejoining phase
            MyNetworkManager.DisconnectGracefully = true;
        }
        else
        {
            MyNetworkManager.DisconnectGracefully = false;
        }

        if (!isOwned) return; // only simulate remaking game for the local player client

        //Check if there is previous data if so reinitialize states to continue
        if (MyNetworkManager.MyPlayerData.NeedsToHostMigrate == false)
        {
            // This is the start of the first game, set the unique identifier
            _privateInfo = "My original netId is " + netIdentity.netId + " and " + DateTime.Now.ToString("HH:mm:ss") + " is when I started";
            Debug.Log("Client started for first time! New startup info: " + _privateInfo);
            Debug.Log("Client started for first time! Requesting new UCID");
            UniqueClientIdProvider.Instance.CmdRequestNewClientId();

            Debug.LogWarning("No data found, returning: this is either the start of the game or HM's bugged");
            return;
        }
        RemakeGame();
    }

    [ClientRpc]
    public void StoreNewHostData(HostConnectionData hostData)
    {
        Debug.Log($"Sucess: Storing data: {hostData}");
        //storing new hostData just incase current host leaves
        MyNetworkManager.BackUpHostConnectionData = hostData;


        //checking if this player is new host if not set false
        if (hostData.FutureHostNetId == netId && isLocalPlayer)
        {
            Debug.Log($"I'm the future host! Storing host data: {hostData}");
            MyNetworkManager.IsNewHost = true;
        }
        else
            MyNetworkManager.IsNewHost = false;
    }

    private void RemakeGame() // LOAD DATA FROM YOUR LOCAL SAVE. UPDATE IT WHEREVER NEEDED.
    {
        BenchmarkManager.StartBenchmark(BenchmarkManager.MethodClientStopWatch);
        Debug.Log("Data found, restoring");

        transform.SetPositionAndRotation(MyNetworkManager.MyPlayerData.Position, MyNetworkManager.MyPlayerData.Rotation);
        _privateInfo = MyNetworkManager.MyPlayerData.PrivateClientInfo;
        UniqueClientIdentifier = MyNetworkManager.MyPlayerData.UniqueClientIdentifier;
        // Set anything on the server side of player data (health, amount of dice, ...) anything saved serverside
        //SetNetIdOnServer(MyNetworkManager.MyPlayerData.StartGameMessage);
        UniqueClientIdProvider.Instance.CmdMakeSureEveryoneKnowsMyUCID(this, this.UniqueClientIdentifier);


        MyNetworkManager.MyPlayerData.NeedsToHostMigrate = false; // (just did)
    }

    private void OnDestroy()
    {
        // This will be called when the host leaves, on all clients.
        //when you are about to be destroyed save your data to be reused on new host
        if (isLocalPlayer)
        {
            // Initialize the extraData with random data
            System.Random rand = new System.Random();
            byte[] extraData = new byte[679]; // Array size to fill the remaining space
            rand.NextBytes(extraData); // Fill the array with random bytes
            var data = new PlayerData(transform.position, transform.rotation, _privateInfo, UniqueClientIdentifier, shouldMigrate: true, extraData);
            Debug.LogWarning($"Player {this.name} being destroyed, trying to save {data}");
            MyNetworkManager.MyPlayerData = data;
        }
    }
     // Debugging if the client values are correct, and when they are updated.
    private void OnGUI()
    {
        if (isLocalPlayer)
        {
            // Set style and positioning for the GUI box
            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.fontSize = Mathf.Max(12, Screen.height / 30);  // Font size scales with screen height
            style.alignment = TextAnchor.UpperLeft;

            // Define the content to display
            string content = $"Private Info: {_privateInfo}\n" +
                             $"Unique ID: {UniqueClientIdentifier}";

            // Calculate size and position relative to screen dimensions
            float width = Screen.width * 0.4f;  // 40% of screen width
            float height = Screen.height * 0.1f; // 10% of screen height
            float x = Screen.width - width - 10;  // 10px padding from right
            float y = Screen.height - height - 10; // 10px padding from bottom

            // Draw the GUI box with the content
            GUI.Box(new Rect(x, y, width, height), content, style);
        }
    }

    public override string ToString()
    {
        return $"Client: {name} with UCID: {UniqueClientIdentifier}";
    }
}