using UnityEngine;

[DefaultExecutionOrder(10000)]
public class CameraOutputStabilizer : MonoBehaviour
{
    [SerializeField] private bool enabledStabilizer = true;
    [SerializeField] private float jumpThreshold = 0.45f;
    [SerializeField] private float maxStepPerFrame = 0.22f;
    [SerializeField] private float recoveryLerpSpeed = 12f;
    [SerializeField] private bool debugLog;

    private bool hasLastFrame;
    private Vector3 lastPosition;

    public void Configure(
        bool enable,
        float threshold,
        float maxStep,
        float recoverySpeed,
        bool enableDebugLog
    )
    {
        enabledStabilizer = enable;
        jumpThreshold = Mathf.Max(0.01f, threshold);
        maxStepPerFrame = Mathf.Max(0.01f, maxStep);
        recoveryLerpSpeed = Mathf.Max(0.01f, recoverySpeed);
        debugLog = enableDebugLog;
    }

    private void LateUpdate()
    {
        if (!enabledStabilizer)
        {
            hasLastFrame = true;
            lastPosition = transform.position;
            return;
        }

        if (!hasLastFrame)
        {
            hasLastFrame = true;
            lastPosition = transform.position;
            return;
        }

        Vector3 currentPosition = transform.position;
        Vector3 delta = currentPosition - lastPosition;
        float distance = delta.magnitude;

        if (distance > jumpThreshold)
        {
            Vector3 clampedPosition = lastPosition + delta.normalized * Mathf.Min(maxStepPerFrame, distance);
            transform.position = Vector3.Lerp(
                lastPosition,
                clampedPosition,
                Mathf.Clamp01(recoveryLerpSpeed * Time.deltaTime)
            );

            if (debugLog)
            {
                Debug.LogWarning(
                    $"[CameraOutputStabilizer] spike={distance:F3} threshold={jumpThreshold:F3} " +
                    $"clampedStep={maxStepPerFrame:F3}"
                );
            }
        }

        lastPosition = transform.position;
    }
}
