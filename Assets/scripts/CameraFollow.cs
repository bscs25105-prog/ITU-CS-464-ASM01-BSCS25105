using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 offset = new Vector3(0f, 4f, -7f);
    [SerializeField] float sharpness = 5f;
    [SerializeField] float lookAhead = 0.35f;

    Rigidbody targetRb;
    Vector3 aimSmooth;

    void Awake()
    {
        if (target == null)
        {
            PlayerController p = FindAnyObjectByType<PlayerController>();
            if (p != null) target = p.transform;
        }
        if (target != null) targetRb = target.GetComponent<Rigidbody>();
        if (target != null) aimSmooth = target.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 vel = targetRb != null ? targetRb.linearVelocity : Vector3.zero;
        vel.y = 0f;
        Vector3 aim = target.position + vel * lookAhead;

        float blend = 1f - Mathf.Exp(-sharpness * Time.deltaTime);
        aimSmooth = Vector3.Lerp(aimSmooth, aim, blend);

        transform.position = aimSmooth + offset;
        transform.LookAt(aimSmooth);
                                    
    }
}
