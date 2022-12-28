using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using AO.Core.Logging;
using AO.Core.Utils;
using JetBrains.Annotations;

namespace AO.Core
{
    public static class AoDebug
    { 
        private static readonly LoggerAdapter log = new(typeof(AoDebug));
        
        [MeansImplicitUse]
        [AttributeUsage(AttributeTargets.Method)]
        [Conditional("AO_ASSERTS")]
        public class StaticAssertAttribute : Attribute { }

        private class AssertionFailedException : Exception
        {
            public AssertionFailedException(string message)
                : base(message) {}
        }

        [Conditional("AO_ASSERTS")]
        public static void Assert(bool condition, string message = "Condition failed",
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "", 
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (!condition)
                AssertInternal($"[ASSERT] - {message}. Caller: {sourceFilePath}: {memberName}-{sourceLineNumber}");
        }

        [Conditional("AO_ASSERTS")]
        private static void AssertInternal(string message)
        {
            log.Error(message);

#if UNITY_EDITOR
            if (Debugger.IsAttached)
                Debugger.Break();
            else
                UnityEngine.Debug.Break();
#else
            throw new AssertionFailedException(message);
#endif
        }

#if AO_PROFILING
        private static readonly Dictionary<int, Stopwatch> activeTimers = new(50);
#endif
        
        [Conditional("AO_PROFILING")]
        public static void BeingTimer([CallerMemberName] string callerName = "")
        {
            BeingTimer(callerName.GetHashCode());
        }
        
        [Conditional("AO_PROFILING")]
        public static void BeingTimer(int timerId)
        {
            var sw = new Stopwatch();
            activeTimers.Add(timerId, sw);
            sw.Start();
        }
        
        [Conditional("AO_PROFILING")]
        public static void EndTimer<T>(int timerId, [CallerMemberName] string callerName = "")
        {
            var timeElapsed = activeTimers.PopKey(timerId).GetMilliseconds();
            string callerClass = typeof(T).Name;
            log.Info($"[PROFILING] - {callerClass}::{callerName} took {timeElapsed:##.####}ms.");
        }
        
        [Conditional("AO_PROFILING")]
        public static void EndTimer<T>([CallerMemberName] string callerName = "")
        {
            var timeElapsed = activeTimers.PopKey(callerName.GetHashCode()).GetMilliseconds();
            string callerClass = typeof(T).Name;
            log.Info($"[PROFILING] - {callerClass}::{callerName} took {timeElapsed:##.####}ms.");
        }
    }
}