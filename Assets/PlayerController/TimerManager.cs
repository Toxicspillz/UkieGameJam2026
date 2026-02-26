using UnityEngine;
using System.Collections;

/// <summary>
/// Singleton timer manager that tracks level completion time and saves best time
/// Starts/stops timer based on level events
/// </summary>
public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    public float CurrentTime { get; private set; }
    public bool IsRunning { get; private set; }

    private Coroutine timerCoroutine;
    private const string BestTimeKey = "BestTime";

    public float BestTime
    {
        get => PlayerPrefs.GetFloat(BestTimeKey, float.MaxValue);
        private set
        {
            PlayerPrefs.SetFloat(BestTimeKey, value);
            PlayerPrefs.Save();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        //FinishLineTrigger.OnLevelCompleted += StopTimer;
    }

    private void OnDisable()
    {
        //FinishLineTrigger.OnLevelCompleted -= StopTimer;
    }

    public void StartTimer(bool forceRestart = false)
    {
        if (IsRunning)
        {
            return;
        }
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        CurrentTime = 0f;
        IsRunning = true;
        timerCoroutine = StartCoroutine(TimerLoop());
    }

    public void StopTimer()
    {
        if (!IsRunning)
        {
            return;
        }
        IsRunning = false;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }

        // Save best time if current time is faster
        if (CurrentTime < BestTime)
        {
            BestTime = CurrentTime;
        }

        //EndPanelController.Instance?.ShowEndPanel(CurrentTime, BestTime);
    }

    public void PauseMenuStopTimer()
    {
        IsRunning = false;
        CurrentTime = 0f;
    }

    private IEnumerator TimerLoop()
    {
        float last = Time.realtimeSinceStartup;

        while (IsRunning)
        {
            CurrentTime += Time.deltaTime;
            yield return null;
        }
    }

    public string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60);
        int seconds = Mathf.FloorToInt(t % 60);
        int millis = Mathf.FloorToInt((t * 1000) % 1000);
        return $"{minutes:00}:{seconds:00}:{millis:000}";
    }
}
