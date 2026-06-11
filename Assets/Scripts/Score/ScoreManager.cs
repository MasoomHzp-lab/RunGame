using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int CurrentScore { get; private set; }
    public int TargetScore { get; private set; }
    public bool TargetReached { get; private set; }

    public event Action<int> OnScoreChanged;
    public event Action OnTargetReached;

    public void SetTargetScore(int target)
    {
        TargetScore = target;
        TargetReached = false;
    }

    public void AddScore(int amount)
    {
        CurrentScore += amount;
        OnScoreChanged?.Invoke(CurrentScore);

        if (TargetScore > 0 && !TargetReached && CurrentScore >= TargetScore)
        {
            TargetReached = true;
            OnTargetReached?.Invoke();
        }
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        TargetScore = 0;
        TargetReached = false;
        OnScoreChanged?.Invoke(CurrentScore);
    }
}