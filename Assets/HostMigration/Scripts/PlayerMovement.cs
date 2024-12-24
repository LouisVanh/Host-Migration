using UnityEngine;
using Mirror;

public class PlayerMove : NetworkBehaviour
{

    [SerializeField] private float _speed = 2.5f; // Default value, overrideable in engine

    private void Update()
    {
        if (isLocalPlayer)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            Vector3 playerMovement = new(h * _speed * Time.deltaTime, v * _speed * Time.deltaTime, 0);

            transform.position = transform.position + playerMovement;
        }
    }
}
