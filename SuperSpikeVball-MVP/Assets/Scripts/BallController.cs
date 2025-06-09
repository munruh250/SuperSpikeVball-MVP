using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BallController : MonoBehaviour
{
    public event Action OnGroundHit;
    public event Action OnOutOfBounds;

    [Header("Arc Settings")]
    public float initialSpeed = 8f;
    public Vector3 initialDirection = Vector3.up;

    [Header("Gravity & Bounce Tuning")]
    [Tooltip("Gravity multiplier during the toss arc.")]
    [Range(0f, 2f)] public float tossGravityScale = 0.5f;
    [Tooltip("Use Unity gravity after first contact.")]
    public bool restoreDefaultGravityAfterContact = true;

    [Range(0f,1f)] public float bounceDampening = 0.8f;
    [Range(0f,1f)] public float horizontalDampening = 0.8f;
    public float minBounceVelocity = 2f;

    private Rigidbody rb;
    private bool useCustomGravity = false;
    private bool firstContactFired = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>Launch with default arc</summary>
    public void Launch() => Launch(initialDirection, initialSpeed, true);

    /// <summary>Launch with given direction/speed. If isToss=true, apply lighter gravity.</summary>
    public void Launch(Vector3 direction, float speed, bool isToss = false)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.velocity = direction.normalized * speed;
        firstContactFired = false;

        // Toggle custom gravity only during the toss
        useCustomGravity = isToss;
        rb.useGravity = !isToss;
    }

    void FixedUpdate()
    {
        // If we're in the toss arc, apply custom gravity each physics step
        if (useCustomGravity)
            rb.AddForce(Physics.gravity * tossGravityScale, ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision collision)
    {
        string tag = collision.collider.tag;

        // Fire first-contact events only once
        if (!firstContactFired)
        {
            if (tag == "Ground")
            {
                OnGroundHit?.Invoke();
                RallyManager.Instance.BallFirstContact();
                firstContactFired = true;
            }
            else if (tag == "OutOfBounds")
            {
                OnOutOfBounds?.Invoke();
                RallyManager.Instance.BallFirstContact();
                firstContactFired = true;
            }

            // Once we’ve hit ground or OOB, restore normal gravity if desired
            if (firstContactFired && restoreDefaultGravityAfterContact)
            {
                useCustomGravity = false;
                rb.useGravity = true;
            }
        }

        if (tag == "Net")
        {
            Debug.Log("Ball hit the net!");
            return;
        }

        // Standard bounce behavior
        if (tag == "Ground" || tag == "Wall")
            DoBounce(collision);
    }

    private void DoBounce(Collision collision)
    {
        ContactPoint cp = collision.contacts[0];
        Vector3 n = cp.normal.normalized;
        Vector3 v = rb.velocity;

        // Decompose into normal and tangential components
        float vn = Vector3.Dot(v, n);
        Vector3 vt = v - n * vn;
        vt *= horizontalDampening;

        // Bounce vertical
        float bvy = Mathf.Abs(vn) * bounceDampening;
        if (bvy < minBounceVelocity)
            bvy = initialSpeed * bounceDampening;

        rb.velocity = vt + n * bvy;
    }
}
