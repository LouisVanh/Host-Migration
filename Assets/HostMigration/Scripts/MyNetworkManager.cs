using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class MyNetworkManager : NetworkManager
{
    public static new MyNetworkManager singleton => (MyNetworkManager)NetworkManager.singleton;

    public bool IsRestartingGame; // Only for restarts, not normal starts

    public override void OnStartServer()
    {
        Debug.LogWarning("Server Started!");
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);
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
        if (!IsRestartingGame) return;
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
    }
    public override void OnServerChangeScene(string sceneName)
    {
        if (!IsRestartingGame) return;
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

}