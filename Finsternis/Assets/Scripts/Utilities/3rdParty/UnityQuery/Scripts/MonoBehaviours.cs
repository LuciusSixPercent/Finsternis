﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MonoBehaviours.cs">
//   Copyright (c) Victor Lucki. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace UnityQuery
{
    using System;
    using System.Collections;
    using UnityEngine;

    public static class MonoBehaviours
    {
        public static void Enable(this MonoBehaviour b)
        {
            b.enabled = true;
        }

        public static void Disable(this MonoBehaviour b)
        {
            b.enabled = false;
        }

        public static Coroutine CallDelayed(this MonoBehaviour mb, float delayInSeconds, Action callback)
        {
            return mb.StartCoroutine(DelayRoutine(delayInSeconds, callback));
        }

        public static Coroutine CallDelayed<T>(this MonoBehaviour mb, float delayInSeconds, Action<T> callback, T arg)
        {
            return mb.StartCoroutine(DelayRoutine(delayInSeconds, callback, arg));
        }

        private static IEnumerator DelayRoutine(float delayInSeconds, Action callback)
        {
            yield return Wait.Sec(delayInSeconds);
            callback();
        }

        private static IEnumerator DelayRoutine<T>(float delayInSeconds, Action<T> callback, T arg)
        {
            yield return Wait.Sec(delayInSeconds);
            callback(arg);
        }
    }
}