using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    /** Player TODO's
    ** 1. Walking
    2. Sprinting
    ** 3. Jumping
    ** 4. Cyote Time
    5. Wall Jumping
    6. Wall Sliding
    7. Dashing
    8. Crouching
    9. Crawling
    10. Climbing
    11. Swimming
    12. Diving
    13. Drowning
    14. Knockback
    15. Death
    16. Respawning
    17. Sliding
    */

    public enum PlayerState
    {
        Idle,
        Walking,
        Jumping,
        Falling,
        Sliding,
    }

    public PlayerState playerState;

    // Player Components
    public InputAction movementInput;
    public InputAction jumpInput;
    public InputAction crouchInput;

    // Player Movement Variables
    public float moveSpeed = 5f;
    public float initialJumpVerticalPosition;
    public float jumpVelocity = 5f;
    public float verticalVelocity;
    public float horizontalVelocity;
    public float rigidity = 1f;
    public float gravity = 9.8f;
    public float isGroundedRaycastDistance = 0.1f;

    // cyote time
    public float cyoteTime = 0.1f;
    public float cyoteTimeCounter = 0.5f;
    public float cyoteTimeInitialValue = 0.5f;


    // Start is called before the first frame update
    void Start()
    {
        playerState = PlayerState.Idle;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(horizontalVelocity);
        Move();
        switch (playerState)
        {
            case PlayerState.Walking:
                //Move();
                break;
            case PlayerState.Jumping:
                Jump();
                break;
            case PlayerState.Falling:
                Fall();
                break;
        }

        Momentum();

        // cyote time fall check
        if (isGrounded())
        {
            cyoteTimeCounter = cyoteTimeInitialValue;
        } else {
            if (playerState != PlayerState.Falling && cyoteTimeCounter > 0) {
                cyoteTimeCounter -= Time.deltaTime;
            }
            else if (playerState != PlayerState.Falling && cyoteTimeCounter <= 0) {
                playerState = PlayerState.Falling;
                cyoteTimeCounter = cyoteTimeInitialValue;
            }
        }
    }

    void Move()
    {
        float movementInputValue = movementInput.ReadValue<float>();

        if (movementInputValue == 0) {
            return;
        }

        horizontalVelocity = movementInputValue * moveSpeed * Time.deltaTime;

        Vector3 currentPos = transform.position;
        currentPos.x += horizontalVelocity;
        transform.position = currentPos;
    }

    void Jump()
    {
        float jumpInputValue = jumpInput.ReadValue<float>();

        Vector3 currentPos = transform.position;
        currentPos.y += verticalVelocity * Time.deltaTime;
        transform.position = currentPos;

        if (verticalVelocity <= 0 || jumpInputValue == 0)
        {
            playerState = PlayerState.Falling;
            verticalVelocity = 0;
        }

        verticalVelocity -= gravity * Time.deltaTime;
    }

    bool canJump()
    {
        return ( isGrounded() || cyoteTimeCounter > 0 ) && playerState != PlayerState.Falling;
    }

    void Fall()
    {
        Vector3 currentPos = transform.position;
        currentPos.y += verticalVelocity * Time.deltaTime;
        transform.position = currentPos;

        if (isGrounded())
        {
            playerState = PlayerState.Idle;
        }

        verticalVelocity -= gravity * Time.deltaTime;
    }

    bool isGrounded()
    {
        // return true if a raycast hits the ground
        if (Physics.Raycast(transform.position, Vector3.down, isGroundedRaycastDistance))
        {
            return true;
        }
        return false;
    }

    void Momentum() {
        
        //horizontalVelocity = Mathf.Lerp(horizontalVelocity, 0, rigidity * Time.deltaTime);
        horizontalVelocity = Mathf.MoveTowards(horizontalVelocity, 0, Mathf.Pow(Time.deltaTime * rigidity, 1.1f));

        if (Mathf.Abs(horizontalVelocity) < 0.01f) {
            horizontalVelocity = 0;
            return;
        }

        Vector3 currentPos = transform.position;
        currentPos.x += horizontalVelocity * moveSpeed * Time.deltaTime;
        transform.position = currentPos;
    }

    void OnEnable()
    {
        movementInput.Enable();
        jumpInput.Enable();
        crouchInput.Enable();

        // Change state to walking if possible
        movementInput.performed += ctx => {
            if (playerState != PlayerState.Jumping && playerState != PlayerState.Falling) { 
                playerState = PlayerState.Walking;
            }
        };

        movementInput.canceled += ctx => {
            if (playerState != PlayerState.Jumping && playerState != PlayerState.Falling) { 
                playerState = PlayerState.Idle;
            }
        };

        // Change state to jumping if possible
        jumpInput.performed += ctx => {
            if (canJump()) { 
                playerState = PlayerState.Jumping;
                verticalVelocity = jumpVelocity;
                initialJumpVerticalPosition = transform.position.y;
            }
        };
    }

    void OnDisable()
    {
        movementInput.Disable();
        jumpInput.Disable();
        crouchInput.Disable();
    }

}
