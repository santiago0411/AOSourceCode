using System;
using System.Collections.Generic;
using UnityEngine;

namespace AO.Network
{ 
    public class ThreadManager : MonoBehaviour
    {
        private static readonly List<(Action<object>, object)> executeOnMainThread = new();
        private static readonly List<(Action<object>, object)> executeCopiedOnMainThread = new();
        private static bool actionToExecuteOnMainThread;

        private void FixedUpdate()
        {
            UpdateMain();
        }

        /// <summary>Sets an action to be executed on the main thread.</summary>
        /// <param name="action">The action to be executed on the main thread.</param>
        /// <param name="state">Object to be passed.</param>
        public static void ExecuteOnMainThread(Action<object> action, object state)
        {
            if (action is null)
                return;

            lock (executeOnMainThread)
            {
                executeOnMainThread.Add((action, state));
                actionToExecuteOnMainThread = true;
            }
        }

        /// <summary>Executes all code meant to run on the main thread. NOTE: Call this ONLY from the main thread.</summary>
        public static void UpdateMain()
        {
            if (!actionToExecuteOnMainThread) return;

            lock (executeOnMainThread)
            {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;
            }

            foreach (var (action, state) in executeCopiedOnMainThread)
                action(state);
            
            executeCopiedOnMainThread.Clear();
        }
    }
}