using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tentacles
{
    public List<float> sizes;
    public GameObject endTarget;
    public float setDistance = 1.0f;
    public float moveSpeed = 5.0f;
    public float pullStrength = 0.5f;
    public float wiggleFrequency = 2.0f;
    public float wiggleAmplitude = 0.5f;

    public Tentacles(List<float> sizes, GameObject endTarget, float setDistance, float moveSpeed, float pullStrength, float wiggleFrequency, float wiggleAmplitude)
    {
        this.sizes = sizes;
        this.endTarget = endTarget;
        this.setDistance = setDistance;
        this.moveSpeed = moveSpeed;
        this.pullStrength = pullStrength;
        this.wiggleFrequency = wiggleFrequency;
        this.wiggleAmplitude = wiggleAmplitude;
    }
}

public class Procedural : MonoBehaviour
{
    public GameObject prefab;
    public List<Tentacles> tentaclesList = new List<Tentacles>();

    private List<List<GameObject>> gameObjectLists = new List<List<GameObject>>();
    private List<List<CircleCollider2D>> collidersLists = new List<List<CircleCollider2D>>();
    private List<List<SpriteRenderer>> renderersLists = new List<List<SpriteRenderer>>();
    public FollowMouse followMouse;

    void Start()
    {
        GameObject tentacleContainer = GameObject.Find("tentacleContainer");
        if (tentacleContainer == null)
        {
            Debug.LogError("tentacleContainer not found!");
            return;
        }

        foreach (var tentacles in tentaclesList)
        {
            List<GameObject> gameObjectList = new List<GameObject>();
            List<CircleCollider2D> colliders = new List<CircleCollider2D>();
            List<SpriteRenderer> renderers = new List<SpriteRenderer>();

            Vector3 targetPosition = transform.position;

            for (int i = 0; i < tentacles.sizes.Count; i++)
            {
                GameObject newGameObject = Instantiate(prefab, targetPosition, Quaternion.identity);
                newGameObject.transform.SetParent(tentacleContainer.transform);

                CircleCollider2D collider = newGameObject.GetComponent<CircleCollider2D>();
                if (collider != null)
                {
                    collider.radius = tentacles.sizes[i];
                    colliders.Add(collider);
                }

                SpriteRenderer renderer = newGameObject.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    float diameter = tentacles.sizes[i] * 2.0f;
                    newGameObject.transform.localScale = new Vector3(diameter, diameter, 1);
                    renderers.Add(renderer);
                }

                gameObjectList.Add(newGameObject);
            }

            gameObjectLists.Add(gameObjectList);
            collidersLists.Add(colliders);
            renderersLists.Add(renderers);
        }
    }

    void Update()
    {
        for (int i = 0; i < tentaclesList.Count; i++)
        {
            var tentacles = tentaclesList[i];
            Vector3 targetPosition = transform.position;
            Vector3 endPosition = tentacles.endTarget != null ? tentacles.endTarget.transform.position : targetPosition;

            ApplyForcesToAllObjects(gameObjectLists[i], targetPosition, endPosition, tentacles);
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < gameObjectLists.Count; i++)
        {
            var gameObjectList = gameObjectLists[i];
            if (gameObjectList.Count > 0 && gameObjectList[0] != null)
            {
                var tentacles = tentaclesList[i];
                Vector3 targetPosition = transform.position;
                Vector3 endPosition = tentacles.endTarget != null ? tentacles.endTarget.transform.position : targetPosition;
                ApplyForcesToAllObjects(gameObjectList, targetPosition, endPosition, tentacles);
            }
        }
    }

    void ApplyForcesToAllObjects(List<GameObject> gameObjectList, Vector3 targetPosition, Vector3 endPosition, Tentacles tentacles)
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
            else if (i == gameObjectList.Count - 1 && tentacles.endTarget != null)
            {
                desiredPosition = endPosition;
            }
            else
            {
                GameObject previousGameObject = gameObjectList[i - 1];
                GameObject nextGameObject = i < gameObjectList.Count - 1 ? gameObjectList[i + 1] : null;
                Vector3 directionPrev = currentGameObject.transform.position - previousGameObject.transform.position;
                Vector3 directionNext = nextGameObject != null ? nextGameObject.transform.position - currentGameObject.transform.position : Vector3.zero;

                if (tentacles.endTarget != null)
                {
                    desiredPosition = nextGameObject != null ? (previousGameObject.transform.position + nextGameObject.transform.position) / 2.0f : previousGameObject.transform.position;
                }
                else
                {
                    desiredPosition = previousGameObject.transform.position + directionPrev.normalized * tentacles.setDistance;
                }

                // Add wiggling motion
                float offset = Mathf.Sin(time * tentacles.wiggleFrequency + i * 0.5f) * tentacles.wiggleAmplitude;
                Vector3 perpendicular = Vector3.Cross(directionPrev, Vector3.forward).normalized;
                desiredPosition += perpendicular * offset;
            }

            MoveTowards(currentGameObject, desiredPosition, tentacles.moveSpeed);

            if (i < gameObjectList.Count - 1)
            {
                GameObject nextGameObject = gameObjectList[i + 1];
                Vector3 pullDirectionNext = currentGameObject.transform.position - nextGameObject.transform.position;
                float distanceNext = pullDirectionNext.magnitude;

                if (distanceNext > tentacles.setDistance)
                {
                    Vector3 pullForceNext = pullDirectionNext.normalized * (distanceNext - tentacles.setDistance) * tentacles.pullStrength;
                    currentGameObject.transform.position -= pullForceNext * Time.deltaTime;
                }
            }

            if (i > 0 && tentacles.endTarget != null)
            {
                GameObject previousGameObject = gameObjectList[i - 1];
                Vector3 pullDirectionPrev = previousGameObject.transform.position - currentGameObject.transform.position;
                float distancePrev = pullDirectionPrev.magnitude;

                if (distancePrev > tentacles.setDistance)
                {
                    Vector3 pullForcePrev = pullDirectionPrev.normalized * (distancePrev - tentacles.setDistance) * tentacles.pullStrength;
                    //currentGameObject.transform.position += pullForcePrev * Time.deltaTime;
                }
            }
        }
    }

    void MoveTowards(GameObject gameObject, Vector3 targetPosition, float moveSpeed)
    {
        Vector3 currentPosition = gameObject.transform.position;
        Vector3 direction = targetPosition - currentPosition;

        // Adjust direction to slow down movement in the y-axis
        direction.y *= 0.5f;

        Vector3 adjustedTargetPosition = currentPosition + direction;
        gameObject.transform.position = Vector3.Lerp(currentPosition, adjustedTargetPosition, moveSpeed * Time.deltaTime);
    }
}
