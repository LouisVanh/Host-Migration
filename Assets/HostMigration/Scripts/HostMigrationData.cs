using Mirror;
using System.Collections;
using UnityEngine;

// this is the host data each player will store, so the address, which for us i hardcoded as localhost, but for steam would probably be the players steamid
[System.Serializable]
public struct HostData
{
    public string address;
    public uint netID;
    public HostData(string address, uint netID)
    {
        this.address = address;
        this.netID = netID;
    }

    public override string ToString()
    {
        return $"HostData with address: {this.address} and netId: {this.netID}";
    }
}

//This will be any data you want to synchronize during host migration, so for us we want to restore positions, rotations and players health.
[System.Serializable]
public struct PlayerData
{
    public Vector3 pos;
    public Quaternion rot;
    public string StartGameMessage;
    public bool NeedsToHostMigrate;
    //public int health;

    public PlayerData(Vector3 pos, Quaternion rot, /*int health,*/ string startGameMessage, bool shouldMigrate)
    {
        this.pos = pos;
        this.rot = rot;
        //this.health = health;
        this.StartGameMessage = startGameMessage;

        // This will be set to true OnDestroy
        this.NeedsToHostMigrate = shouldMigrate;
    }

    public override string ToString()
    {
        return $"Playerdata with pos: {pos} and msg: {StartGameMessage}";
    }
}

public class HostMigrationData : NetworkBehaviour
{

    public static HostMigrationData Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    //Server code
    public void TrySetBackUpHost(string address, NetworkConnectionToClient? randomHost)
    {
        Debug.Log("Trying to setup backup host");
        if (PlayersManager.Instance.GetPlayers().Count > 1)
        {
            Debug.Log("Multiple players detected!");
            if (randomHost == null)
            {
                Debug.LogError("No next backup host found");
                return;
            }

            //once found send to each client to store;
            HostData newHostData = new HostData(address, randomHost.identity.netId);
            Debug.Log($"Trying to send over new host data: " + newHostData);
            randomHost.identity.GetComponent<MyClient>().StoreNewHostData(newHostData);
        }
        else Debug.LogWarning("you're the only player, can't find a backup host");
    }

    public void StartCoroutineMigrateHost()
    {
        this.StartCoroutine(MigrateHost());
    }
    public IEnumerator MigrateHost()
    {
        //if new host, start host
        if (MyNetworkManager.isNewHost)
        {
            //these delays can be played with, i was told we have to wait x amount of frames before attempting to start
            Debug.Log("I'm the new host, waiting to start server");
            yield return new WaitForSeconds(0.3f);
            MyNetworkManager.singleton.StartHost();
            Debug.Log("Started new host");

        }
        else
        {
            //these delays can be played with, i was told we have to wait x amount of frames before attempting to start
            Debug.Log("I'm a client, waiting to start server");
            yield return new WaitForSeconds(0.6f);

            //if not new host, set hostaddress to backup and initialize joiining
            MyNetworkManager.singleton.networkAddress = MyNetworkManager.backUpHostData.address;
            MyNetworkManager.singleton.StartClient();
            Debug.Log("Started new client");
        }

        yield return null;
    }


    public static NetworkConnectionToClient GetNextHost()
    {
        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn.identity.isLocalPlayer) continue;

            return conn;
        }

        return null;
    }
}
