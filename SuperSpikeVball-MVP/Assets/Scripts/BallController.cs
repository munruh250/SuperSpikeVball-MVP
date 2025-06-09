using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class BallController : MonoBehaviour
{
    // Fired when the ball first hits the ground
    public event Action OnGroundHit;

    [Header("Arc Settings")]
    [Tooltip("Initial speed of the ball when launched.")]
    public float initialSpeed = 8f;
    [Tooltip("Direction in which the ball is launched (will be normalized).")]
    public Vector3 initialDirection = new Vector3(0, 1, 0);

    [Header("Bounce Settings")]
    [Tooltip("0 = no bounce; 1 = perfect elastic bounce.")]
    [Range(0f, 1f)]
    public float bounceDampening = 0.8f;
    [Tooltip("Minimum vertical velocity on bounce.")]
    public float minBounceVelocity = 2f;

    [Header("Friction Settings")]
    [Tooltip("Horizontal velocity dampening on bounce.")]
    [Range(0f, 1f)]
    public float horizontalDampening = 0.8f;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Launches the ball using the default direction & speed.
    /// Ensures rb is assigned for edit‐mode tests.
    /// </summary>
    public void Launch()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.velocity = initialDirection.normalized * initialSpeed;
    }

    /// <summary>
    /// Launches the ball with a custom direction & speed.
    /// Ensures rb is assigned for edit‐mode tests.
    /// </summary>
    public void Launch(Vector3 direction, float speed)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.velocity = direction.normalized * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        string tag = collision.collider.tag;

        if (tag == "Net")
        {
            Debug.Log("Ball hit the net!");
            return;
        }

        if (tag == "Ground")
        {
            OnGroundHit?.Invoke();
            DoManualBounce(collision);
            return;
        }

        if (tag == "Player")
        {
            Debug.Log("Ball contacted player—handled by PlayerController");
            return;
        }

        DoManualBounce(collision);
    }

    private void DoManualBounce(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Vector3 normal = contact.normal.normalized;

        Vector3 v = rb.velocity;
        float velN = Vector3.Dot(v, normal);
        Vector3 vTangent = v - normal * velN;

        vTangent *= horizontalDampening;

        float bounceVel = Mathf.Abs(velN) * bounceDampening;
        if (bounceVel < minBounceVelocity)
            bounceVel = initialSpeed * bounceDampening;

        rb.velocity = vTangent + normal * bounceVel;
    }
}
