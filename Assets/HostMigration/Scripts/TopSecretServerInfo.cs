using Mirror;
using UnityEngine;

public class TopSecretServerInfo : NetworkBehaviour
{
    //This example class will be on every player, but the data will be set on the server only.
    [ReadOnly] public string SecretName;

    public void Update() // Testing purposes of course
    {
        if (!isServer) return;
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("I am the server and I pressed X");
            SetSecretNameOfEveryone();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            NetworkServerDebugger.PrintAllNetworkObjects();
        }
    }

    [Command(requiresAuthority = false)]
    public void SetSecretNameOfEveryone()
    {
        foreach (var player in PlayersManager.Instance.Players)
        {
            if (NetworkServer.spawned.TryGetValue(player, out NetworkIdentity playerNetId))
            {
                string secretName = TopSecretServerInfo.GetSecretName();
                playerNetId.GetComponent<TopSecretServerInfo>().SecretName = secretName;

                Debug.Log("Name set: " + secretName);
                var ucid = playerNetId.GetComponent<MyClient>().UniqueClientIdentifier;
                HostMigrationData.Instance.AddMigrationData(
                    new MigrationData(ucid, nameof(TopSecretServerInfo), nameof(SecretName), secretName));
            }
        }
    }

    public static string GetSecretName()
    {
        var random = Random.Range(1, 10);
        return random switch
        {
            1 => "John",
            2 => "Bob",
            3 => "Jef",
            4 => "Fries",
            5 => "Sara",
            6 => "Louis",
            7 => "Cedric",
            8 => "Alexander",
            9 => "Filou",
            _ => throw new System.NotImplementedException(),
        };
    }
}
