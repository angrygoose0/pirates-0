using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Target
{
    public GameObject targetObject;
    public Vector3 offset;
}

public class FollowMouse : MonoBehaviour
{
    public float speed = 5f; // Speed at which the object moves towards the mouse
    public float thresholdDistance = 4f; // Distance threshold for updating the targetObject's position
    public List<Target> targets = new List<Target>(); // List of targets and their offsets

    private List<Vector3> targetGoalPositions = new List<Vector3>(); // List to store current goal positions for each target
    private List<Vector3> targetVelocities = new List<Vector3>(); // List to store current velocities for each target

    void Start()
    {
        // Initialize the targetGoalPositions and targetVelocities
        foreach (var target in targets)
        {
            targetGoalPositions.Add(transform.position + target.offset);
            targetVelocities.Add(Vector3.zero); // Initialize velocities to zero
        }
    }

    void Update()
    {
        // Get the mouse position in world coordinates
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Ensure z is zero to prevent unintended movement on the z-axis

        // Calculate the direction from the object to the mouse position
        Vector3 directionToMouse = (mousePosition - transform.position).normalized;

        // Calculate the new position of the object
        Vector3 newPosition = transform.position + directionToMouse * speed * Time.deltaTime;

        // Move the object to the new position
        transform.position = newPosition;

        // Update each target's position based on its offset and threshold distance
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            var targetGoalPosition = targetGoalPositions[i];

            // Calculate the current offset position from the main GameObject
            Vector3 currentOffsetPosition = transform.position + target.offset;

            // Calculate the distance from the targetObject to the current offset position
            float distanceToOffset = Vector3.Distance(target.targetObject.transform.position, currentOffsetPosition);

            // Update the targetGoalPosition if the distance exceeds the threshold
            if (distanceToOffset > thresholdDistance)
            {
                targetGoalPosition = currentOffsetPosition;
                targetGoalPositions[i] = targetGoalPosition;
            }

            // Calculate the direction from the targetObject to the targetGoalPosition
            Vector3 directionToGoal = (targetGoalPosition - target.targetObject.transform.position).normalized;

            // Calculate the new velocity of the targetObject with acceleration
            targetVelocities[i] = Vector3.Lerp(targetVelocities[i], directionToGoal * speed, Time.deltaTime * 3f); // 3f is the acceleration factor

            // Calculate the new position of the targetObject
            Vector3 newTargetPosition = target.targetObject.transform.position + targetVelocities[i] * Time.deltaTime;

            // Move the targetObject to the new position
            target.targetObject.transform.position = newTargetPosition;
        }
    }
}
