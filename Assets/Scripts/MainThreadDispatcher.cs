using UnityEngine;
using Firebase;
using Firebase.Database;
using System.Collections.Generic;
using System.Linq;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<System.Action> executionQueue = new Queue<System.Action>();

    public static void RunOnMainThread(System.Action action)
    {
        lock (executionQueue)
        {
            executionQueue.Enqueue(action);
        }
    }

    void Update()
    {
        lock (executionQueue)
        {
            while (executionQueue.Count > 0)
            {
                executionQueue.Dequeue().Invoke();
            }
        }
    }
}