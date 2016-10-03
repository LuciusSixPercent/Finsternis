﻿namespace Finsternis {
    using UnityEngine;
    using System.Collections.Generic;
    using System.Collections;
    using UnityQuery;

    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private Follow follow;

        [SerializeField]
        [Range(1, 100)]
        private float shakeDamping = 2;

        [SerializeField]
        [Range(1, 20)]
        private float shakeFrequency = 20;

        [SerializeField]
        [Range(1, 100)]
        private float shakeAmplitude = 20;

        [SerializeField][Range(1, 100)]
        private float maxDistanceForOcclusion = 5f;

        private bool shaking;
        
        private Vector3 lastTarget;
        private Coroutine shakeHandle;

        [SerializeField]
        private bool reactToOcclusion = true;

        public bool ReactToOcclusion {
            get {return this.reactToOcclusion; }
            set { this.reactToOcclusion = value; }
        }

        void Awake()
        {
            if (!this.follow)
                this.follow = GetComponentInParent<Follow>();
            shaking = false;
        }

        void FixedUpdate()
        {
            if (!this.follow)
                return;
            if (!this.follow.Target)
                return;

            RaycastHit hit;
            bool occlusionHappened = false;
            if (reactToOcclusion && WouldBeOccluded(out hit))
            {
                float distanceDampening = 1f - hit.distance / maxDistanceForOcclusion;
                this.follow.MemorizeOffset(this.follow.OriginalOffset + (Vector3.up * 2 + Vector3.forward * 3) * distanceDampening);
                occlusionHappened = true;
            }

            if (!this.shaking)
                this.follow.ResetOffset(!occlusionHappened);

        }

        private bool WouldBeOccluded(out RaycastHit hit)
        {
            Vector3 origin = this.follow.Target.position + this.follow.Target.transform.forward / 10;
            int mask = 1 << LayerMask.NameToLayer("Wall");
            
            Ray ray = new Ray(origin + Vector3.up / 2, origin.Towards(origin+this.follow.OriginalOffset));
            float radius = 0.5f;

            return (Physics.SphereCast(ray, radius, out hit, maxDistanceForOcclusion, mask)) ;
        }

        private void FinishedInterpolating()
        {
            this.follow.translationInterpolation = 0.1f;
            this.follow.MemorizeOffset(this.follow.OriginalOffset);
            this.follow.OnTargetReached.RemoveListener(FinishedInterpolating);
        }

        internal void Shake(float time, float damping, float amplitude, float frequency, bool overrideShake = true)
        {
            if (overrideShake && this.shaking)
            {
                this.shaking = false;
                StopCoroutine(this.shakeHandle);
            }

            if (!this.shaking)
            {
                this.shakeDamping = damping;
                this.shakeFrequency = frequency;
                this.shakeAmplitude = amplitude;
                this.shakeHandle = StartCoroutine(_Shake(time));
            }
        }

        IEnumerator _Shake(float shakeTime)
        {
            this.shaking = true;
            float amplitude = this.shakeAmplitude;
            while (shakeTime > 0)
            {
                yield return Wait.Sec(1 / this.shakeFrequency);
                shakeTime -= Time.deltaTime + 1 / this.shakeFrequency;

                Vector3 shakeOffset = Random.insideUnitSphere / 10;

                shakeOffset.z = 0;
                transform.localPosition = shakeOffset * amplitude;
                transform.localRotation = Quaternion.Euler(new Vector3(Random.value, Random.value, Random.value) * amplitude / 5);
                amplitude /= this.shakeDamping;
            }

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            this.shaking = false;
        }
    }
}