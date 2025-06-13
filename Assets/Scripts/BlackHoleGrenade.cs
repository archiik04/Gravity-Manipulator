using UnityEngine;

public class BlackHoleGrenade : MonoBehaviour
{
    private float range;
    private Transform teleportTarget;

    public void Initialize(float range, Transform teleportTarget)
    {
        this.range = range;
        this.teleportTarget = teleportTarget;
    }

    void Update()
    {
        // Check for nearby objects to attract
        Collider[] colliders = Physics.OverlapSphere(transform.position, range);
        foreach (var col in colliders)
        {
            if (col.CompareTag("Sphere"))
            {
                Rigidbody rb = col.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Apply force towards the grenade
                    Vector3 direction = (transform.position - col.transform.position).normalized;
                    rb.AddForce(direction * 10f * Time.deltaTime, ForceMode.Force);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Sphere"))
        {
            // Teleport object when it reaches the black hole center
            other.transform.position = teleportTarget.position;
        }
    }
}
