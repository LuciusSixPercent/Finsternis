﻿namespace Finsternis
{
    using UnityEngine;
    using UnityEngine.UI;
    using MovementEffects;
    using System;
    using System.Collections.Generic;
    using UnityEngine.EventSystems;
    using UnityQuery;
    using UnityEngine.Events;

    [RequireComponent(typeof(Image))]
    [DisallowMultipleComponent]
    public class MenuEyeController : MonoBehaviour
    {

        [SerializeField]
        private Circle eyeBounds;

        [SerializeField]
        [Range(0.01f, 1)]
        private float movementInterpolationAmount = 0.2f;

        [SerializeField][Range(0,1)]
        private float distanceThreshold = 0.1f;

        public UnityEvent OnBeganMoving;
        public UnityEvent OnTargetReached;

        private IEnumerator<float> lookAtEnumerator;

        private GameObject pupil;
        private GameObject Pupil
        {
            get
            {
                if (!pupil)
                    GetPupil();
                return pupil;
            }
        }

        private void GetPupil()
        {
            try
            {
                pupil = transform.FindDescendent("Pupil").gameObject;
            }
            catch (NullReferenceException ex)
            {
                Log.Error(this, "Failed to find eye pupil.\n" + ex.Message);
            }
        }

        void Awake()
        {
            var t = GetComponent<RectTransform>(); 
            eyeBounds.center = t.anchoredPosition;
            if (eyeBounds.radius == 0)
                eyeBounds.radius = t.sizeDelta.Min() / 2;
        }

        public void Reset()
        {
            if (lookAtEnumerator != null)
                Timing.KillCoroutines(lookAtEnumerator);

            lookAtEnumerator = Timing.RunCoroutine(_LookAtTarget(Vector2.zero));
        }

        public void LookAt(BaseEventData data)
        {
            if (!Pupil)
                return;
            LookAt(data.selectedObject);
        }

        public void LookAt(GameObject target)
        {
            LookAt(eyeBounds.ProjectPoint(target.GetComponent<RectTransform>().anchoredPosition));
        }

        public void LookAt(Vector2 target)
        {
            if (lookAtEnumerator != null)
                Timing.KillCoroutines(lookAtEnumerator);

            lookAtEnumerator = Timing.RunCoroutine(_LookAtTarget(target));
        }

        private IEnumerator<float> _LookAtTarget(Vector2 target)
        {
            if (Pupil)
            {
                OnBeganMoving.Invoke();

                var transform = Pupil.GetComponent<RectTransform>();
                Vector2 currentPos = transform.anchoredPosition;
                float initialDistance = Vector2.Distance(currentPos, target);

                do
                {
                    currentPos = Vector3.Slerp(currentPos, target, this.movementInterpolationAmount);
                    transform.anchoredPosition = currentPos;

                    yield return 0;
                } while (Vector2.Distance(currentPos, target) / initialDistance >= distanceThreshold);

                transform.anchoredPosition = target;
                OnTargetReached.Invoke();
            } else
            {
                Log.Error("No game object asigned as pupil");
            }
        }

        void OnDisable()
        {
            Timing.KillCoroutines(lookAtEnumerator);
        }
    }
}