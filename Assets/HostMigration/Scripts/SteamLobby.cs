using UnityEngine;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{

    private GameObject _hostButton;


    private MyNetworkManager networkManager;

    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private const string HostAddressKey = "HostAddress";


    private void Start()
    {
        networkManager = GetComponent<MyNetworkManager>();

        if (!SteamManager.Initialized) { return; }


        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        _hostButton = GameObject.FindWithTag("SteamHostButton");
    }



    public void HostLobby() // CALL THIS FROM A BUTTON
    {
        _hostButton.SetActive(false);

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }



    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            _hostButton.SetActive(true);
            return;
        }


        networkManager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());

    }

    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {

        if (NetworkServer.active)
        {
            return;
        }


        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();

        _hostButton.SetActive(false);

    }


}