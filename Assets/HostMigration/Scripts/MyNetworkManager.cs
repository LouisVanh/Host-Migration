using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkManager
{
    #region host migration
    //Disconnect only if you are host, if not this will be false and you will join next lobby
    public static bool DisconnectGracefully = false;
    //If new host, so you start new lobby
    public static bool IsNewHost = false;

    //this is set by localclient, so once leaving this will be stored
    public static PlayerData MyPlayerData;
    //also stored by localclient everytime a new client joins
    public static HostConnectionData BackUpHostConnectionData;

    public bool IsDebugging; // Set to true to avoid any normal game scene interactions to test out host migration

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
        Debug.LogWarning("1. OnServerAddPlayer called!");

        if(NetworkServer.spawned.TryGetValue(NetworkServer.localConnection.identity.netId, out NetworkIdentity hostId))
        {
            Debug.Log("2. Host found!");
            var hostClient = hostId.GetComponent<MyClient>();
            if (hostClient.UniqueClientIdentifier ==0)
            {
                Debug.Log("3. But host is the only person here");
            }
            else
            {
                Debug.Log("3. Host is not the only person here! Sending over UCID to new player");
                UniqueClientIdProvider.Instance.CmdMakeSureEveryoneKnowsMyUCID(hostClient, hostClient.UniqueClientIdentifier);
            }
        }
        // Don't do it here as it calls to early for the playercount to work, done in PlayerRegistering
        //TrySetBackUpHost("localhost", GetNextHost());
    }
    public override void OnClientDisconnect()
    {
        if (!DisconnectGracefully)
        {
            Debug.Log("Client disconnected, retrying to join!");
            HostMigrationData.Instance.StartCoroutineMigrateHost();
        }
        else
        {
            Debug.LogWarning("Host disconnected, clearing data!");
            //clear the data if not rejoining a game
            MyPlayerData = new PlayerData();
        }

        base.OnClientDisconnect();
    }
    
    #endregion host migration


    #region normal network manager
    public static new MyNetworkManager singleton => (MyNetworkManager)NetworkManager.singleton;
    private bool _isRestartingGame;
    public bool IsRestartingGame // This is set through an RPC, so all players receive this
    {
        get { return _isRestartingGame; }
        set
        {
            if (_isRestartingGame == value) return;
            if (value == true) // if we want to restart
            {
                if (!NetworkClient.localPlayer.isServer) Debug.Log("hey i'm the client");
                if (NetworkClient.localPlayer.isServer) Debug.Log("hey i'm the server");
                _isRestartingGame = true;
            }
        }
    } // Only for restarts, not normal starts

    public static void RestartGameScene()
    {
        singleton.ServerChangeScene("GameScene");
    }
    public override void OnStartServer()
    {
        Debug.LogWarning("Server Started!");
    }
    public override void OnStopServer()
    {
        Debug.Log("Server Stopped!");
    }
    public override void OnStartClient()
    {
        Debug.LogWarning("OnStartClient!");
    }
    public override void OnStopClient()
    {
        Debug.Log("Client stopped!");
    }
    public override void OnStopHost()
    {
        Debug.Log("Host stopped!");
    }

    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        Debug.LogWarning("Client changing scene IS NOT RUNNING EVER?");

        if (!IsRestartingGame) return;
        Debug.Log("Client restarting scene");

        if (UIManager.Instance.gameObject)
            SceneManager.MoveGameObjectToScene(UIManager.Instance.gameObject, SceneManager.GetActiveScene());
        if (TurnManager.Instance.gameObject)
            SceneManager.MoveGameObjectToScene(TurnManager.Instance.gameObject, SceneManager.GetActiveScene());
        if (SoundManager.Instance.gameObject)
            SceneManager.MoveGameObjectToScene(SoundManager.Instance.gameObject, SceneManager.GetActiveScene());
        if (PlayersManager.Instance.gameObject)
            SceneManager.MoveGameObjectToScene(PlayersManager.Instance.gameObject, SceneManager.GetActiveScene());
        if (WaveManager.Instance.gameObject)
            SceneManager.MoveGameObjectToScene(WaveManager.Instance.gameObject, SceneManager.GetActiveScene());
        if (DiceManager.Instance.gameObject)
            SceneManager.MoveGameObjectToScene(DiceManager.Instance.gameObject, SceneManager.GetActiveScene());

        IsRestartingGame = false;
    }
    public override void OnServerChangeScene(string sceneName)
    {
        Debug.LogWarning("SERVER CHANGING SCENE");

        if (!IsRestartingGame) return;
        Debug.Log("Server restarting scene");
        //if (networkSceneName == offlineScene) return;
        // This will not work ^^
        //Debug.Log(networkSceneName + " " + offlineScene); // It's still the offline scene, but it's giving GameScene as the networkSceneName
        if (UIManager.Instance.gameObject)
            SceneManager.MoveGameObjectToScene(UIManager.Instance.gameObject, SceneManager.GetActiveScene());
        if (TurnManager.Instance.gameObject)
            SceneManager.MoveGameObjectToScene(TurnManager.Instance.gameObject, SceneManager.GetActiveScene());
        if (SoundManager.Instance.gameObject)
            SceneManager.MoveGameObjectToScene(SoundManager.Instance.gameObject, SceneManager.GetActiveScene());
        if (PlayersManager.Instance.gameObject)
            SceneManager.MoveGameObjectToScene(PlayersManager.Instance.gameObject, SceneManager.GetActiveScene());
        if (WaveManager.Instance.gameObject)
            SceneManager.MoveGameObjectToScene(WaveManager.Instance.gameObject, SceneManager.GetActiveScene());
        if (DiceManager.Instance.gameObject)
            SceneManager.MoveGameObjectToScene(DiceManager.Instance.gameObject, SceneManager.GetActiveScene());

        IsRestartingGame = false;
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
    }

    //Wanna use IEnumerator WaitUntilEndOfFrame()? Should probably do it in start of the actual script instead (call from player for example)
    #endregion normal networkmanager
}