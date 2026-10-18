using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpHeight = 1.8f; 

    Rigidbody rb;
    Vector3 move;
    bool isGrounded;
    bool jumpQueued;
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBuffer = 0.12f;
    [SerializeField] float fallMultiplier = 2.2f;
    [SerializeField] float squashDuration = 0.25f;
    [SerializeField] Transform body;

    float lastGroundedTime = -1f;
    float lastPressedTime = -1f;
    float airTime;
    Coroutine squashRoutine;
    Vector3 lastGroundPoint, lastGroundNormal;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.sleepThreshold = 0f;
        if (body == null) body = transform.GetChild(0);
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        move = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);

        if (isGrounded && !jumpQueued && rb.linearVelocity.y <= 0f)
            lastGroundedTime = Time.time;

        if (Input.GetKeyDown(KeyCode.Space))
            lastPressedTime = Time.time;

        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        bool wantJump = Time.time - lastPressedTime <= jumpBuffer;

        if (canJump && wantJump)
        {
            jumpQueued = true;
            lastGroundedTime = -1f;  
            lastPressedTime = -1f;
        }

       
        if (!isGrounded)
            airTime += Time.deltaTime;
        else
        {
            if (airTime > 0.1f) StartSquash();
            airTime = 0f;
        }
        
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);
        if (jumpQueued)
        {
            jumpQueued = false;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            float g = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);
            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);
        }
        if (rb.linearVelocity.y < 0f)
            rb.AddForce(Physics.gravity * (fallMultiplier - 1f), ForceMode.Acceleration);
        isGrounded = false;
    }
    void OnCollisionStay(Collision col)
    {
        foreach (ContactPoint c in col.contacts)
        {
            if (c.normal.y > 0.5f)
            {
                isGrounded = true;
                lastGroundPoint = c.point;
                lastGroundNormal = c.normal;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (isGrounded)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(lastGroundPoint, lastGroundNormal * 3f);
            Gizmos.DrawWireSphere(lastGroundPoint, 0.15f);
        }
        else
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.8f);
        }
    }
    void StartSquash()
    {
        if (squashRoutine != null) StopCoroutine(squashRoutine);
        squashRoutine = StartCoroutine(Squash());
    }

    IEnumerator Squash()
    {
        for (float elapsed = 0f; elapsed < squashDuration; elapsed += Time.deltaTime)
        {
            float p = OutBack(elapsed / squashDuration);
            float y = Mathf.LerpUnclamped(0.7f, 1f, p);
            float xz = 1f / Mathf.Sqrt(y);                 
            body.localScale = new Vector3(xz, y, xz);
            body.localPosition = new Vector3(0f, -(1f - y), 0f);  
            yield return null;
        }
        body.localScale = Vector3.one;      
        body.localPosition = Vector3.zero;
    }

    static float OutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }

}