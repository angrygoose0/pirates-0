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

    private List<GameObject> gameObjectList = new List<GameObject>();
    private List<CircleCollider2D> colliders = new List<CircleCollider2D>();
    private List<SpriteRenderer> renderers = new List<SpriteRenderer>();

    void Start()
    {
        for (int i = 0; i < sizes.Count; i++)
        {
            GameObject newGameObject = Instantiate(prefab, Vector3.zero, Quaternion.identity);

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

    void FixedUpdate()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Camera.main.nearClipPlane;
        Vector3 worldMousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        if (gameObjectList.Count > 0 && gameObjectList[0] != null)
        {
            ApplyForcesToAllObjects(worldMousePosition);
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
            else
            {
                GameObject previousGameObject = gameObjectList[i - 1];
                Vector3 direction = currentGameObject.transform.position - previousGameObject.transform.position;
                desiredPosition = previousGameObject.transform.position + direction.normalized * setDistance;

                // Add wiggling motion
                float offset = Mathf.Sin(time * wiggleFrequency + i * 0.5f) * wiggleAmplitude;
                Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;
                desiredPosition += perpendicular * offset;
            }

            MoveTowards(currentGameObject, desiredPosition);

            if (i < gameObjectList.Count - 1)
            {
                GameObject nextGameObject = gameObjectList[i + 1];
                Vector3 pullDirection = currentGameObject.transform.position - nextGameObject.transform.position;
                float distance = pullDirection.magnitude;

                if (distance > setDistance)
                {
                    Vector3 pullForce = pullDirection.normalized * (distance - setDistance) * pullStrength;
                    currentGameObject.transform.position -= pullForce * Time.deltaTime;
                }
            }
        }
    }

    void MoveTowards(GameObject gameObject, Vector3 targetPosition)
    {
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
}
