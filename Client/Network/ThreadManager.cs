using System;
using System.Collections.Generic;
using UnityEngine;

namespace AOClient.Network
{
    public class ThreadManager : MonoBehaviour
    {
        private static ThreadManager instance;
        
        private readonly List<Action> executeOnMainThread = new();
        private readonly List<Action> executeCopiedOnMainThread = new();
        private bool actionToExecuteOnMainThread;

        private void Awake()
        {
            if (instance is null)
            {
                instance = this;
                DontDestroyOnLoad(this);
            }
            else if (instance != this)
            {
                gameObject.SetActive(false);
                Destroy(this);
            }
        }

        private void Update()
        {
            UpdateMain();
        }

        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="action">The action to be executed on the main thread.</param>
        public static void ExecuteOnMainThread(Action action)
        {
            if (action is null)
                return;

            lock (instance.executeOnMainThread)
            {
                instance.executeOnMainThread.Add(action);
                instance.actionToExecuteOnMainThread = true;
            }
        }

        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
        private void UpdateMain()
        {
            if (!actionToExecuteOnMainThread) 
                return;
            
            lock (executeOnMainThread)
            {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;
            }

            foreach (var action in executeCopiedOnMainThread)
                action();
            
            executeCopiedOnMainThread.Clear();
        }
    }
}
