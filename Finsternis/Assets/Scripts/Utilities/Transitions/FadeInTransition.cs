﻿namespace Finsternis
{
    using System;
    using UnityEngine;

    [AddComponentMenu("Finsternis/Transitions/Fade In")]
    public class FadeInTransition : FadeTransition
    {
        protected override void Awake()
        {
            transitionType = FadeType.FadeIn;
            base.Awake();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            transitionType = FadeType.FadeIn;
            base.OnValidate();
        }
#endif
    }
}