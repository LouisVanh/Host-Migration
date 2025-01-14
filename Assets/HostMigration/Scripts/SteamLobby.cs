using UnityEngine;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{
    private GameObject _hostButton;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    public static CSteamID SteamLobbyId;
    public const string HostAddressKey = "HostAddress";

    private void Start()
    {
        if (!SteamManager.Initialized) { return; }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        _hostButton = GameObject.FindWithTag("SteamHostButton");
    }



    public void HostLobby() // CALL THIS FROM A BUTTON
    {
        _hostButton.SetActive(false);

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, MyNetworkManager.singleton.maxConnections);
    }



    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            _hostButton.SetActive(true);
            return;
        }


        MyNetworkManager.singleton.StartHost();

        SteamLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(SteamLobbyId, HostAddressKey, SteamUser.GetSteamID().ToString());
        Debug.LogWarning($"Created Steam lobby: {SteamLobbyId} - steamid: {SteamUser.GetSteamID()}");
    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.RequestLobbyData(callback.m_steamIDLobby);
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active)
        {
            return;
        }

        SteamLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        string hostAddress = SteamMatchmaking.GetLobbyData(SteamLobbyId, HostAddressKey);

        Debug.LogWarning("Entering lobby with hostaddress " + hostAddress);
        MyNetworkManager.singleton.networkAddress = hostAddress;
        MyNetworkManager.singleton.StartClient();

        _hostButton.SetActive(false);
    }
}