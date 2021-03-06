﻿namespace Finsternis
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityQuery;

    [Serializable]
    public abstract class Effect
    {
        [SerializeField][ReadOnly]
        protected string name;

        [SerializeField]
        protected List<EffectConstraint> constraints;

        public string Name { get { return this.name; } }

        public int ConstraintsCount
        {
            get
            {
                return (this.constraints != null ? this.constraints.Count : 0);
            }
        }

        public Effect(string name = null)
        {
            this.constraints = new List<EffectConstraint>();
            if (!string.IsNullOrEmpty(name))
                this.name = name;
        }

        public static implicit operator bool(Effect e)
        {
            return e != null;
        }

        public void AddConstraint(EffectConstraint constraint)
        {
            if (!constraint.IsValid(this))
                return;
            constraints.Add(constraint);
        }

        public bool HasConstraint<T>() where T : IConstraint
        {
            return HasConstraint(typeof(T));
        }

        private bool HasConstraint(Type t)
        {
            return GetConstraint(t) != null;
        }

        public T GetConstraint<T>() where T : EffectConstraint
        {
            return (T)GetConstraint(typeof(T));
        }

        private Constraint GetConstraint(Type t)
        {
            return constraints == null ? null : constraints.Find((constraint) => { return constraint.GetType().Equals(t); });
        }

        /// <summary>
        /// Should this effect be taken in consideation?
        /// </summary>
        /// <returns>True if every constraint is valid</returns>
        public virtual bool ShouldBeActive()
        {
            return constraints.Find((constraint) => { return !constraint.IsValid(this); }) == null;
        }

        public override string ToString()
        {
            return base.ToString() + ((constraints != null && constraints.Count > 0) ? ", constraints: {" + StringfyConstraints() + "}" : "");
        }

        private string StringfyConstraints()
        {
            if (this.constraints.Count == 0)
                return null;

            string constraintsStr = null;

            this.constraints.ForEach(constraint => constraintsStr += (constraint.ToString() + ","));

            return constraintsStr.Substring(0, constraintsStr.Length - 2);
        }
    }
}