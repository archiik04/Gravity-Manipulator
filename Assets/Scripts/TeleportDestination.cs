using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportDestination : MonoBehaviour
{
    [SerializeField] private Color gizmoColor = new Color(0, 1, 1, 0.5f);
    [SerializeField] private float gizmoRadius = 3f;
    [SerializeField] private ParticleSystem teleportEffect;

    public void TriggerEffect()
    {
        if (teleportEffect != null)
        {
            teleportEffect.Play();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(transform.position, gizmoRadius);
    }
}