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
    public List<MigrationData> MigrationDatas;
    // For this example, every player has a secret nickname that the server gave them. (That only the server knows)
    // This needs to be saved in here and then later sent to the new host.
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
public struct MigrationData
{
    public uint OwnerNetId;        // The player this data belongs to
    public string ComponentName;   // The name of the component (e.g., "PlayerController")
    public string TypeName;        // The name of the type (e.g., "System.Int32")
    public string VariableName;    // The name of the variable (e.g., "Health")
    public string SerializedValue; // The serialized value as a string

    public MigrationData(uint ownerNetId, string componentName, string variableName, object value)
    {
        OwnerNetId = ownerNetId;
        ComponentName = componentName;
        VariableName = variableName;
        // These are automatically derived from value
        //TypeName = value.GetType().AssemblyQualifiedName; // Fully qualified type name
        //SerializedValue = JsonUtility.ToJson(value); // Use JSON for serialization
        TypeName = value.GetType().ToString();
        Debug.Log("value = " + value);
        SerializedValue = value.ToString();
        Debug.Log("SerializedValue = " + value.ToString());
    }

    public override string ToString()
    {
        string valueString = string.IsNullOrWhiteSpace(SerializedValue) ? "null" : SerializedValue.ToString();
        return $"NetID: {OwnerNetId}, Component: {ComponentName}, Variable: {VariableName}, Type: {TypeName}, Value: {valueString}";
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
    public uint UniqueClientIdentifier;

    public PlayerData(Vector3 pos, Quaternion rot, string startGameMessage, uint ucid, bool shouldMigrate)
    {
        this.Position = pos;
        this.Rotation = rot;
        this.StartGameMessage = startGameMessage;
        this.UniqueClientIdentifier = ucid;
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
            Destroy(this);
        else
            Instance = this;

        //DontDestroyOnLoad(this.gameObject); // Already done in netmgr
    }
    [Space(10)]
    [/*ReadOnly,*/ SerializeField] private List<MigrationData> _migrationDatas = new();
    [Space(1000)]
    [SerializeField] private string _spaceForTheEditor = " ";
    public List<MigrationData> GetMigrationDatas() { return _migrationDatas; }

    public void OverrideMigrationData(List<MigrationData> newDatas)
    {
        _migrationDatas.Clear();
        _migrationDatas = newDatas;
    }

    // Save new info to this, send it through the MigrationDataTransfer 
    // Serveronly
    /// <summary>
    /// Add server-side data to a list in HostMigrationData, to transfer over to the next host
    /// </summary>
    /// <param name="migrationData"></param>
    public void AddMigrationData(MigrationData migrationData)
    {
        Debug.Log($"Attempting to add migration data: {migrationData}");

        // Check if an entry already exists with the same netId, componentName, and variableName
        for (int i = 0; i < _migrationDatas.Count; i++)
        {
            if (_migrationDatas[i].OwnerNetId == migrationData.OwnerNetId &&
                _migrationDatas[i].ComponentName == migrationData.ComponentName &&
                _migrationDatas[i].VariableName == migrationData.VariableName)
                // Don't need to check for the type/value, irrelevant. If it exists it's covered here already.
            {
                _migrationDatas[i] = migrationData;
                Debug.Log($"Replaced migration data for NetID {migrationData.OwnerNetId}," +
                    $" Component {migrationData.ComponentName}, Variable {migrationData.VariableName}");
                return;
            }
        }

        // If no matching entry was found, add the new data
        _migrationDatas.Add(migrationData);
        Debug.Log($"Added new migration data: {migrationData}");
    }


    public void RetrieveFromDataMembers()
    {
        foreach (var migrationData in _migrationDatas)
        {
            // Find the player object using the OwnerNetId
            if (!NetworkServer.spawned.TryGetValue(migrationData.OwnerNetId, out NetworkIdentity playerIdentity))
            {
                Debug.LogWarning($"Player with NetID: {migrationData.OwnerNetId} not found!");
                continue;
            }

            // Get the component where the variable resides
            Component targetComponent = playerIdentity.GetComponent(migrationData.ComponentName);
            if (targetComponent == null)
            {
                Debug.LogWarning($"Component {migrationData.ComponentName} not found on player with NetID: {migrationData.OwnerNetId}!");
                continue;
            }

            // Use reflection to find and set the variable dynamically
            var field = targetComponent.GetType().GetField(migrationData.VariableName,
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                try
                {
                    // Handle primitive types and complex types separately during deserialization
                    object deserializedValue;
                    if (field.FieldType == typeof(string))
                    {
                        deserializedValue = migrationData.SerializedValue; // Strings are stored as-is
                    }
                    else if (field.FieldType.IsPrimitive || field.FieldType == typeof(decimal))
                    {
                        deserializedValue = Convert.ChangeType(migrationData.SerializedValue, field.FieldType);
                    }
                    else
                    {
                        deserializedValue = JsonUtility.FromJson(migrationData.SerializedValue, field.FieldType);
                    }

                    // Set the deserialized value back to the field
                    field.SetValue(targetComponent, deserializedValue);
                    Debug.Log($"Restored {migrationData.VariableName} to {deserializedValue} for player {migrationData.OwnerNetId}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to restore {migrationData.VariableName}: {ex.Message}");
                }
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
