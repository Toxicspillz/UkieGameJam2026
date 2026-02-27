using System.Collections.Generic;
using UnityEngine;

public class SmoothCamFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);

    [Header("Delay (real lag)")]
    [Tooltip("Seconds behind the target. This is a real time delay, not just smoothing.")]
    [SerializeField] private float followDelay = 0.15f;

    [Header("Extra smoothing (optional)")]
    [Tooltip("0 = no smoothing (hard delayed). Higher = smoother.")]
    [SerializeField] private float smoothTime = 0.08f;

    [SerializeField] private float maxSpeed = 50f;

    struct Sample { public float t; public Vector3 p; }

    private readonly List<Sample> _samples = new();
    private Vector3 _vel;

    private void OnEnable()
    {
        _samples.Clear();
        _vel = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (!target) return;

        float now = Time.time;
        _samples.Add(new Sample { t = now, p = target.position });

        // Keep enough history (a bit more than delay)
        float oldestNeeded = now - (followDelay + 0.5f);
        while (_samples.Count > 2 && _samples[0].t < oldestNeeded)
            _samples.RemoveAt(0);

        float targetTime = now - Mathf.Max(0f, followDelay);

        // Find two samples surrounding targetTime
        int i = 0;
        while (i < _samples.Count - 2 && _samples[i + 1].t < targetTime) i++;

        Sample a = _samples[i];
        Sample b = _samples[Mathf.Min(i + 1, _samples.Count - 1)];

        float t = (Mathf.Abs(b.t - a.t) < 0.0001f) ? 1f : Mathf.InverseLerp(a.t, b.t, targetTime);
        Vector3 delayedPos = Vector3.Lerp(a.p, b.p, t);

        Vector3 desired = delayedPos + offset;

        // SmoothDamp is a spring-damper smoother; maxSpeed clamps catch-up speed. [web:162]
        if (smoothTime <= 0f)
        {
            transform.position = desired; // hard delayed
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position, desired, ref _vel, smoothTime, maxSpeed, Time.deltaTime
            ); // [web:162]
        }
    }

    public void SetTarget(Transform t)
    {
        target = t;
        _samples.Clear();
        _vel = Vector3.zero;
    }
}
