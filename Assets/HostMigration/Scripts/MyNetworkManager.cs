using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    public override void OnStartServer()
    {
        Debug.Log("Server Started!");
        TurnManager.Instance.UpdateGameState(GameState.WaitingLobby);
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
        Debug.Log("OnStartClient!");
        UIManager.Instance.StartTheUI();
        // make player activate controls

    }
    public override void OnStopClient()
    {
        Debug.Log("Client stopped!");
    }
    public override void OnStopHost()
    {
        Debug.Log("Host stopped!");
    }
    //Wanna use IEnumerator WaitUntilEndOfFrame()? Should probably do it in start of the actual script instead (call from player for example)
}