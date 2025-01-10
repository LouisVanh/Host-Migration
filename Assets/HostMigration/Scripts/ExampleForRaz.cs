using Mirror;
using UnityEngine;

// Example for raz
public class Example : NetworkBehaviour
{
    [SerializeField] private GameObject _bullet;
    [SerializeField] private Transform _gunTip;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        var startPos = Camera.main.transform.position;
        var dir = Camera.main.transform.forward;
        Ray ray = new(startPos, dir);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Can have a command here to do damage to the player if there's a hit
        }

        CmdShootBullet(startPos, dir);
    }

    [Command(requiresAuthority = false)]
    private void CmdShootBullet(Vector3 pos, Vector3 dir)
    {
        // Locally instantiate bullet on all clients
        RpcSpawnBullet(pos, dir);
    }

    [ClientRpc]
    private void RpcSpawnBullet(Vector3 pos, Vector3 dir)
    {
        GameObject bullet = Instantiate(_bullet, pos, Quaternion.identity);
        // Move bullet along dir
    }
}



