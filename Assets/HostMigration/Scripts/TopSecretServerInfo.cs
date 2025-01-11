using Mirror;
using UnityEngine;

public class TopSecretServerInfo : NetworkBehaviour
{
    //This example class will be on every player, but the data will be set on the server only.
    [ReadOnly] public string SecretName;

    // DO NOT CHANGE THESE COMMENTS! vvvv They are markers, looked for exactly with reflection. vvvv
    // Secret Info Fields Start
    [ReadOnly] public byte SecretInfo1;
    [ReadOnly] public byte SecretInfo2;
    [ReadOnly] public byte SecretInfo3;
    [ReadOnly] public byte SecretInfo4;
    [ReadOnly] public byte SecretInfo5;
    [ReadOnly] public byte SecretInfo6;
    [ReadOnly] public byte SecretInfo7;
    [ReadOnly] public byte SecretInfo8;
    [ReadOnly] public byte SecretInfo9;
    [ReadOnly] public byte SecretInfo10;
// Secret Info Fields End
    // DO NOT CHANGE THESE COMMENTS! ^^^^ They are markers, looked for exactly with reflection. ^^^^

    public void Update() // Testing purposes of course
    {
        if (!isServer) return;
        if (!isLocalPlayer) return;
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("I am the server and I pressed X");
            SetSecretNameOfEveryone();
            SetManyFieldsForBenchmark();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            NetworkServerDebugger.PrintAllNetworkObjects();
        }
    }

    [Command(requiresAuthority = false)]
    public void SetSecretNameOfEveryone()
    {
        foreach (var player in PlayersManager.Instance.GetClients())
        {
            string secretName = TopSecretServerInfo.GetSecretName();
            player.GetComponent<TopSecretServerInfo>().SecretName = secretName;

            Debug.Log("Name set: " + secretName);
            var ucid = player.GetComponent<MyClient>().UniqueClientIdentifier;
            HostMigrationData.Instance.AddMigrationData(
                new MigrationData(ucid, nameof(TopSecretServerInfo), nameof(SecretName), secretName));
        }
    }

    [Command(requiresAuthority = false)]
    public void SetManyFieldsForBenchmark()
    {
        foreach (var player in PlayersManager.Instance.GetClients())
        {
            var playerInfo = player.GetComponent<TopSecretServerInfo>();
            var ucid = player.GetComponent<MyClient>().UniqueClientIdentifier;

            for (int i = 1; i <= BenchmarkManager.AmountOfExtraServerDatas; i++) // this is one REALLY expensive method, please don't do this in your actual games
            {
                byte newByte = GetRandomByte();

                // Use reflection to get the field dynamically by name
                var fieldName = $"SecretInfo{i}";
                var field = typeof(TopSecretServerInfo).GetField(fieldName);

                if (field != null && field.FieldType == typeof(byte))
                {
                    field.SetValue(playerInfo, newByte); // Set the value of the field

                    HostMigrationData.Instance.AddMigrationData(
                        new MigrationData(ucid, nameof(TopSecretServerInfo), fieldName, newByte));
                }
                else
                {
                    Debug.LogWarning($"Field '{fieldName}' not found or is not of type byte.");
                }
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

    public static byte GetRandomByte()
    {
        System.Random random = new();
        byte newByte = (byte)random.Next(0, 255);
        return newByte;
    }
}
