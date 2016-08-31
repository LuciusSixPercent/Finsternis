﻿namespace Finsternis
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System;

    public class TransitionManager : MonoBehaviour
    {
        private List<Transition> playing;
        private List<Transition> toRemove;
        private bool skipping;

        void Awake()
        {
            playing = new List<Transition>();
            Transition[] transitions = gameObject.GetComponentsInChildren<Transition>();
            foreach (var t in transitions)
            {
                t.OnTransitionStarted.AddListener(WatchTransition);
                t.OnTransitionEnded.AddListener(UnwatchTransition);
            }
        }

        private void UnwatchTransition(Transition t)
        {
            t.OnTransitionStarted.RemoveListener(WatchTransition);
            t.OnTransitionEnded.RemoveListener(UnwatchTransition);
            if (!skipping)
                playing.Remove(t);
            else
                toRemove.Add(t);
        }

        private void WatchTransition(Transition t)
        {
            playing.Add(t);
        }

        public void Skip()
        {
            skipping = true;
            toRemove = new List<Transition>();
            foreach (var t in playing)
                t.Skip();
            skipping = false;

            for (int i = toRemove.Count - 1; i >= 0; i--)
            {
                playing.Remove(toRemove[i]);
                toRemove.RemoveAt(i);
            }
        }
    }
}