using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TentacleSegment2
{
    public float size;
    public GameObject gameObject;
    public CircleCollider2D collider;
    public SpriteRenderer renderer;
    public Vector3 direction;

    public TentacleSegment2(float size, GameObject gameObject, CircleCollider2D collider, SpriteRenderer renderer)
    {
        this.size = size;
        this.gameObject = gameObject;
        this.collider = collider;
        this.renderer = renderer;
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
    public LineRenderer lineRenderer;  // Added LineRenderer for each tentacle

    public Tentacles2(List<TentacleSegment2> segments, GameObject endTarget, float setDistance, float moveSpeed, float pullStrength, float wiggleFrequency, float wiggleAmplitude)
    {
        this.segments = segments;
        this.endTarget = endTarget;
        this.setDistance = setDistance;
        this.moveSpeed = moveSpeed;
        this.pullStrength = pullStrength;
        this.wiggleFrequency = wiggleFrequency;
        this.wiggleAmplitude = wiggleAmplitude;
        this.lineRenderer = null;  // Initialize LineRenderer as null
    }
}

public class Procedural2 : MonoBehaviour
{
    public GameObject prefab;
    public Material gooMaterial;
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
            InitializeTentacles(tentacles, tentacleContainer);
        }
    }

    void Update()
    {
        foreach (var tentacles in tentaclesList)
        {
            UpdateTentaclePositions(tentacles);
        }
    }

    void InitializeTentacles(Tentacles2 tentacles, GameObject tentacleContainer)
    {
        List<TentacleSegment2> tentacleSegments = new List<TentacleSegment2>();

        foreach (var segment in tentacles.segments)
        {
            GameObject newGameObject = Instantiate(prefab, transform.position, Quaternion.identity);
            newGameObject.transform.SetParent(tentacleContainer.transform);

            CircleCollider2D collider = newGameObject.GetComponent<CircleCollider2D>();
            SpriteRenderer renderer = newGameObject.GetComponent<SpriteRenderer>();

            if (collider != null)
            {
                collider.radius = segment.size;
            }

            if (renderer != null)
            {
                float diameter = segment.size * 2.0f;
                newGameObject.transform.localScale = new Vector3(diameter, diameter, 1);
            }

            tentacleSegments.Add(new TentacleSegment2(segment.size, newGameObject, collider, renderer));
        }

        tentacles.segments = tentacleSegments;

        // Add a LineRenderer to the first segment of the tentacle
        if (tentacles.segments.Count > 0)
        {
            TentacleSegment2 firstSegment = tentacles.segments[0];
            tentacles.lineRenderer = firstSegment.gameObject.AddComponent<LineRenderer>();
            tentacles.lineRenderer.material = gooMaterial;
            tentacles.lineRenderer.startColor = Color.white;
            tentacles.lineRenderer.endColor = Color.white;

            // Set up width curve based on segment sizes
            AnimationCurve widthCurve = new AnimationCurve();
            for (int i = 0; i < tentacles.segments.Count; i++)
            {
                float t = (float)i / (tentacles.segments.Count - 1); // Normalized position along the line
                float width = tentacles.segments[i].size * 2.0f; // Diameter
                widthCurve.AddKey(t, width);
            }
            tentacles.lineRenderer.widthCurve = widthCurve;

            // Set the width multiplier to smooth out the edges
            tentacles.lineRenderer.widthMultiplier = 1.0f;

            // Ensure corner and end caps are rounded by default (using a proper shader)
            tentacles.lineRenderer.numCapVertices = 10;  // Add vertices to round the caps
        }


    }

    void UpdateTentaclePositions(Tentacles2 tentacles)
    {
        Vector3 targetPosition = transform.position;
        Vector3 endPosition = tentacles.endTarget != null ? tentacles.endTarget.transform.position : targetPosition;

        ApplyForcesToAllSegments(tentacles.segments, targetPosition, endPosition, tentacles);

        float distortion = Mathf.PingPong(Time.time * 0.1f, 0.05f) + 0.05f;

        List<Vector3> splinePoints = GenerateCatmullRomSpline(tentacles.segments);
        UpdateLineRenderer(tentacles.lineRenderer, splinePoints);
    }

    void ApplyForcesToAllSegments(List<TentacleSegment2> segments, Vector3 targetPosition, Vector3 endPosition, Tentacles2 tentacles)
    {
        float time = Time.time;
        for (int i = 0; i < segments.Count; i++)
        {
            TentacleSegment2 currentSegment = segments[i];
            Vector3 desiredPosition = GetDesiredPosition(i, segments, targetPosition, endPosition, tentacles, time);
            MoveTowards(currentSegment.gameObject, desiredPosition, tentacles.moveSpeed);
            currentSegment.direction = (currentSegment.gameObject.transform.position - desiredPosition).normalized;
            ApplyPullForces(i, segments, tentacles);
        }
    }

    Vector3 GetDesiredPosition(int index, List<TentacleSegment2> segments, Vector3 targetPosition, Vector3 endPosition, Tentacles2 tentacles, float time)
    {
        if (index == 0) return targetPosition;

        TentacleSegment2 previousSegment = segments[index - 1];
        TentacleSegment2 nextSegment = index < segments.Count - 1 ? segments[index + 1] : null;
        Vector3 directionPrev = segments[index].gameObject.transform.position - previousSegment.gameObject.transform.position;

        Vector3 desiredPosition = previousSegment.gameObject.transform.position + directionPrev.normalized * tentacles.setDistance;
        if (index > 0 && tentacles.endTarget != null)
        {
            desiredPosition = nextSegment != null ?
                (previousSegment.gameObject.transform.position + nextSegment.gameObject.transform.position) / 2.0f :
                previousSegment.gameObject.transform.position;
        }

        float offset = Mathf.Sin(time * tentacles.wiggleFrequency + index * 0.5f) * tentacles.wiggleAmplitude;
        Vector3 perpendicular = Vector3.Cross(directionPrev, Vector3.forward).normalized;
        return desiredPosition + perpendicular * offset;
    }

    void ApplyPullForces(int index, List<TentacleSegment2> segments, Tentacles2 tentacles)
    {
        if (index < segments.Count - 1)
        {
            TentacleSegment2 nextSegment = segments[index + 1];
            Vector3 pullDirection = segments[index].gameObject.transform.position - nextSegment.gameObject.transform.position;
            float distance = pullDirection.magnitude;
            if (distance > tentacles.setDistance)
            {
                Vector3 pullForce = pullDirection.normalized * (distance - tentacles.setDistance) * tentacles.pullStrength;
                segments[index].gameObject.transform.position -= pullForce * Time.deltaTime;
            }
        }

        if (index > 0 && tentacles.endTarget != null)
        {
            TentacleSegment2 previousSegment = segments[index - 1];
            Vector3 pullDirection = previousSegment.gameObject.transform.position - segments[index].gameObject.transform.position;
            float distance = pullDirection.magnitude;
            if (distance > tentacles.setDistance)
            {
                Vector3 pullForce = pullDirection.normalized * (distance - tentacles.setDistance) * tentacles.pullStrength;
                segments[index].gameObject.transform.position += pullForce * Time.deltaTime;
            }
        }
    }

    void MoveTowards(GameObject gameObject, Vector3 targetPosition, float moveSpeed)
    {
        gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetPosition, moveSpeed * Time.deltaTime);
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

    void UpdateLineRenderer(LineRenderer lineRenderer, List<Vector3> splinePoints)
    {
        if (lineRenderer == null) return;

        lineRenderer.positionCount = splinePoints.Count;
        lineRenderer.SetPositions(splinePoints.ToArray());
    }
}
