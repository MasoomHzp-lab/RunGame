using System;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    [Range(0f, 1f)]
    public float CurrentProgress { get; private set; }

    public event Action<float> OnProgressChanged;

    public void AddProgress(float amount)
    {
        CurrentProgress = Mathf.Clamp01(CurrentProgress + amount);
        OnProgressChanged?.Invoke(CurrentProgress);
    }

    public void ResetProgress()
    {
        CurrentProgress = 0f;
        OnProgressChanged?.Invoke(CurrentProgress);
    }
}