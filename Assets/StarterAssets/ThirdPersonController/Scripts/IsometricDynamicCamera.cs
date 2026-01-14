using UnityEngine;
using Cinemachine;

public class IsometricDynamicCamera : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineVirtualCamera cinemachineCamera;

    [Header("Targets")]
    [SerializeField] private Transform targetA;
    [SerializeField] private Transform targetB;

    [Header("Distance Limits")]
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 20f;

    [Header("Distance Scaling")]
    [SerializeField] private float distanceMultiplier = 1f;

    [Header("Cinematic Distance Offset")]
    public float cinematicDistanceOffset = 0f;

    [Header("Padding (screen space 0–1)")]
    [Tooltip("Extra horizontal/vertical padding applied to keep targets inside view")]
    [SerializeField] private Vector2 padding = new Vector2(0.1f, 0.1f);

    [Header("Screen Offset (0–1)")]
    [SerializeField] private Vector2 screenOffset = new Vector2(0.5f, 0.5f);

    private CinemachineFramingTransposer transposer;

    private void Awake()
    {
        transposer = cinemachineCamera.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (transposer == null)
        {
            Debug.LogError("Cinemachine Framing Transposer is required.");
            enabled = false;
            return;
        }

        ApplyFramingSettings();
    }

    private void LateUpdate()
    {
        if (targetA == null || targetB == null)
            return;

        UpdateMidpoint();
        UpdateCameraDistance();
    }

    private void UpdateMidpoint()
    {
        transform.position = (targetA.position + targetB.position) * 0.5f;
    }

    private void UpdateCameraDistance()
    {
        // Raw distance between targets in world space
        Vector3 delta = targetA.position - targetB.position;
        float horizontalDistance = Mathf.Abs(delta.x);
        float verticalDistance = Mathf.Abs(delta.z); // assuming Z is forward

        // Apply padding in screen space: approximate by scaling world distance
        float paddedHorizontal = horizontalDistance / (1f - padding.x);
        float paddedVertical = verticalDistance / (1f - padding.y);

        // Take the largest axis to ensure both targets fit
        float targetDistance = Mathf.Max(paddedHorizontal, paddedVertical);

        // Apply scaling & clamp
        float desiredDistance = targetDistance * distanceMultiplier;
        float clampedDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);

        transposer.m_CameraDistance = clampedDistance + cinematicDistanceOffset;
    }

    private void ApplyFramingSettings()
    {
        transposer.m_ScreenX = screenOffset.x;
        transposer.m_ScreenY = screenOffset.y;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (transposer != null)
            ApplyFramingSettings();
    }
#endif
}
