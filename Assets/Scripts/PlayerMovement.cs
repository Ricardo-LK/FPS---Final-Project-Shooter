using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

//Todo: Level doors close when enter and open after some time/ kill enough enemies
//      If die or fall player sent to lower level
//          if dies there or alredy sended
//              player die 
//          else find teleporter
//              player is sent back
//              player alredy sended

public class PlayerMovement : MonoBehaviour
{

    [Header("Player Status")]
    [SerializeField] public int PlayerHealth;
    [SerializeField] public int MaxPlayerHealth;
    [SerializeField] int HealthLossRate;
    [SerializeField] int healthDecay;
    public bool secondChance;
    [SerializeField] GameObject SecondChancePos;
    public bool PlayerAlive;

    [Header("Movement")]
    [SerializeField] TMP_Text Speed;
    [SerializeField] Image RunningEffect;
    private float moveSpeed;
    public float WalkSpeed;
    public float SprintSpeed;
    public float SlideSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float SpeedIncreaseMultiplier;
    public float SlopeIncreaseMultiplier;

    public float GroundDrag;

    [Header("Jumping")]
    public float JumpForce;
    public float JumpCooldown;
    public float AirMultiplier;
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float PlayerHeight;
    public LayerMask IsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float MaxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Health Bar")]
    Image HealthBar;
    public Transform orientation;

    [Header("Timer")]
    public TextMeshProUGUI TimerText;
    float time;

    float horizontalInput;
    float verticalInput;


    Vector3 moveDirection;

    Rigidbody rb;

    [Header("State")]
    public MovementState state;
    public enum MovementState
    {
        WALKING,
        SPRINTING,
        SLIDING,
        AIR
    }

    public bool sliding;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        foreach (var image in GameObject.FindGameObjectWithTag("Canvas").GetComponentsInChildren<Image>(true))
        {
            if (image.name == "Health")
            {
                HealthBar = image;
            }
        };

        RunningEffect.gameObject.SetActive(false);
    }

    void Start()
    {
        PlayerAlive = true;
        secondChance = true;
        readyToJump = true;
        StartCoroutine(HealthLoss());
    }

    void Update()
    {
        if (PlayerHealth <= 0 && secondChance)
        {
            gameObject.transform.position = SecondChancePos.transform.position;
            PlayerHealth = MaxPlayerHealth;
            secondChance = false;
        }
        else if (PlayerHealth <= 0)
        {

            RunningEffect.gameObject.SetActive(false);
            PlayerAlive = false;
            sliding = false;
            //Destroy(gameObject);
        }

        time += Time.deltaTime;
        TimerText.SetText("Timer: " + Mathf.CeilToInt(time).ToString());

        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, PlayerHeight * 0.5f + 0.2f, IsGround);

        if (PlayerAlive)
        {
            MyInput();
            SpeedControl();
            StateHandler();
        }

        // handle drag
        if (grounded)
            rb.drag = GroundDrag;
        else
            rb.drag = 0;
    }

    void FixedUpdate()
    {
        if (PlayerAlive)
            MovePlayer();
    }

    void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), JumpCooldown);
        }
    }


    void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * AirMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();

    }

    void SpeedControl()
    {
        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * JumpForce, ForceMode.Impulse);
    }
    void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, PlayerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < MaxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    void StateHandler()
    {
        // Mode - Sliding
        if (sliding)
        {
            state = MovementState.SLIDING;

            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = SlideSpeed;

            else
                desiredMoveSpeed = SprintSpeed;
        }

        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.SPRINTING;
            desiredMoveSpeed = SprintSpeed;
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.WALKING;
            desiredMoveSpeed = WalkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.AIR;
        }

        // check if desiredMoveSpeed has changed drastically
        if (Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 6.0f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;

        if (lastDesiredMoveSpeed >= 13)
            RunningEffect.gameObject.SetActive(true);
        else
            RunningEffect.gameObject.SetActive(false);


        Speed.text = moveSpeed.ToString();
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smoothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            RunningEffect.gameObject.SetActive(true);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * SpeedIncreaseMultiplier * SlopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * SpeedIncreaseMultiplier;


            yield return null;
        }

        RunningEffect.gameObject.SetActive(false);
        moveSpeed = desiredMoveSpeed;
    }

    IEnumerator HealthLoss()
    {
        while (true)
        {
            yield return new WaitForSeconds(HealthLossRate);
            PlayerHealth -= healthDecay;
            HealthBar.fillAmount = (float)PlayerHealth / MaxPlayerHealth;
        }
    }
}