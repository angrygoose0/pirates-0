using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TentacleSegment2
{
    public float size;
    public GameObject gameObject;
    public CircleCollider2D collider;
    public SpriteRenderer renderer;
    public LineRenderer lineRenderer;
    public Vector3 direction;

    public TentacleSegment2(float size, GameObject gameObject, CircleCollider2D collider, SpriteRenderer renderer, LineRenderer lineRenderer)
    {
        this.size = size;
        this.gameObject = gameObject;
        this.collider = collider;
        this.renderer = renderer;
        this.lineRenderer = lineRenderer;
        this.direction = Vector3.zero;
    }
}

[System.Serializable]
public class Tentacles2
{
    public List<TentacleSegment2> segments;
    public GameObject endTarget;
    public float setDistance = 1.0f;
    public float moveSpeed = 5.0f;
    public float pullStrength = 0.5f;
    public float wiggleFrequency = 2.0f;
    public float wiggleAmplitude = 0.5f;
    public Material organicMaterial;

    public Tentacles2(List<TentacleSegment2> segments, GameObject endTarget, float setDistance, float moveSpeed, float pullStrength, float wiggleFrequency, float wiggleAmplitude, Material organicMaterial)
    {
        this.segments = segments;
        this.endTarget = endTarget;
        this.setDistance = setDistance;
        this.moveSpeed = moveSpeed;
        this.pullStrength = pullStrength;
        this.wiggleFrequency = wiggleFrequency;
        this.wiggleAmplitude = wiggleAmplitude;
        this.organicMaterial = organicMaterial;
    }
}

public class Procedural2 : MonoBehaviour
{
    public GameObject prefab;
    public Material gooMaterial; // Add this line
    public List<Tentacles2> tentaclesList = new List<Tentacles2>();

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
            List<TentacleSegment2> tentacleSegments = new List<TentacleSegment2>();
            Vector3 targetPosition = transform.position;

            foreach (var size in tentacles.segments)
            {
                GameObject newGameObject = Instantiate(prefab, targetPosition, Quaternion.identity);
                newGameObject.transform.SetParent(tentacleContainer.transform);

                CircleCollider2D collider = newGameObject.GetComponent<CircleCollider2D>();
                SpriteRenderer renderer = newGameObject.GetComponent<SpriteRenderer>();
                LineRenderer lineRenderer = newGameObject.AddComponent<LineRenderer>();

                if (collider != null)
                {
                    collider.radius = size.size;
                }

                if (renderer != null)
                {
                    float diameter = size.size * 2.0f;
                    newGameObject.transform.localScale = new Vector3(diameter, diameter, 1);
                }

                // Configure LineRenderer
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;
                lineRenderer.material = gooMaterial; // Use the custom material
                lineRenderer.startColor = Color.black;
                lineRenderer.endColor = Color.black;

                TentacleSegment2 segment = new TentacleSegment2(size.size, newGameObject, collider, renderer, lineRenderer);
                tentacleSegments.Add(segment);
            }

