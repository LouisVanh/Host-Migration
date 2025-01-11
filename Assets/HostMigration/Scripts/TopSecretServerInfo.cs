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
    [ReadOnly] public byte SecretInfo11;
    [ReadOnly] public byte SecretInfo12;
    [ReadOnly] public byte SecretInfo13;
    [ReadOnly] public byte SecretInfo14;
    [ReadOnly] public byte SecretInfo15;
    [ReadOnly] public byte SecretInfo16;
    [ReadOnly] public byte SecretInfo17;
    [ReadOnly] public byte SecretInfo18;
    [ReadOnly] public byte SecretInfo19;
    [ReadOnly] public byte SecretInfo20;
    [ReadOnly] public byte SecretInfo21;
    [ReadOnly] public byte SecretInfo22;
    [ReadOnly] public byte SecretInfo23;
    [ReadOnly] public byte SecretInfo24;
    [ReadOnly] public byte SecretInfo25;
    [ReadOnly] public byte SecretInfo26;
    [ReadOnly] public byte SecretInfo27;
    [ReadOnly] public byte SecretInfo28;
    [ReadOnly] public byte SecretInfo29;
    [ReadOnly] public byte SecretInfo30;
    [ReadOnly] public byte SecretInfo31;
    [ReadOnly] public byte SecretInfo32;
    [ReadOnly] public byte SecretInfo33;
    [ReadOnly] public byte SecretInfo34;
    [ReadOnly] public byte SecretInfo35;
    [ReadOnly] public byte SecretInfo36;
    [ReadOnly] public byte SecretInfo37;
    [ReadOnly] public byte SecretInfo38;
    [ReadOnly] public byte SecretInfo39;
    [ReadOnly] public byte SecretInfo40;
    [ReadOnly] public byte SecretInfo41;
    [ReadOnly] public byte SecretInfo42;
    [ReadOnly] public byte SecretInfo43;
    [ReadOnly] public byte SecretInfo44;
    [ReadOnly] public byte SecretInfo45;
    [ReadOnly] public byte SecretInfo46;
    [ReadOnly] public byte SecretInfo47;
    [ReadOnly] public byte SecretInfo48;
    [ReadOnly] public byte SecretInfo49;
    [ReadOnly] public byte SecretInfo50;
    [ReadOnly] public byte SecretInfo51;
    [ReadOnly] public byte SecretInfo52;
    [ReadOnly] public byte SecretInfo53;
    [ReadOnly] public byte SecretInfo54;
    [ReadOnly] public byte SecretInfo55;
    [ReadOnly] public byte SecretInfo56;
    [ReadOnly] public byte SecretInfo57;
    [ReadOnly] public byte SecretInfo58;
    [ReadOnly] public byte SecretInfo59;
    [ReadOnly] public byte SecretInfo60;
    [ReadOnly] public byte SecretInfo61;
    [ReadOnly] public byte SecretInfo62;
    [ReadOnly] public byte SecretInfo63;
    [ReadOnly] public byte SecretInfo64;
    [ReadOnly] public byte SecretInfo65;
    [ReadOnly] public byte SecretInfo66;
    [ReadOnly] public byte SecretInfo67;
    [ReadOnly] public byte SecretInfo68;
    [ReadOnly] public byte SecretInfo69;
    [ReadOnly] public byte SecretInfo70;
    [ReadOnly] public byte SecretInfo71;
    [ReadOnly] public byte SecretInfo72;
    [ReadOnly] public byte SecretInfo73;
    [ReadOnly] public byte SecretInfo74;
    [ReadOnly] public byte SecretInfo75;
    [ReadOnly] public byte SecretInfo76;
    [ReadOnly] public byte SecretInfo77;
    [ReadOnly] public byte SecretInfo78;
    [ReadOnly] public byte SecretInfo79;
    [ReadOnly] public byte SecretInfo80;
    [ReadOnly] public byte SecretInfo81;
    [ReadOnly] public byte SecretInfo82;
    [ReadOnly] public byte SecretInfo83;
    [ReadOnly] public byte SecretInfo84;
    [ReadOnly] public byte SecretInfo85;
    [ReadOnly] public byte SecretInfo86;
    [ReadOnly] public byte SecretInfo87;
    [ReadOnly] public byte SecretInfo88;
    [ReadOnly] public byte SecretInfo89;
    [ReadOnly] public byte SecretInfo90;
    [ReadOnly] public byte SecretInfo91;
    [ReadOnly] public byte SecretInfo92;
    [ReadOnly] public byte SecretInfo93;
    [ReadOnly] public byte SecretInfo94;
    [ReadOnly] public byte SecretInfo95;
    [ReadOnly] public byte SecretInfo96;
    [ReadOnly] public byte SecretInfo97;
    [ReadOnly] public byte SecretInfo98;
    [ReadOnly] public byte SecretInfo99;
    [ReadOnly] public byte SecretInfo100;
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

            for (int i = 1; i <= BenchmarkManager.AmountOfExtraServerDatas; i++) // this is gonna be one expensive method, please don't do this in your actual games
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
