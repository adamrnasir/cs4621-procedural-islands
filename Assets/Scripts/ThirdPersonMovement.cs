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

    public Camera cam;

    public GameObject boat;
    public Animator animator;

    public float gravity;
    public Transform groundCheck;
    public float groundDistance = 0.2f; 
    public LayerMask groundMask; 
    public LayerMask waterMask; 
    public ParticleSystem wake; 
    ParticleSystem wake_effect;

    bool isGrounded; 
    bool isSwimming;

    // New movement stuff
    public const float MAX_BOAT_SPEED = 48f;
    public const float BOAT_ACCELERATION = 0.5f;
    public const float BOAT_DECELERATION = 0.1f;

    public const float MAX_BOAT_OMEGA = 16f;
    public const float BOAT_ALPHA = 0.5f;
    public const float BOAT_DEALPHA = 0.1f;

    public const float MAX_BOAT_PITCH = 12f;
    public const float MAX_BOAT_ROLL = 16f;



    public const float MAX_WALK_SPEED = 36f;
    public const float WALK_ACCELERATION = 0.55f;
    public const float WALK_DECELERATION = 0.55f;

    public const float MAX_WALK_OMEGA = 48f;
    public const float WALK_ALPHA = 3f;
    public const float WALK_DEALPHA = 3f;

    public const float MAX_WALK_PITCH = 12f;
    public const float MAX_WALK_ROLL = 2f;


    public float speed = 0f;
    public float accel = 0f;
    public float theta = 0f;
    public float omega = 0f;
    public float alpha = 0f;

    Vector3 velocity; 

    void Update() 
    {
        // Check if we're on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Check if we're in the water
        isSwimming = Physics.CheckSphere(groundCheck.position, groundDistance, waterMask);

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (vertical != 0) 
        {
            accel = (isSwimming) ? vertical * BOAT_ACCELERATION : vertical * WALK_ACCELERATION;
        }
        else 
        {
            if (speed != 0) 
            {
                accel = -Mathf.Sign(speed) * ((isSwimming) ? BOAT_DECELERATION : WALK_DECELERATION);
            }
            else 
            {
                accel = 0f;
            }
        }

        speed += accel;
        speed = (isSwimming) ? Mathf.Clamp(speed, -MAX_BOAT_SPEED, MAX_BOAT_SPEED) : Mathf.Clamp(speed, -MAX_WALK_SPEED, MAX_WALK_SPEED);

        if (horizontal != 0) 
        {
            alpha = (isSwimming) ? horizontal * BOAT_ALPHA : horizontal * WALK_ALPHA;
        }
        else 
        {
            if (omega != 0) 
            {
                alpha = -Mathf.Sign(omega) * ((isSwimming) ? BOAT_DEALPHA : WALK_DEALPHA);
            }
            else 
            {
                alpha = 0f;
            }
        }

        omega += alpha;
        omega = (isSwimming) ? Mathf.Clamp(omega, -MAX_BOAT_OMEGA, MAX_BOAT_OMEGA) : Mathf.Clamp(omega, -MAX_WALK_OMEGA, MAX_WALK_OMEGA);

        // Set speed and omega to 0 if they're close enough to 0
        if (Mathf.Abs(speed) <= 0.1f) 
        {
            speed = Mathf.SmoothStep(speed, 0f, WALK_DECELERATION);
        }

        if (Mathf.Abs(omega) <= 0.1f) 
        {
            omega = Mathf.SmoothStep(omega, 0f, WALK_DEALPHA);
        }

        if (!isSwimming)
        {
            // landAudio.mute = false;
            landAudio.volume = Mathf.Lerp(landAudio.volume, 1f, 0.02f);
            waterAudio.volume = Mathf.Lerp(waterAudio.volume, 0f, 0.02f);
            // waterAudio.mute = true;
            boat.SetActive(false);

            if (vertical > 0 || horizontal != 0)
            {
                animator.SetBool("isWalkingForward", true);
            } 
            else if (vertical < 0)
            {
                animator.SetBool("isWalkingBackward", true);
            }
            else
            {
                if (Mathf.Abs(speed) < 1f) {
                    animator.SetBool("isWalkingForward", false);
                    animator.SetBool("isWalkingBackward", false);
                }
            }
        }
        else
        {
            // landAudio.mute = true;
            // waterAudio.mute = false;
            landAudio.volume = Mathf.Lerp(landAudio.volume, 0f, 0.02f);
            waterAudio.volume = Mathf.Lerp(waterAudio.volume, 1f, 0.02f);
            boat.SetActive(true);
            animator.SetBool("isWalkingForward", false);
        }


        // wake and wind particle controller
        if (Mathf.Abs(speed) >= 30f && vertical != 0) { 

            if (isSwimming) { 
                wake_effect = Instantiate(wake, new Vector3(groundCheck.transform.position.x, groundCheck.transform.position.y, groundCheck.transform.position.z), wake.transform.rotation);
            }

            if (isGrounded) { 
                Destroy(wake_effect); 
            }

        }

        float speedProportion = (isSwimming) ? speed / MAX_BOAT_SPEED : speed / MAX_WALK_SPEED;
        float omegaProportion = (isSwimming) ? omega / MAX_BOAT_OMEGA : omega / MAX_WALK_OMEGA;

        // Rotate the boat
        theta += omega * (1f + Mathf.Abs(speedProportion)) * Time.deltaTime;
        float pitch = (isSwimming) ? -Mathf.Max(0, speedProportion) * MAX_BOAT_PITCH : -Mathf.Max(0, speedProportion) * MAX_WALK_PITCH;
        float roll = (isSwimming) ? -omegaProportion * MAX_BOAT_ROLL : -omegaProportion * MAX_WALK_ROLL;
        transform.rotation = Quaternion.Euler(pitch, theta, roll);

        // Move the boat
        Vector3 velocity_point = transform.forward * speed * Time.deltaTime;
        Vector3 move = new Vector3(velocity_point.x, 0f, velocity_point.z);
        controller.Move(move);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Jumping
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = 6f;
        }

        controller.Move(velocity * Time.deltaTime);

        // Increase camera FOV depending on speed
        cam.fieldOfView = Mathf.SmoothStep(40f, 50f, speed / MAX_BOAT_SPEED);
    }
}
