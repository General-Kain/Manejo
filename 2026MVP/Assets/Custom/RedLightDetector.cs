using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Detects when the player runs a red light.
/// Finds traffic light GameObjects directly and checks their visual state.
/// Just attach this to the Player vehicle.
/// </summary>
public class RedLightDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Distance to intersection center to consider 'at intersection'")]
    public float intersectionDetectionRadius = 20f;

    [Tooltip("Minimum speed (km/h) to trigger red light detection")]
    public float minSpeedToDetect = 5f;

    [Tooltip("Cooldown between violations (seconds)")]
    public float violationCooldown = 10f;

    [Tooltip("How often to check (seconds)")]
    public float checkInterval = 0.2f;

    [Header("Penalties")]
    public int redLightPenalty = 20;

    [Header("References")]
    public ViolationDetector violationDetector;

    [Header("Debug")]
    public bool showDebug = true;

    // State tracking
    private TrafficLightInfo currentLight = null;
    private bool wasRedOnEntry = false;
    private Vector3 entryPosition;
    private float lastViolationTime = -999f;
    private float lastCheckTime = 0f;
    private Rigidbody vehicleRb;

    // Cached traffic light data
    private List<TrafficLightInfo> trafficLights = new List<TrafficLightInfo>();
    private bool lightsCached = false;

    private class TrafficLightInfo
    {
        public string name;
        public GameObject gameObject;
        public Transform redLight;
        public Transform yellowLight;
        public Transform greenLight;
        public Vector3 position;
    }

    // RCCP integration
    private Component rccpController;
    private System.Type rccpType;
    private bool useRCCP = false;

    void Start()
    {
        vehicleRb = GetComponent<Rigidbody>();
        TryFindRCCP();

        if (violationDetector == null)
        {
            violationDetector = GetComponent<ViolationDetector>();
        }
    }

    void TryFindRCCP()
    {
        foreach (var component in GetComponents<Component>())
        {
            if (component.GetType().Name.Contains("RCCP_CarController"))
            {
                rccpController = component;
                rccpType = component.GetType();
                useRCCP = true;
                break;
            }
        }
    }

    void Update()
    {
        // Cache traffic lights on first run (don't require API.IsInitialized)
        if (!lightsCached)
        {
            CacheTrafficLights();
        }

        if (Time.time - lastCheckTime > checkInterval)
        {
            lastCheckTime = Time.time;
            CheckTrafficLights();
        }
    }

    void CacheTrafficLights()
    {
        trafficLights.Clear();

        if (showDebug) Debug.Log("[RedLight] Searching for TrafficLightPost objects...");

        // Find all TrafficLightPost objects by name
        var allTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);

        foreach (var t in allTransforms)
        {
            if (t.name == "TrafficLightPost")
            {
                var info = new TrafficLightInfo
                {
                    name = t.name,
                    gameObject = t.gameObject,
                    position = t.position
                };

                // Find Lights child, then find RedLightOn/GreenLightOn
                Transform lights = t.Find("Lights");
                if (lights != null)
                {
                    info.redLight = lights.Find("RedLightOn");
                    info.greenLight = lights.Find("GreenLightOn");
                    info.yellowLight = lights.Find("YellowLightOn");
                }

                if (info.redLight != null || info.greenLight != null)
                {
                    trafficLights.Add(info);

                    if (showDebug)
                    {
                        Debug.Log($"[RedLight] Found: '{info.name}' at {info.position} (red={info.redLight != null}, green={info.greenLight != null})");
                    }
                }
            }
        }

        if (showDebug)
        {
            Debug.Log($"[RedLight] Cached {trafficLights.Count} traffic lights");
        }

        lightsCached = true;
    }

    bool IsRedLightActive(TrafficLightInfo light)
    {
        if (light == null) return false;

        // Gley toggles SetActive() on RedLightOn/GreenLightOn GameObjects
        bool redOn = light.redLight != null && light.redLight.gameObject.activeInHierarchy;
        bool greenOn = light.greenLight != null && light.greenLight.gameObject.activeInHierarchy;

        if (showDebug && Time.frameCount % 120 == 0)
        {
            Debug.Log($"[RedLight] '{light.name}': Red={redOn}, Green={greenOn}");
        }

        return redOn && !greenOn;
    }

    void CheckTrafficLights()
    {
        float speed = GetSpeed();
        Vector3 playerPos = transform.position;

        // Find closest traffic light
        TrafficLightInfo closestLight = null;
        float closestDistance = float.MaxValue;

        foreach (var light in trafficLights)
        {
            float dist = Vector3.Distance(playerPos, light.position);
            if (dist < closestDistance && dist < intersectionDetectionRadius)
            {
                closestDistance = dist;
                closestLight = light;
            }
        }

        // Check if closest light is red
        bool isAtRedLight = false;

        if (closestLight != null)
        {
            isAtRedLight = IsRedLightActive(closestLight);

            if (showDebug)
            {
                Color debugColor = isAtRedLight ? Color.red : Color.green;
                Debug.DrawLine(playerPos + Vector3.up * 2, closestLight.position, debugColor, checkInterval);
            }
        }

        // Log state periodically
        if (showDebug && Time.frameCount % 60 == 0)
        {
            string lightName = closestLight?.name ?? "none";
            Debug.Log($"[RedLight] Closest: '{lightName}' ({closestDistance:F1}m), Red={isAtRedLight}, Speed={speed:F0}km/h");
        }

        // State machine for violation detection
        if (currentLight == null)
        {
            // Not tracking any light - check if entering one on red
            if (closestLight != null && isAtRedLight && speed > minSpeedToDetect)
            {
                currentLight = closestLight;
                wasRedOnEntry = true;
                entryPosition = playerPos;

                if (showDebug)
                {
                    Debug.Log($"[RedLight] Entering '{closestLight.name}' on RED at {speed:F0} km/h");
                }
            }
        }
        else
        {
            // We're tracking a light
            float distanceFromEntry = Vector3.Distance(playerPos, entryPosition);

            // Check if we've passed through (moved significant distance)
            if (distanceFromEntry > intersectionDetectionRadius * 0.5f)
            {
                if (wasRedOnEntry && speed > minSpeedToDetect)
                {
                    // Check cooldown
                    if (Time.time - lastViolationTime > violationCooldown)
                    {
                        TriggerRedLightViolation(speed, currentLight.name);
                    }
                }

                // Reset state
                currentLight = null;
                wasRedOnEntry = false;
            }
            else if (!isAtRedLight)
            {
                // Light turned green while we were waiting
                if (showDebug)
                {
                    Debug.Log("[RedLight] Light turned GREEN, resetting");
                }
                currentLight = null;
                wasRedOnEntry = false;
            }
        }
    }

    void TriggerRedLightViolation(float speed, string intersectionName)
    {
        lastViolationTime = Time.time;

        if (showDebug)
        {
            Debug.LogWarning($"[RedLight] RED LIGHT VIOLATION at '{intersectionName}'! Speed: {speed:F0} km/h");
        }

        if (violationDetector != null)
        {
            violationDetector.totalScore -= redLightPenalty;
        }

        if (NotificationManager.Instance != null)
        {
            NotificationManager.Instance.ShowNotification(
                $"-{redLightPenalty} ¡SEMÁFORO EN ROJO!",
                Color.red
            );
        }

        if (TelemetryLogger.Instance != null)
        {
            TelemetryLogger.Instance.LogEvent(
                "SEMAFORO_ROJO",
                $"Pasó el semáforo en rojo ({intersectionName})",
                -redLightPenalty,
                speed
            );
        }
    }

    float GetSpeed()
    {
        if (useRCCP && rccpController != null)
        {
            try
            {
                return (float)rccpType.GetProperty("speed").GetValue(rccpController);
            }
            catch { }
        }

        if (vehicleRb != null)
            return vehicleRb.linearVelocity.magnitude * 3.6f;

        return 0f;
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, intersectionDetectionRadius);

        if (!Application.isPlaying) return;

        // Draw cached traffic lights
        foreach (var light in trafficLights)
        {
            bool isRed = IsRedLightActive(light);
            Gizmos.color = isRed ? Color.red : Color.green;
            Gizmos.DrawWireSphere(light.position, 3f);
            Gizmos.DrawLine(transform.position, light.position);
        }
    }
}
