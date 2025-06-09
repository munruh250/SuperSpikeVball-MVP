using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;

    [Header("Serve Settings")]
    public BallController ballController;
    public float minTossSpeed = 4f;
    public float maxTossSpeed = 10f;
    public float maxChargeTime = 2f;
    public float serveSpeed = 12f;
    public Vector3 serveDirectionAxis = new Vector3(0, 0, 1);

    [Header("Hold Offsets")]
    public float holdOffsetX = 0.5f;
    public float holdOffsetY = 1.5f;
    public float holdOffsetZ = 0.5f;

    [Header("Arc Settings")]
    [Range(0f, 1f)] public float maxArcY = 1f;
    [Range(0f, 1f)] public float minArcY = 0f;

    // Internal state
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool ballInHand = true;
    private bool isCharging = false;
    private bool hasServed = false;
    private bool movementLocked = false;
    private float chargeStart = 0f;

    private Rigidbody rb;
    private Animator animator;
    private Collider ballCollider;
    private PlayerInputActions inputActions;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Input setup
        inputActions = new PlayerInputActions();
        inputActions.Enable();
        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled  += ctx => moveInput = Vector2.zero;
        inputActions.Player.Jump.performed  += ctx =>
        {
            jumpPressed = true;
            Debug.Log("[Input] Jump pressed – triggering animation");
            animator.SetTrigger("Jump");
        };
        inputActions.Player.Spike.performed += ctx => OnServePressed();
        inputActions.Player.Spike.canceled  += ctx => OnServeReleased();
        inputActions.Player.Bump.performed  += ctx => animator.SetTrigger("Bump");
        inputActions.Player.Set.performed   += ctx => animator.SetTrigger("Set");

        // Prepare ball for holding
        if (ballController)
        {
            var ballRb = ballController.GetComponent<Rigidbody>();
            ballRb.isKinematic = true;
            ballCollider = ballController.GetComponent<Collider>();
            ballCollider.enabled = false;
            ballController.OnGroundHit += UnlockMovement;
        }
    }

    void OnDisable()
    {
        inputActions.Disable();
        if (ballController)
            ballController.OnGroundHit -= UnlockMovement;
    }

    void Update()
    {
        // Snap the held ball to the offset position
        if (ballInHand && ballController)
        {
            Vector3 offset = transform.right * holdOffsetX
                           + transform.up    * holdOffsetY
                           + transform.forward * holdOffsetZ;
            ballController.transform.position = transform.position + offset;
        }
    }

    void FixedUpdate()
    {
        // Apply horizontal movement if not locked
        if (!movementLocked)
        {
            Vector3 v = rb.velocity;
            v.x = moveInput.x * moveSpeed;
            v.z = moveInput.y * moveSpeed;
            rb.velocity = v;
        }
        else
        {
            // Freeze horizontal while locked
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        // Jump physics
        if (jumpPressed && Mathf.Abs(rb.velocity.y) < 0.05f)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpPressed = false;

        animator.SetFloat("Speed", moveInput.magnitude);
    }

    private void OnServePressed()
    {
        if (!ballController) return;

        if (ballInHand)
        {
            // Begin toss charge: lock movement
            isCharging = true;
            movementLocked = true;
            chargeStart = Time.time;
            Debug.Log($"Charge start at {chargeStart:F2}s");
            animator.SetBool("IsCharging", true);
        }
        else
        {
            // Always play spike animation
            animator.SetTrigger("Spike");
            Debug.Log("Spike animation triggered");

            // Only launch serve once
            if (!hasServed)
            {
                hasServed = true;
                UnlockMovement();
                ballCollider.enabled = true;
                var brb = ballController.GetComponent<Rigidbody>();
                brb.isKinematic = false;

                // Compute arc based on left stick Y
                float arcInput = Mathf.Clamp01(1f - moveInput.y);
                float arcY = Mathf.Lerp(minArcY, maxArcY, arcInput);
                Vector3 dir = new Vector3(serveDirectionAxis.x, arcY, serveDirectionAxis.z);

                Debug.Log($"Serving with arcY={arcY:F2}, dir={dir}, speed={serveSpeed}");
                ballController.Launch(dir, serveSpeed);
            }
        }
    }

    private void OnServeReleased()
    {
        if (ballInHand && isCharging)
        {
            // Compute toss velocity
            float held    = Time.time - chargeStart;
            float t       = Mathf.Clamp01(held / maxChargeTime);
            float tossVel = Mathf.Lerp(minTossSpeed, maxTossSpeed, t);
            Debug.Log($"Charge held {held:F2}s → toss speed {tossVel:F2}");

            // Toss upward
            ballCollider.enabled = true;
            var brb = ballController.GetComponent<Rigidbody>();
            brb.isKinematic = false;
            Vector3 offset = transform.right * holdOffsetX
                           + transform.up    * holdOffsetY
                           + transform.forward * holdOffsetZ;
            ballController.transform.position = transform.position + offset;
            ballController.Launch(Vector3.up, tossVel);

            ballInHand = false;
            isCharging = false;
            animator.SetBool("IsCharging", false);
        }
    }

    private void UnlockMovement()
    {
        movementLocked = false;
    }
}
