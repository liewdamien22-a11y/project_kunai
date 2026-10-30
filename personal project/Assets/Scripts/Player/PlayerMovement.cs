using System.Collections;
using UnityEditor.Animations;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public enum movementState
    {
        idle, moving, onWall, sliding, dashing
    }
    public movementState currentState = movementState.idle;
    public PlayerStats PlayerStats;
    public CharacterController controller;
    public Transform cameraTransform;

    /*
    [Header("testing stuff for gun")]
    public Animator gunAnimator;
    public string shootTriggerName = "Shoot";
    */
    [Header("movement and looking around variables")]
    public float moveSpeed = 5.0f;
    public float lookSensitivity = 2.0f;//having this allows future functionality to change sensitivity
    public float gravity = -9.81f;
    public float jumpHeight = 2f;

    [Header("dash variables")]
    public float dashDistance = 10f;
    public float dashDuration = 0.10f;
    public float dashStaminaCost = 30f;
    public bool canDash = true;

    [Header("slide variables")]
    public float slideSpeed = 12f;
    public float slideDecay = 1f;//how much slide speed is lost per second
    public float slideStaminaCost = 15f;
    public float slideHeight = 1.0f;
    public float slideCameraY = 0.5f;
    public float slideThreshold = 2.5f;//speed at which slide self terminates
    public float slopeAcceleration = 15f;//speed boost when on slope
    public float slopeDecayMultiplier = 2.5f;//how much to decay speed when uphill
    public float slideBufferTime = 0.2f;//allow instant slide when: holding ctrl + hits ground after airborne
    public float slideBufferCounter;
    private float originalHeight;
    private Vector3 originalCenter;
    private Vector3 originalCameraPos;
    private Vector3 slideDirection;
    private float currentSlideSpeed;
    private float xRotation = 0f;
    public Vector3 velocity;

    [Header("wall run variables")]
    public LayerMask wallMask;
    public float wallRunSpeed = 8f;
    public float wallCheckDistance = 1.5f;
    public float wallGravityScale = 0.2f; // fraction of standard gravity applied during wall run
    public float wallJumpForce = 7f;
    public float wallJumpSideForce = 6f;

    // Wall running tracking variables
    private RaycastHit wallHit;
    private bool wallLeft;
    private bool wallRight;
    private Vector3 wallVector;
    /*
    [Header("simple animation stuff")]
    [SerializeField] private Animator gunAnimator;
    */

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (PlayerStats == null) PlayerStats = GetComponent<PlayerStats>();

        originalHeight = controller.height;
        originalCenter = controller.center;

        if (cameraTransform != null)
        {
            originalCameraPos = cameraTransform.localPosition;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleLooking();
        HandleMovement();
        
        HandleDash();
        HandleSlide();
        HandleWallRun();

    }
    /*
    void simpleAnimator()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            gunAnimator.Play("shootState");
        }
    }
    */
    void HandleLooking()//simple function to allow player to look around
    {
        float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

        xRotation = Mathf.Clamp(xRotation - mouseY, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        if (currentState == movementState.dashing || currentState == movementState.sliding) return;

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = (transform.right * x + transform.forward * z).normalized;

        if (controller.isGrounded)
        {
            velocity.x = moveDirection.x * moveSpeed;
            velocity.z = moveDirection.z * moveSpeed;  
        }

        if (Input.GetKey(KeyCode.Space) && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move((moveDirection * moveSpeed + velocity) * Time.deltaTime);

        if (currentState != movementState.sliding && currentState != movementState.dashing)
        {
            currentState = moveDirection.sqrMagnitude > 0.01f ? movementState.moving : movementState.idle;
        }
    }

    void HandleDash()//dash controller function, physics is handled by dash coroutine
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && currentState != movementState.dashing)
        {
            if (PlayerStats != null && PlayerStats.HasEnoughStamina(dashStaminaCost))
            {
                if (currentState == movementState.sliding)
                {
                    if (!CanStandUp()) return;
                    StopSlide();
                }

                PlayerStats.ConsumeStamina(dashStaminaCost);
                StartCoroutine(DashRoutine());
            }
        }
    }

    IEnumerator DashRoutine()//dash achieved via drawing a vector and moving the player along the vector(more consistent)
    {
        canDash = false;
        currentState = movementState.dashing;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 dashDirection = (transform.right * x + transform.forward * z).normalized;
        if (dashDirection == Vector3.zero) dashDirection = transform.forward;

        float dashSpeed = dashDistance / dashDuration;
        float timer = 0f;
        velocity.y = 0f;

        while (timer < dashDuration)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
        
        currentState = movementState.idle;
        canDash = true;
    }



    void HandleSlide()//controller function for slide
    {
        bool groundedForSlide = controller.isGrounded || Physics.Raycast(transform.position, Vector3.down, (controller.height / 2f) + 0.3f);
        bool wantToSlide = Input.GetKey(KeyCode.LeftControl);//allow player to slide regardless of in air or not 
        if (/*slideBufferCounter > 0f &&*/ wantToSlide && groundedForSlide && currentState != movementState.sliding && currentState != movementState.dashing && currentState != movementState.onWall)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            if ((x != 0 || z != 0) && PlayerStats != null && PlayerStats.HasEnoughStamina(slideStaminaCost))
            {
                slideBufferCounter = 0f;
                StartSlide();
            }
        }

        if (currentState == movementState.sliding)
        {
            UpdateSlide();

            bool keyHeld = Input.GetKey(KeyCode.LeftControl);
            bool speedExhausted = currentSlideSpeed <= slideThreshold;

            if (!keyHeld && CanStandUp())//when ctrl key is released: exit slide
            {
                StopSlide();
            }
        }
    }

    void StartSlide()//logic to initiate slide
    {
        currentState = movementState.sliding;
        controller.height = slideHeight;
        controller.center = new Vector3(originalCenter.x, slideHeight / 2f, originalCenter.z);

        if (cameraTransform != null)
        {
            cameraTransform.localPosition = new Vector3(originalCameraPos.x, slideCameraY, originalCameraPos.z);
        }

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        slideDirection = (transform.right * x + transform.forward * z).normalized;
        currentSlideSpeed = slideSpeed;
    }

    void UpdateSlide()//does exactly what its name suggests: updates and checks the slide
    {
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;

        RaycastHit groundHit;
        if (Physics.Raycast(transform.position, Vector3.down, out groundHit, (slideHeight / 2f) + 0.5f))
        {
            float slopeAngle = Vector3.Angle(Vector3.up, groundHit.normal);

            if (slopeAngle > 0f)
            {
                //dot product determines if slide direction is downhill (positive) or uphill (negative)
                float slideDot = Vector3.Dot(slideDirection, groundHit.normal);

                if (slideDot > 0f) //downhill
                {
                    currentSlideSpeed += (slopeAngle / 90f) * slopeAcceleration * Time.deltaTime;
                }
                else if (slideDot < 0f) //uphill
                {
                    currentSlideSpeed -= (slopeAngle / 90f) * (slideDecay * slopeDecayMultiplier) * Time.deltaTime;
                }
            }
        }

        Vector3 slideMove = slideDirection * currentSlideSpeed + Vector3.up * velocity.y;//draw slide path 
        controller.Move(slideMove * Time.deltaTime);

        //currentSlideSpeed = Mathf.MoveTowards(currentSlideSpeed, slideThreshold, slideDecay * Time.deltaTime);//decay slide speed by a defined decrement over time until it hits the slide threshold 

        if (Input.GetKeyDown(KeyCode.Space) && controller.isGrounded && CanStandUp())//allow player to jump to exit slide(whoohoo movement)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            StopSlide();
            
        }
    }

    void StopSlide()//player exits slide into idle state and can immediately perform other movement actions
    {
        currentState = movementState.idle;
        controller.height = originalHeight;
        controller.center = originalCenter;
        //velocity.y = 2f;

        if (cameraTransform != null)
        {
            cameraTransform.localPosition = originalCameraPos;
        }
    }

    bool CanStandUp()//prevent player getting stuck in world geometry when exiting slide
    {
        float castDistance = originalHeight - slideHeight;
        Vector3 rayStart = transform.position + Vector3.up * (slideHeight - controller.radius);
        return !Physics.SphereCast(new Ray(rayStart, Vector3.up), controller.radius, castDistance);
    }
    void HandleWallRun()
    {
        //input and airborne checks
        bool isAirborne = !controller.isGrounded;
        bool pressingForward = Input.GetAxisRaw("Vertical") > 0;
        bool pressingLeft = Input.GetKey(KeyCode.A);
        bool pressingRight = Input.GetKey(KeyCode.D);

        //player must be airbone and pressing W + A|D
        if (isAirborne && pressingForward && (pressingLeft || pressingRight))
        {
            //raycast left when pressing A and vice versa
            Vector3 checkDirection = pressingLeft ? -transform.right : transform.right;

            if (Physics.Raycast(transform.position, checkDirection, out wallHit, wallCheckDistance, wallMask))
            {
                if (currentState != movementState.onWall)
                {
                    currentState = movementState.onWall;
                    velocity.y = 0f; //reset vertical speed upon attaching to wall
                }

                //get wall vector via cross product
                Vector3 wallNormal = wallHit.normal;
                wallVector = Vector3.Cross(wallNormal, Vector3.up);

                //align wall vector with player's facing direction
                if (Vector3.Dot(wallVector, transform.forward) < 0)
                {
                    wallVector = -wallVector;
                }

                //reduce gravity on wall and move on wal
                velocity.y += (gravity * wallGravityScale) * Time.deltaTime;
                Vector3 wallRunVelocity = (wallVector * wallRunSpeed) + (Vector3.up * velocity.y);
                controller.Move(wallRunVelocity * Time.deltaTime);

                //jumping off wall logic
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    Vector3 wallJumpDir = (wallNormal * wallJumpSideForce) + (Vector3.up * wallJumpForce);
                    velocity = wallJumpDir;
                    StopWallRun();
                }

                return; //active wall run handled; exit early
            }
        }

        //exit wall run if raycast fails or inputs are released
        if (currentState == movementState.onWall)
        {
            StopWallRun();
        }
    }

    void StopWallRun()
    {
        currentState = movementState.idle;
    }
}