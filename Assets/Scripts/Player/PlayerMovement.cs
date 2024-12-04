using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float speed = 2f;


    private void Movement(float delta)
    {
        if (Input.GetKey(KeyCode.W))
        {
        }
        if (Input.GetKey(KeyCode.S))
        {
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.position -= new Vector3(speed * delta, 0f, 0f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position += new Vector3(speed * delta, 0f, 0f);
        }
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (IsOwner)
        {
            Movement(Time.fixedDeltaTime);
        }
    }
}
