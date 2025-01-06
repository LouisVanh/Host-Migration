using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct ServerOnlyInformation
{
    // Any information that only the server has access to, which will need to be synced to the next host

    // For this example, every player has a secret nickname that the server gave them. (That only the server knows)
    // This needs to be saved in here and then later 
}

// this is the host data each player will store, so the connection address, but for steam would probably be the players steamid
[System.Serializable]
public struct HostConnectionData
{
    public string ConnectionAddress;
    public uint FutureHostNetId;
    public HostConnectionData(string address, uint netID)
    {
        this.ConnectionAddress = address;
        this.FutureHostNetId = netID;
    }

    public override string ToString()
    {
        return $"HostData with address: {this.ConnectionAddress} and netId: {this.FutureHostNetId}";
    }
}

[Serializable]
public struct MigrationData<T>
{
    public uint OwnerNetId;            // The player/object this data belongs to
    public string ComponentName;  // The name of the component (e.g., "PlayerController")
    public string VariableName;   // The name of the variable (e.g., "Health")
    public T VariableValue;               // The value of the variable

    public MigrationData(uint netId, string componentName, string variableName, T value)
    {
        this.OwnerNetId = netId;
        this.ComponentName = componentName;
        this.VariableName = variableName;
        this.VariableValue = value;
    }

    public override string ToString()
    {
        return $"NetID: {OwnerNetId}, Component: {ComponentName}, Variable: {VariableName}, Value: {VariableValue}";
    }
}


//This will be any data you want to synchronize during host migration, so for us we want to restore positions, rotations and players health.
[System.Serializable]
public struct PlayerData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public string StartGameMessage;
    public bool NeedsToHostMigrate;

    public PlayerData(Vector3 pos, Quaternion rot, string startGameMessage, bool shouldMigrate)
    {
        this.Position = pos;
        this.Rotation = rot;
        this.StartGameMessage = startGameMessage;

        // This will be set to true OnDestroy
        this.NeedsToHostMigrate = shouldMigrate;
    }

    public override string ToString()
    {
        return $"Playerdata with pos: {Position} and msg: {StartGameMessage}";
    }
}

public class HostMigrationData : MonoBehaviour
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

    private List<MigrationData<object>> _migrationDatas = new();

    // Save new info to this, send it through the MigrationDataTransfer
    public void AddMigrationData(MigrationData<object> migrationData)
    {
        Debug.Log($"Attempting to add migration data: {migrationData}");

        // Check if an entry already exists with the same netId, componentName, and variableName
        for (int i = 0; i < _migrationDatas.Count; i++)
        {
            if (_migrationDatas[i] is MigrationData<object> existingData &&
                existingData.OwnerNetId == migrationData.OwnerNetId &&
                existingData.ComponentName == migrationData.ComponentName &&
                existingData.VariableName == migrationData.VariableName)
            {
                // Replace the existing value
                _migrationDatas[i] = migrationData;
                Debug.Log($"Replaced migration data for NetID {migrationData.OwnerNetId}, Component {migrationData.ComponentName}, Variable {migrationData.VariableName}");
                return;
            }
        }

        // If no matching entry was found, add the new data
        _migrationDatas.Add(migrationData);
        Debug.Log($"Added new migration data: {migrationData}");
    }

    public void RetrieveFromDataMembers()
    {
        // Iterate through all migration data
        foreach (var migrationData in _migrationDatas)
        {
            // Find the player object using the OwnerNetId
            NetworkIdentity playerIdentity = NetworkServer.connections.Values
                .Where(conn => conn.identity.netId == migrationData.OwnerNetId)
                .Select(conn => conn.identity)
                .FirstOrDefault();

            if (playerIdentity == null)
            {
                Debug.LogWarning($"Player with NetID: {migrationData.OwnerNetId} not found!");
                continue; // Skip to the next migration data
            }

            // Get the component where the variable resides
            Component targetComponent = playerIdentity.GetComponent(migrationData.ComponentName);
            if (targetComponent == null)
            {
                Debug.LogWarning($"Component {migrationData.ComponentName} not found on player with NetID: {migrationData.OwnerNetId}!");
                continue; // Skip to the next migration data
            }

            // Use reflection to set the variable dynamically (you may need to adjust based on the variable type)
            var field = targetComponent.GetType().GetField(migrationData.VariableName);
            if (field != null)
            {
                field.SetValue(targetComponent, Convert.ChangeType(migrationData.VariableValue, field.FieldType));
                Debug.Log($"Restored {migrationData.VariableName} to {migrationData.VariableValue} for player {migrationData.OwnerNetId}");
            }
            else
            {
                Debug.LogWarning($"Variable {migrationData.VariableName} not found in component {migrationData.ComponentName}!");
            }
        }
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
            HostConnectionData newHostData = new HostConnectionData(address, randomHost.identity.netId);
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
        if (MyNetworkManager.IsNewHost)
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

            //if not new host, set hostaddress to backup and initialize joining
            MyNetworkManager.singleton.networkAddress = MyNetworkManager.BackUpHostConnectionData.ConnectionAddress;
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
