using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 1200f;
    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask; 
    public bool flight = false;

    Vector3 velocity;
    bool isGrounded;

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (Input.GetKeyDown(KeyCode.F) && !flight) {
            gravity = 0f;
            velocity.y = 0f;
            flight = true;
        } else if (Input.GetKeyDown(KeyCode.F) && flight) { 
            gravity = -9.81f;
            flight = false;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        if (isGrounded && Input.GetKeyDown(KeyCode.V)){ 
            speed = 30f;
        }

        if(isGrounded && Input.GetKeyUp(KeyCode.V)) { 
            speed = 12f;
        }

        controller.Move(move * speed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = 6f;
        }

 

        if (flight && Input.GetKeyDown(KeyCode.Space)) { 
            velocity.y = 3f;
        }
        if (flight && Input.GetKeyUp(KeyCode.Space)) { 
            velocity.y = 0f;
        }

        if (flight && Input.GetKeyDown(KeyCode.LeftShift)) { 
            velocity.y = -3f; 
        } 
        if (flight && Input.GetKeyUp(KeyCode.LeftShift)) { 
            velocity.y = 0f; 
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
