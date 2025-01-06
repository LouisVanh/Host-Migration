using Mirror;
using UnityEngine;

public class SyncDataTestBtn : MonoBehaviour
{
    // example class with a UI button to sync before going into co-host or interval based solution

    public void ButtonSyncClick()
    {
        SyncData();
    }
    public void ButtonRetrieveClick()
    {
        RetrieveData();
    }

    private void SyncData()
    {
        // send the 
        uint targetNetId = MyNetworkManager.BackUpHostConnectionData.FutureHostNetId;
        if (NetworkServer.spawned.TryGetValue(targetNetId, out NetworkIdentity playerNetId))
        {
            NetworkConnection networkConnectionToSendTo = playerNetId.connectionToClient;
            Debug.Assert(networkConnectionToSendTo != null);
            Debug.Assert(HostMigrationData.Instance != null);
            Debug.Assert(HostMigrationData.Instance.GetMigrationDatas() != null);
            MigrationDataTransfer.Instance.SendMigrationData(networkConnectionToSendTo, HostMigrationData.Instance.GetMigrationDatas());
            Debug.Log("I just sent the data from the button click");
        } else Debug.LogWarning("I could not send the data from the button click, no other player found");
    }

    private void RetrieveData()
    {
        HostMigrationData.Instance.RetrieveFromDataMembers();
    }
}