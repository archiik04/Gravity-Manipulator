using System.Collections.Generic;
using UnityEngine;

public enum TypeofGun
{
    Pull,
    Push,
}

public class GravityGun : MonoBehaviour
{
    [Header("References")]
    public GravityGun pairedGun;

    [SerializeField] private Transform Barrel;
    [SerializeField] private GameObject _Light;

    [Header("Settings")]
    [SerializeField] private float _maxDistance = 3f;
    [SerializeField] private float attractionForce = 2f;
    [SerializeField] private float radius = 0.6f;
    [SerializeField] private TypeofGun _guntype;

    private bool grabbed = false;
    private Vector3 HitPoint;
    private float currentdistance;
    private bool _activated = false;

    private List<Rigidbody> affectedRigidbodies = new List<Rigidbody>();
    private static List<Rigidbody> allAffectedRigidbodies = new List<Rigidbody>();

    private void Start()
    {
        currentdistance = _maxDistance;
        _Light.SetActive(false);
    }

    private void Update()
    {
        // If no guns are active, reset all affected objects
        if (!AnyGunActive())
        {
            ResetAllObjectsGravity();
            return;
        }

        if (!_activated || !grabbed) return;

        if (pairedGun != null && pairedGun._activated)
        {
            FloatModeControl();
        }
        else
        {
            NormalPushPull();
        }
    }

    private bool AnyGunActive()
    {
        return (_activated || (pairedGun != null && pairedGun._activated));
    }

    private void NormalPushPull()
    {
        RaycastHit hit;
        Vector3 aimPoint = Barrel.position + Barrel.forward * currentdistance;
        Debug.DrawLine(Barrel.position, aimPoint, Color.green);

        if (Physics.Raycast(Barrel.position, Barrel.forward, out hit, currentdistance))
        {
            aimPoint = hit.point;
        }

        HitPoint = aimPoint;

        Collider[] targets = Physics.OverlapSphere(aimPoint, radius);
        if (targets.Length > 0)
        {
            ApplyForces(aimPoint, targets);

            currentdistance = _guntype == TypeofGun.Pull
                ? Mathf.Max(1f, currentdistance - 0.008f)
                : currentdistance + 0.008f;
        }
        else
        {
            ResetAffectedObjectsGravity();
        }
    }

    private void FloatModeControl()
    {
        if (pairedGun == null || pairedGun.Barrel == null) return;

        Vector3 centerPoint = (Barrel.position + pairedGun.Barrel.position) / 2f;
        Vector3 avgDirection = ((Barrel.forward + pairedGun.Barrel.forward) / 2f).normalized;

        Collider[] targets = Physics.OverlapSphere(centerPoint, radius + 0.5f);
        foreach (var col in targets)
        {
            if (!col.CompareTag("Sphere")) continue;

            Rigidbody rb = col.attachedRigidbody;
            if (rb == null || affectedRigidbodies.Contains(rb)) continue;

            rb.useGravity = false;
            affectedRigidbodies.Add(rb);
            if (!allAffectedRigidbodies.Contains(rb))
                allAffectedRigidbodies.Add(rb);
        }

        foreach (var rb in affectedRigidbodies)
        {
            if (rb == null || !rb.CompareTag("Sphere")) continue;

            Vector3 toCenter = (centerPoint - rb.position).normalized;
            Vector3 moveDir = (toCenter + avgDirection * 1.5f).normalized;

            Vector3 movePos = rb.position + moveDir * (attractionForce * Time.deltaTime);
            movePos.y = Mathf.Lerp(rb.position.y, centerPoint.y + 1f, 0.05f);

            rb.velocity = Vector3.zero;
            rb.MovePosition(movePos);
        }
    }

    private void ApplyForces(Vector3 center, Collider[] colliders)
    {
        foreach (var col in colliders)
        {
            if (!col.CompareTag("Sphere")) continue;

            Rigidbody rb = col.attachedRigidbody;
            if (rb != null && !affectedRigidbodies.Contains(rb))
            {
                rb.useGravity = false;
                affectedRigidbodies.Add(rb);
                if (!allAffectedRigidbodies.Contains(rb))
                    allAffectedRigidbodies.Add(rb);
            }
        }

        foreach (var rb in affectedRigidbodies)
        {
            if (rb == null || !rb.CompareTag("Sphere")) continue;

            Vector3 dirToGun = (Barrel.position - rb.position).normalized;
            Vector3 dirFromGun = (rb.position - Barrel.position).normalized;

            Vector3 forceDir = _guntype == TypeofGun.Pull ? dirToGun : dirFromGun;
            rb.MovePosition(rb.position + forceDir * (attractionForce * Time.deltaTime));
        }
    }

    private void ResetAffectedObjectsGravity()
    {
        for (int i = affectedRigidbodies.Count - 1; i >= 0; i--)
        {
            if (affectedRigidbodies[i] != null)
                affectedRigidbodies[i].useGravity = true;

            affectedRigidbodies.RemoveAt(i);
        }

        currentdistance = _maxDistance;
    }

    private void ResetAllObjectsGravity()
    {
        for (int i = allAffectedRigidbodies.Count - 1; i >= 0; i--)
        {
            if (allAffectedRigidbodies[i] != null)
                allAffectedRigidbodies[i].useGravity = true;
        }

        allAffectedRigidbodies.Clear();
    }

    public void Selected() => grabbed = true;
    public void Unselected() => grabbed = false;

    public void _Activated()
    {
        _activated = true;
        _Light.SetActive(true);
    }

    public void _Deactivated()
    {
        _activated = false;
        _Light.SetActive(false);
        ResetAffectedObjectsGravity();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(HitPoint, radius);
    }
#endif
}
