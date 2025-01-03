using UnityEngine;
using Mirror;
public class GameStarter : NetworkBehaviour
{
    // This class's sole purpose is to start the online game; 
    // Refactoring this code out of the NetworkManager is a good idea as the networkmanager is peristent through multiple scenes, of which only a few you want to actually call this logic
    // So, here goes.

    public override void OnStartServer()
    {
        base.OnStartServer();
        TurnManager.Instance.UpdateGameState(GameState.WaitingLobby);
    }
}
