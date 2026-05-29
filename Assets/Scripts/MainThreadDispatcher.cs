using System;
using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher instance;
    private static readonly Queue<Action> actionQueue = new Queue<Action>();
    private static readonly object queueLock = new object();

    public static MainThreadDispatcher Instance => instance;
    public static bool HasInstance => instance != null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static void EnqueueStatic(Action action)
    {
        if (action == null) return;

        lock (queueLock)
        {
            actionQueue.Enqueue(action);
        }
    }

    public void Enqueue(Action action)
    {
        EnqueueStatic(action);
    }

    private void Update()
    {
        while (true)
        {
            Action action = null;

            lock (queueLock)
            {
                if (actionQueue.Count > 0)
                    action = actionQueue.Dequeue();
            }

            if (action == null)
                break;

            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError("MainThreadDispatcher action failed: " + ex);
            }
        }
    }
}