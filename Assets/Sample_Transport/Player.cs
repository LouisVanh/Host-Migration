using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    void Update() {
        if (Input.GetKeyDown(KeyCode.D) || Input.GetMouseButtonDown(0)) {
            transform.position += Vector3.right;
        } else if (Input.GetKeyDown(KeyCode.A)) {
            transform.position += Vector3.left;
        }
    }
}
