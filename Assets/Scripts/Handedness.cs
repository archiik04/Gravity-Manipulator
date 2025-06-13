using UnityEngine;

// This should be in its own script or at the top of the Handedness.cs file
public enum Handed
{
    Left,
    Right
}


public class Handedness : MonoBehaviour
{
    public Handed handed;  // Enum to store whether the player is Left or Right handed.
    
    [SerializeField] private GameObject[] leftHandedObjects;  // Objects to be shown when left-handed.
    [SerializeField] private GameObject[] rightHandedObjects;  // Objects to be shown when right-handed.

    private void Awake()
    {
        // Based on the handedness, we activate or deactivate objects for Left or Right hand.
        if (handed == Handed.Left)
        {
            // Activate left-handed objects and deactivate right-handed objects.
            SetHandedness(leftHandedObjects, rightHandedObjects);
        }
        else
        {
            // Activate right-handed objects and deactivate left-handed objects.
            SetHandedness(rightHandedObjects, leftHandedObjects);
        }
    }

    // Helper method to set the active state of handedness objects.
    private void SetHandedness(GameObject[] activateObjects, GameObject[] deactivateObjects)
    {
        foreach (var obj in activateObjects)
        {
            obj.SetActive(true);
        }

        foreach (var obj in deactivateObjects)
        {
            obj.SetActive(false);
        }
    }
}
