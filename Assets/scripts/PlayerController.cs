using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 5f;
    [SerializeField] float jumpHeight = 1.8f; 

    Rigidbody rb;
    Vector3 move;
    bool isGrounded;
    bool jumpQueued;
    Vector3 lastGroundPoint, lastGroundNormal;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.sleepThreshold = 0f;
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        move = Vector3.ClampMagnitude(new Vector3(h, 0f, v), 1f);
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            jumpQueued = true;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector3(move.x * speed, rb.linearVelocity.y, move.z * speed);
        if (jumpQueued)
        {
            jumpQueued = false;
            float g = -Physics.gravity.y;
            float v0 = Mathf.Sqrt(2f * g * jumpHeight);
            rb.AddForce(Vector3.up * rb.mass * v0, ForceMode.Impulse);
        }
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
    
}