            tentacles.segments = tentacleSegments;
        }
    }

    void Update()
    {
        foreach (var tentacles in tentaclesList)
        {
            Vector3 targetPosition = transform.position;
            Vector3 endPosition = tentacles.endTarget != null ? tentacles.endTarget.transform.position : targetPosition;

            ApplyForcesToAllSegments(tentacles.segments, targetPosition, endPosition, tentacles);

            // Animate shader properties
            float distortion = Mathf.PingPong(Time.time * 0.1f, 0.05f) + 0.05f;
            foreach (var segment in tentacles.segments)
            {
                segment.lineRenderer.material.SetFloat("_Distortion", distortion);
            }
        }
    }


    void FixedUpdate()
    {
        foreach (var tentacles in tentaclesList)
        {
            if (tentacles.segments.Count > 0 && tentacles.segments[0].gameObject != null)
            {
                Vector3 targetPosition = transform.position;
                Vector3 endPosition = tentacles.endTarget != null ? tentacles.endTarget.transform.position : targetPosition;
                ApplyForcesToAllSegments(tentacles.segments, targetPosition, endPosition, tentacles);
            }
        }
    }

    void ApplyForcesToAllSegments(List<TentacleSegment2> tentacleSegments, Vector3 targetPosition, Vector3 endPosition, Tentacles2 tentacles)
    {
        float time = Time.time;

        for (int i = 0; i < tentacleSegments.Count; i++)
        {
            TentacleSegment2 currentSegment = tentacleSegments[i];
            Vector3 desiredPosition;

            if (i == 0)
            {
                desiredPosition = targetPosition;
            }
            else if (i == tentacleSegments.Count - 3 && tentacles.endTarget != null)
            {
                desiredPosition = endPosition;
            }
            else
            {
                TentacleSegment2 previousSegment = tentacleSegments[i - 1];
                TentacleSegment2 nextSegment = i < tentacleSegments.Count - 1 ? tentacleSegments[i + 1] : null;
                Vector3 directionPrev = currentSegment.gameObject.transform.position - previousSegment.gameObject.transform.position;
                Vector3 directionNext = nextSegment != null ? nextSegment.gameObject.transform.position - currentSegment.gameObject.transform.position : Vector3.zero;

                if (tentacles.endTarget != null)
                {
                    desiredPosition = nextSegment != null ? (previousSegment.gameObject.transform.position + nextSegment.gameObject.transform.position) / 2.0f : previousSegment.gameObject.transform.position;
                }
                else
                {
                    desiredPosition = previousSegment.gameObject.transform.position + directionPrev.normalized * tentacles.setDistance;
                }

                float offset = Mathf.Sin(time * tentacles.wiggleFrequency + i * 0.5f) * tentacles.wiggleAmplitude;
                Vector3 perpendicular = Vector3.Cross(directionPrev, Vector3.forward).normalized;
                desiredPosition += perpendicular * offset;
            }

            Vector3 previousPosition = currentSegment.gameObject.transform.position;
            MoveTowards(currentSegment.gameObject, desiredPosition, tentacles.moveSpeed);
            currentSegment.direction = (currentSegment.gameObject.transform.position - previousPosition).normalized;

            if (i < tentacleSegments.Count - 1)
            {
                TentacleSegment2 nextSegment = tentacleSegments[i + 1];
                Vector3 pullDirectionNext = currentSegment.gameObject.transform.position - nextSegment.gameObject.transform.position;
                float distanceNext = pullDirectionNext.magnitude;

                if (distanceNext > tentacles.setDistance)
                {
                    Vector3 pullForceNext = pullDirectionNext.normalized * (distanceNext - tentacles.setDistance) * tentacles.pullStrength;
                    currentSegment.gameObject.transform.position -= pullForceNext * Time.deltaTime;
                }
            }

            if (i > 0 && tentacles.endTarget != null)
            {
                TentacleSegment2 previousSegment = tentacleSegments[i - 1];
                Vector3 pullDirectionPrev = previousSegment.gameObject.transform.position - currentSegment.gameObject.transform.position;
                float distancePrev = pullDirectionPrev.magnitude;

                if (distancePrev > tentacles.setDistance)
                {
                    Vector3 pullForcePrev = pullDirectionPrev.normalized * (distancePrev - tentacles.setDistance) * tentacles.pullStrength;
                    currentSegment.gameObject.transform.position += pullForcePrev * Time.deltaTime;
                }
            }
        }

        // Generate Catmull-Rom spline points
        List<Vector3> splinePoints = GenerateCatmullRomSpline(tentacleSegments);

        // Update LineRenderer with spline points
        for (int i = 0; i < tentacleSegments.Count; i++)
        {
            tentacleSegments[i].lineRenderer.positionCount = splinePoints.Count;
            tentacleSegments[i].lineRenderer.SetPositions(splinePoints.ToArray());
        }
    }

    void MoveTowards(GameObject gameObject, Vector3 targetPosition, float moveSpeed)
    {
        Vector3 currentPosition = gameObject.transform.position;
        Vector3 direction = targetPosition - currentPosition;
        direction.y *= 0.5f;
        Vector3 adjustedTargetPosition = currentPosition + direction;
        gameObject.transform.position = Vector3.Lerp(currentPosition, adjustedTargetPosition, moveSpeed * Time.deltaTime);
    }

    List<Vector3> GenerateCatmullRomSpline(List<TentacleSegment2> segments, int resolution = 10)
    {
        List<Vector3> splinePoints = new List<Vector3>();
        for (int i = 0; i < segments.Count - 1; i++)
        {
            Vector3 p0 = segments[Mathf.Max(i - 1, 0)].gameObject.transform.position;
            Vector3 p1 = segments[i].gameObject.transform.position;
            Vector3 p2 = segments[i + 1].gameObject.transform.position;
            Vector3 p3 = segments[Mathf.Min(i + 2, segments.Count - 1)].gameObject.transform.position;

            for (int j = 0; j < resolution; j++)
            {
                float t = j / (float)resolution;
                Vector3 position = GetCatmullRomPosition(t, p0, p1, p2, p3);
                splinePoints.Add(position);
            }
        }
        return splinePoints;
    }

    Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        float a = -0.5f * t3 + t2 - 0.5f * t;
        float b = 1.5f * t3 - 2.5f * t2 + 1.0f;
        float c = -1.5f * t3 + 2.0f * t2 + 0.5f * t;
        float d = 0.5f * t3 - 0.5f * t2;

        return a * p0 + b * p1 + c * p2 + d * p3;
    }
}
