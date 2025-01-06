using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class MigrationDataTransfer : NetworkBehaviour
{
    public static MigrationDataTransfer Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    [TargetRpc]
    public void SendMigrationData(NetworkConnection conn, List<MigrationData> migrationDataList)
    {
        // Send over the data to the next host
        Debug.Log("Sending over migration data list!");
        HostMigrationData.Instance.OverrideMigrationData(newDatas: migrationDataList);
    }
}
