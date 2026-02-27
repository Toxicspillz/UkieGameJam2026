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
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public Vector2 PlatformVelocity { get; private set; }

    private Rigidbody2D rb;

    private float _speedFactorCurrent;
    private float _speedFactorVel;
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

    private void OnDisable()
    {
        PlatformVelocity = Vector2.zero;
    }

    private void FixedUpdate()
    {
        if (!pointA || !pointB || secondsA_to_B <= 0f) return;

        float target = Mathf.Max(0f, speedFactor);
        _speedFactorCurrent = Mathf.SmoothDamp(
            _speedFactorCurrent, target, ref _speedFactorVel, speedFactorSmoothTime, Mathf.Infinity, Time.fixedDeltaTime
        ); // SmoothDamp smoothing [web:88]

        _phase += Time.fixedDeltaTime * (_speedFactorCurrent / secondsA_to_B);

        float ping = Mathf.PingPong(_phase, 1f);
        float t = ease.Evaluate(ping);

        Vector2 current = rb.position;
        Vector2 next = Vector2.Lerp(pointA.position, pointB.position, t);

        PlatformVelocity = (next - current) / Time.fixedDeltaTime;

        rb.MovePosition(next); // Drive in FixedUpdate for physics movement. [web:73]
    }

    public void SetSpeedFactor(float newFactor) => speedFactor = newFactor;
}
