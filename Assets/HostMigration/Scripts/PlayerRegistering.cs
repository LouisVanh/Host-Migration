using Mirror;

public class PlayerRegistering : NetworkBehaviour
{
    public override void OnStartClient()
    {
        base.OnStartClient();
        CmdRegisterPlayer();
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
    private void CmdRegisterPlayer() // Call this on Start, make sure to check for isLocalPlayer
    {
        PlayersManager.Instance.AddPlayer(gameObject.GetComponent<NetworkIdentity>().netId);
        HostMigrationData.Instance.TrySetBackUpHost();
    }
}