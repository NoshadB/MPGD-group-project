using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    private Animator animator;
    int isWalkingHash;
    int isRunningHash;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        air
    }

    public void Start()
    {

        // Find the "Player" object (you can use GameObject.Find, FindGameObjectWithTag, etc.)
        Transform player = GameObject.Find("Player").transform;

        if (player != null)
        {
            // Find the "PlayerObj" object as a child of "Player"
            Transform playerObj = player.Find("PlayerObj");

            if (playerObj != null)
            {
                // Find the "Y Bot" object as a child of "PlayerObj"
                Transform yBot = playerObj.Find("Y Bot");

                if (yBot != null)
                {
                    // Get the Animator component from "Y Bot"
                    animator = yBot.GetComponent<Animator>();

                    if (animator != null)
                    {
                        // You now have the Animator component from "Y Bot"
                        Debug.Log("Animator found!");
                    }
                    else
                    {
                        Debug.LogError("Animator component not found on 'Y Bot'.");
                    }
                }
                else
                {
                    Debug.LogError("'Y Bot' not found under 'PlayerObj'.");
                }
            }
            else
            {
                Debug.LogError("'PlayerObj' not found under 'Player'.");
            }
        }
        else{
            Debug.LogError("'Player' not found in the hierarchy.");
        }

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        ResetJump();
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position + new Vector3(0, 0.05f, 0), Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0;
        }
        
        //debug
        Debug.Log(grounded);
        if (Input.GetKey(jumpKey))
            Debug.Log(jumpKey);


        //animation for walking ctr k + c
        if (moveDirection != Vector3.zero && moveSpeed<= 7)
        {
            animator.SetBool("IsMoving", true);
            animator.SetBool("IsRunning", false);
        }
        else if ((moveDirection != Vector3.zero && moveSpeed > 7))
        {
            animator.SetBool("IsMoving", true);
            animator.SetBool("IsRunning", true);
        }
        else
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsRunning", false);
        }

    }

    private void FixedUpdate() {
        MovePlayer();
    }
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

    }

    private void StateHandler()
    {
        //sprinting 
        if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        //walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        //air
        else
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        //calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //on ground
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
            

        //in air
        else if (!grounded)
            { rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force); }
    }

    private void SpeedControl()
    {
         Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity here
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        //reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }

    private void ResetJump()
    {
        readyToJump = true;
    }
}
