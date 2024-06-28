using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    public float speed = 5f; // Speed at which the object moves towards the mouse
    public GameObject targetObject; // The GameObject that will move towards the offset position
    public Vector3 offset = new Vector3(1f, 1f, 0f); // Offset from the main GameObject
    public float thresholdDistance = 4f; // Distance threshold for updating the targetObject's position

    private Vector3 targetGoalPosition; // The current goal position for the targetObject

    void Start()
    {
        // Initialize the targetGoalPosition to the initial offset position
        targetGoalPosition = transform.position + offset;
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

        // Calculate the current offset position from the main GameObject
        Vector3 currentOffsetPosition = transform.position + offset;

        // Calculate the distance from the targetObject to the current offset position
        float distanceToOffset = Vector3.Distance(targetObject.transform.position, currentOffsetPosition);

        // Update the targetGoalPosition if the distance exceeds the threshold
        if (distanceToOffset > thresholdDistance)
        {
            targetGoalPosition = currentOffsetPosition;
        }

        // Calculate the direction from the targetObject to the targetGoalPosition
        Vector3 directionToGoal = (targetGoalPosition - targetObject.transform.position).normalized;

        // Calculate the new position of the targetObject
        Vector3 newTargetPosition = targetObject.transform.position + directionToGoal * speed * Time.deltaTime;

        // Move the targetObject to the new position
        targetObject.transform.position = newTargetPosition;
    }
}
