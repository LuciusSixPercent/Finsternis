﻿namespace Finsternis
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System;

    public class TransitionManager : MonoBehaviour
    {
        private List<Transition> playing;

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
            playing.Remove(t);
        }

        private void WatchTransition(Transition t)
        {
            playing.Add(t);
        }

        public void Skip()
        {
            for(int i = playing.Count - 1; i >= 0; i--)
                playing[i].Skip();
        }
    }
}