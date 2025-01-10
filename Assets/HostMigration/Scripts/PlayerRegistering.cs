using Mirror;

public class PlayerRegistering : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        CmdRegisterPlayer(gameObject.GetComponent<NetworkIdentity>().netId);
    }
    public override void OnStopClient()
    {
        if (isServer)
        {
            PlayersManager.Instance.RemovePlayer(gameObject.GetComponent<NetworkIdentity>().netId);
        }
        base.OnStopClient();
    }

    [Command(requiresAuthority = false)]
    private void CmdRegisterPlayer(uint netId) // Call this on Start, make sure to check for isLocalPlayer
    {
        PlayersManager.Instance.AddPlayer(netId);
        HostMigrationData.Instance.TrySetBackUpHost();
    }
}