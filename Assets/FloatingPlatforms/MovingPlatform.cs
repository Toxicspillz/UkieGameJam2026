using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FloatingPlatform : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;

    [Header("Timing")]
    [Tooltip("Seconds to go from A -> B when Speed Factor = 1")]
    [SerializeField] private float secondsA_to_B = 2.0f;

    [Tooltip("Designer-controlled multiplier (can be changed at runtime). 0 = stop.")]
    [SerializeField] private float speedFactor = 1.0f;

    [Tooltip("How quickly the platform adapts to speedFactor changes (seconds).")]
    [SerializeField] private float speedFactorSmoothTime = 0.25f;

    [Header("Easing (non-constant speed)")]
    [Tooltip("Maps 0..1 to 0..1. EaseInOut gives smooth accel/decel.")]
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Rigidbody2D rb;

    // smoothed runtime factor
    private float _speedFactorCurrent;
    private float _speedFactorVel;

    // phase accumulator (in "A->B seconds" units)
    private float _phase;

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        _speedFactorCurrent = Mathf.Max(0f, speedFactor);
    }

    private void FixedUpdate()
    {
        if (!pointA || !pointB || secondsA_to_B <= 0f) return;

        // Smoothly change the factor (so designers can tweak it live without a jerk). [web:88]
        float target = Mathf.Max(0f, speedFactor);
        _speedFactorCurrent = Mathf.SmoothDamp(
            _speedFactorCurrent, target, ref _speedFactorVel, speedFactorSmoothTime, Mathf.Infinity, Time.fixedDeltaTime
        ); // SmoothDamp uses a spring-damper style smoothing. [web:88]

        // Advance phase; speedFactor=1 means A->B takes secondsAtoB.
        _phase += Time.fixedDeltaTime * (_speedFactorCurrent / secondsA_to_B);

        // Loop A<->B using PingPong in [0..1]. [page:0]
        float ping = Mathf.PingPong(_phase, 1f); // 0..1..0..1...
        float t = ease.Evaluate(ping);           // ease to make speed non-constant (floaty)

        Vector2 pos = Vector2.Lerp(pointA.position, pointB.position, t);
        rb.MovePosition(pos); // Call from FixedUpdate for physics movement. [page:1]
    }

    // Optional: let gameplay code change it.
    public void SetSpeedFactor(float newFactor) => speedFactor = newFactor;
}
