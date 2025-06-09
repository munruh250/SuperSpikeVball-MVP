using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float rotationSpeed = 10f;

    [Header("Serve Zones")]
    public float zone1MinX = -13f, zone1MaxX = -8.5f, zone1MinZ = -4f, zone1MaxZ = 4f;
    public float zone2MinX = 8.5f, zone2MaxX = 13f, zone2MinZ = -4f, zone2MaxZ = 4f;

    [Header("Ball Hold Offset")]
    [Tooltip("Local offset where the ball sits when held")]
    public Vector3 holdOffset = new Vector3(-0.28f, 0.92f, 0.18f);

    [Header("Ideal Hit Offset & Forgiveness")]
    [Tooltip("Where the ball should be for a perfect hit, relative to player root.")]
    public Vector3 idealHitOffset = new Vector3(0.5f, 1.2f, 0f);
    [Tooltip("Max distance from ideal hit for full accuracy.")]
    public float hitRadius = 2f;

    [Header("Serve Toss Settings")]
    public BallController ballController;
    public float minTossSpeed = 4f, maxTossSpeed = 10f, maxChargeTime = 2f;
    [Tooltip("Direction axis for the toss (plus upward arc)")]
    public Vector3 tossDirAxis = Vector3.up;

    [Header("Spike Settings")]
    [Tooltip("Base speed given to the ball on a spike")]
    public float spikeBaseSpeed = 12f;
    [Tooltip("Upward boost on a spike")]
    public float spikeUpward = 1f;

    [Header("Debug UI")]
    [Tooltip("Optional UI Text for live accuracy feedback")]
    public UnityEngine.UI.Text accuracyText;

    // Cached components
    private Rigidbody rb;
    private Animator animator;
    private Rigidbody ballRb;
    private Collider ballCollider;
    private PlayerInputActions input;

    // Input & state
    private Vector2 moveInput;
    private bool jumpPressed;
    private float chargeStart;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (ballController)
        {
            ballRb = ballController.GetComponent<Rigidbody>();
            ballRb.isKinematic = true;
            ballCollider = ballController.GetComponent<Collider>();
            ballCollider.enabled = false;
        }

        // Setup input
        input = new PlayerInputActions();
        input.Enable();
        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled  += ctx => moveInput = Vector2.zero;
        input.Player.Jump.performed  += ctx => jumpPressed = true;
        input.Player.Spike.performed += ctx => OnSpikePressed();
        input.Player.Spike.canceled  += ctx => OnSpikeReleased();
        input.Player.Bump.performed  += ctx => animator.SetTrigger("Bump");
        input.Player.Set.performed   += ctx => animator.SetTrigger("Set");
    }

    void OnDisable()
    {
        input.Disable();
    }

    void Update()
    {
        var state = RallyManager.Instance.State;
        // Hold ball in hand
        if ((state == RallyState.PreServe || state == RallyState.TossCharging) && ballController)
        {
            ballRb.isKinematic = true;
            ballCollider.enabled = false;
            ballController.transform.position = transform.position
                + transform.right * holdOffset.x
                + transform.up * holdOffset.y
                + transform.forward * holdOffset.z;
        }
    }

    void FixedUpdate()
    {
        var state = RallyManager.Instance.State;

        // Movement
        Vector3 v = new Vector3(moveInput.x * moveSpeed, rb.velocity.y, moveInput.y * moveSpeed);
        rb.velocity = v;

        // Clamp pre-serve
        if (state == RallyState.PreServe)
        {
            int s = RallyManager.Instance.servingTeam;
            float minX = (s == 1) ? zone1MinX : zone2MinX;
            float maxX = (s == 1) ? zone1MaxX : zone2MaxX;
            float minZ = (s == 1) ? zone1MinZ : zone2MinZ;
            float maxZ = (s == 1) ? zone1MaxZ : zone2MaxZ;
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, minX, maxX);
            p.z = Mathf.Clamp(p.z, minZ, maxZ);
            rb.MovePosition(p);
        }

        // Jump
        if (jumpPressed && Mathf.Abs(rb.velocity.y) < 0.05f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetTrigger("Jump");
        }
        jumpPressed = false;

        // Facing & run anim
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Quaternion tgt = Quaternion.LookRotation(new Vector3(moveInput.x, 0, moveInput.y));
            transform.rotation = Quaternion.Slerp(transform.rotation, tgt, rotationSpeed * Time.deltaTime);
        }
        animator.SetFloat("Speed", moveInput.magnitude);
    }

    private void OnSpikePressed()
    {
        var state = RallyManager.Instance.State;
        if (state == RallyState.PreServe)
        {
            // Start toss charge
            chargeStart = Time.time;
            animator.SetBool("IsCharging", true);
            RallyManager.Instance.BeginTossCharge();
        }
        else if (state == RallyState.Tossed)
        {
            // Always play spike
            animator.SetTrigger("Spike");
            RallyManager.Instance.SpikeInFlight();

            // Accuracy based on ideal offset
            Vector3 idealPos = transform.position
                + transform.right * idealHitOffset.x
                + transform.up    * idealHitOffset.y
                + transform.forward * idealHitOffset.z;
            float dist = Vector3.Distance(idealPos, ballController.transform.position);
            float accuracy = 1f - Mathf.Clamp01(dist / hitRadius);
            Debug.Log($"Hit dist={dist:F2}m → accuracy={accuracy:P0}");

            if (accuracyText != null)
            {
                accuracyText.text = $"Acc: {accuracy:P0}";
                accuracyText.color = Color.Lerp(Color.red, Color.green, accuracy);
            }

            // Launch toward opponent
            int server = RallyManager.Instance.servingTeam;
            Vector3 forwardDir = (server == 1) ? Vector3.right : Vector3.left;
            Vector3 spikeDir = (forwardDir + Vector3.up * spikeUpward).normalized;
            float launchSpeed = spikeBaseSpeed * Mathf.Lerp(0.5f, 1.5f, accuracy);
            ballController.Launch(spikeDir, launchSpeed, false);
        }
    }

    private void OnSpikeReleased()
    {
        var state = RallyManager.Instance.State;
        if (state != RallyState.TossCharging) return;

        float held = Time.time - chargeStart;
        float t = Mathf.Clamp01(held / maxChargeTime);
        float speed = Mathf.Lerp(minTossSpeed, maxTossSpeed, t);
        Vector3 dir = tossDirAxis + Vector3.up * t;

        ballRb.isKinematic = false;
        ballCollider.enabled = true;
        ballController.Launch(dir, speed, true);

        animator.SetBool("IsCharging", false);
        RallyManager.Instance.ReleaseToss();
    }

    void OnDrawGizmosSelected()
    {
        // Draw ideal hit sphere
        Gizmos.color = Color.cyan;
        Vector3 idealPos = transform.position
            + transform.right * idealHitOffset.x
            + transform.up    * idealHitOffset.y
            + transform.forward * idealHitOffset.z;
        Gizmos.DrawWireSphere(idealPos, hitRadius);
    }
}
