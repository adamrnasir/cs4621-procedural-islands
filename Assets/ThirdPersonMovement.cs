using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{

    // Land music
    [SerializeField] private AudioSource landAudio = null;
    // Water music
    [SerializeField] private AudioSource waterAudio = null;


    public CharacterController controller; 
    public Transform cam; 

    public float gravity = -9.81f;
    public Transform groundCheck;
    public float groundDistance = 0.4f; 
    public LayerMask groundMask; 
    public LayerMask waterMask; 

    Vector3 velocity; 
    bool isGrounded; 
    bool isSwimming;

    public float speed = 1200f;

    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    // Update is called once per frame
    void Update()
    {
        Cursor.visible = false;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        isSwimming = Physics.CheckSphere(groundCheck.position, groundDistance, waterMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

    //    System.Console.WriteLine( if (Input.GetKeyDown(KeyCode.F) && !flight) {
    //         gravity = 0f;
    //         velocity.y = 0f;
    //         flight = true;
    //     } else if (Input.GetKeyDown(KeyCode.F) && flight) { 
    //         gravity = -9.81f;
    //         flight = false;
    //     });

        // Unmute land music and mute water music if the player is on land
        if (isGrounded && !isSwimming)
        {
            landAudio.mute = false;
            waterAudio.mute = true;
        }

        // Unmute water music and mute land music if the player is in the water
        if (isSwimming && !isGrounded)
        {
            waterAudio.mute = false;
            landAudio.mute = true;
        }


        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(0f, 0f, vertical).normalized;

        direction.x += horizontal * Time.deltaTime;

        velocity.y += gravity * Time.deltaTime;

        if ((isGrounded || isSwimming) && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = 6f;
        }


        if (direction.magnitude >= 0.1f) { 

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y; 
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; 

            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        controller.Move(velocity * Time.deltaTime);
    }
}
