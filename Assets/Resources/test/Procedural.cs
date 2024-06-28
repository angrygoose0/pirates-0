using System.Collections.Generic;
using UnityEngine;

public class Procedural : MonoBehaviour
{
    public GameObject prefab;
    public List<float> sizes = new List<float>();
    public float setDistance = 1.0f;
    public float moveSpeed = 5.0f;
    public float pullStrength = 0.5f;
    public float wiggleFrequency = 2.0f;
    public float wiggleAmplitude = 0.5f;

    private Vector3 targetPosition;
    private Vector3 endPosition;
    private List<GameObject> gameObjectList = new List<GameObject>();
    private List<CircleCollider2D> colliders = new List<CircleCollider2D>();
    private List<SpriteRenderer> renderers = new List<SpriteRenderer>();
    public FollowMouse followMouse;

    void Start()
    {
        endPosition = followMouse.targetObject.transform.position;

        targetPosition = transform.position;

        GameObject tentacleContainer = GameObject.Find("tentacleContainer");
        if (tentacleContainer == null)
        {
            Debug.LogError("tentacleContainer not found!");
            return;
        }

        for (int i = 0; i < sizes.Count; i++)
        {
            GameObject newGameObject = Instantiate(prefab, targetPosition, Quaternion.identity);
            newGameObject.transform.SetParent(tentacleContainer.transform);

            CircleCollider2D collider = newGameObject.GetComponent<CircleCollider2D>();
            if (collider != null)
            {
                collider.radius = sizes[i];
                colliders.Add(collider);
            }

            SpriteRenderer renderer = newGameObject.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                float diameter = sizes[i] * 2.0f;
                newGameObject.transform.localScale = new Vector3(diameter, diameter, 1);
                renderers.Add(renderer);
            }

            gameObjectList.Add(newGameObject);
        }
    }

    void Update()
    {
        // Update targetPosition if needed (e.g., based on user input or AI logic)
        targetPosition = transform.position;
        endPosition = followMouse.targetObject.transform.position;
    }

    void FixedUpdate()
    {
        if (gameObjectList.Count > 0 && gameObjectList[0] != null)
        {
            ApplyForcesToAllObjects(targetPosition);
        }
    }

    void ApplyForcesToAllObjects(Vector3 targetPosition)
    {
        float time = Time.time;

        for (int i = 0; i < gameObjectList.Count; i++)
        {
            GameObject currentGameObject = gameObjectList[i];
            Vector3 desiredPosition;

            if (i == 0)
            {
                desiredPosition = targetPosition;
            }
            else if (i == gameObjectList.Count - 1)
            {
                desiredPosition = endPosition;
            }
            else
            {
                GameObject previousGameObject = gameObjectList[i - 1];
                GameObject nextGameObject = gameObjectList[i + 1];
                Vector3 directionPrev = currentGameObject.transform.position - previousGameObject.transform.position;
                Vector3 directionNext = nextGameObject.transform.position - currentGameObject.transform.position;
                desiredPosition = (previousGameObject.transform.position + nextGameObject.transform.position) / 2.0f;

                // Add wiggling motion
                float offset = Mathf.Sin(time * wiggleFrequency + i * 0.5f) * wiggleAmplitude;
                Vector3 perpendicular = Vector3.Cross(directionPrev, Vector3.forward).normalized;
                desiredPosition += perpendicular * offset;
            }

            MoveTowards(currentGameObject, desiredPosition);

            if (i < gameObjectList.Count - 1)
            {
                GameObject nextGameObject = gameObjectList[i + 1];
                Vector3 pullDirectionNext = currentGameObject.transform.position - nextGameObject.transform.position;
                float distanceNext = pullDirectionNext.magnitude;

                if (distanceNext > setDistance)
                {
                    Vector3 pullForceNext = pullDirectionNext.normalized * (distanceNext - setDistance) * pullStrength;
                    currentGameObject.transform.position -= pullForceNext * Time.deltaTime;
                }
            }

            if (i > 0)
            {
                GameObject previousGameObject = gameObjectList[i - 1];
                Vector3 pullDirectionPrev = previousGameObject.transform.position - currentGameObject.transform.position;
                float distancePrev = pullDirectionPrev.magnitude;

                if (distancePrev > setDistance)
                {
                    Vector3 pullForcePrev = pullDirectionPrev.normalized * (distancePrev - setDistance) * pullStrength;
                    currentGameObject.transform.position += pullForcePrev * Time.deltaTime;
                }
            }
        }
    }

    void MoveTowards(GameObject gameObject, Vector3 targetPosition)
    {
        Vector3 currentPosition = gameObject.transform.position;
        Vector3 direction = targetPosition - currentPosition;

        // Adjust direction to slow down movement in the y-axis
        direction.y *= 0.5f;

        Vector3 adjustedTargetPosition = currentPosition + direction;
        gameObject.transform.position = Vector3.Lerp(currentPosition, adjustedTargetPosition, moveSpeed * Time.deltaTime);
    }
}
