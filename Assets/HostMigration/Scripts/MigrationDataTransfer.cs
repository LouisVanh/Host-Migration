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
    public void SendMigrationData(NetworkConnection conn, List<MigrationData<object>> migrationDataList)
    {
        // Send over the data to the next host
    }
}
