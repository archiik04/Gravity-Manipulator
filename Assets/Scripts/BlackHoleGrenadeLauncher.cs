using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit; // For VR interaction

public class BlackHoleGrenadeLauncher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject blackHoleGrenadePrefab;
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private Transform teleportDestination;
    
    [Header("XR Input")]
    [SerializeField] private XRController controller; // Reference to the VR controller
    [SerializeField] private InputHelpers.Button fireButton = InputHelpers.Button.Trigger; // Default to trigger
    
    [Header("Launcher Settings")]
    [SerializeField] private float launchForce = 15f;
    [SerializeField] private float cooldown = 2f;
    [SerializeField] private int maxSimulationSteps = 200;
    [SerializeField] private float timeStep = 0.025f;
    [SerializeField] private LayerMask collisionMask;

    private bool canFire = true;
    private List<Vector3> trajectoryPoints = new List<Vector3>();
    private bool isFiring = false;

    private void Update()
    {
        // Update the trajectory visualization
        SimulateTrajectory();

        // Check for VR controller input
        CheckControllerInput();
    }

    private void CheckControllerInput()
    {
        if (controller != null && canFire)
        {
            bool triggerPressed = false;
            InputHelpers.IsPressed(controller.inputDevice, fireButton, out triggerPressed);
            
            if (triggerPressed && !isFiring)
            {
                isFiring = true;
                FireGrenade();
                StartCoroutine(CooldownRoutine());
            }
            else if (!triggerPressed)
            {
                isFiring = false;
            }
        }
    }

    private void SimulateTrajectory()
    {
        trajectoryPoints.Clear();
        
        // Initial values
        Vector3 position = firePoint.position;
        Vector3 velocity = firePoint.forward * launchForce;
        float gravityY = Physics.gravity.y;

        trajectoryPoints.Add(position);

        for (int i = 0; i < maxSimulationSteps; i++)
        {
            // Calculate position in next step
            Vector3 nextPosition = position + velocity * timeStep;
            
            // Apply gravity to velocity
            velocity += Vector3.up * gravityY * timeStep;

            // Check for collision
            if (Physics.Linecast(position, nextPosition, out RaycastHit hit, collisionMask))
            {
                // Add collision point to the trajectory
                trajectoryPoints.Add(hit.point);

                // Calculate reflected velocity for bounce
                Vector3 reflectedVelocity = Vector3.Reflect(velocity, hit.normal);
                velocity = reflectedVelocity * 0.8f; // Reduce velocity slightly after bounce
                position = hit.point + hit.normal * 0.1f; // Offset slightly from the surface
            }
            else
            {
                position = nextPosition;
                trajectoryPoints.Add(position);
            }
        }

        // Update the line renderer
        trajectoryLine.positionCount = trajectoryPoints.Count;
        trajectoryLine.SetPositions(trajectoryPoints.ToArray());
    }

    private void FireGrenade()
    {
        GameObject grenade = Instantiate(blackHoleGrenadePrefab, firePoint.position, firePoint.rotation);
        BlackHoleGrenade grenadeComponent = grenade.GetComponent<BlackHoleGrenade>();
        
        if (grenadeComponent != null)
        {
            grenadeComponent.Initialize(launchForce, teleportDestination);
        }
        
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = firePoint.forward * launchForce;
        }
    }

    private IEnumerator CooldownRoutine()
    {
        canFire = false;
        yield return new WaitForSeconds(cooldown);
        canFire = true;
    }
}