﻿namespace Finsternis
{
    using UnityEngine;
    using System.Collections.Generic;
    using System;

    [RequireComponent(typeof(Animator))]
    public class Chest : OpeneableEntity
    {
        private Animator animator;

        [Serializable]
        public struct RangedValue
        {
            public int min;
            public int max;

            public RangedValue(int min, int max)
            {
                this.min = min;
                this.max = max;
            }
        }

        public RangedValue rangeOfCardsToGive = new RangedValue(1, 3);

        private int cardsToGive;

        protected override void Awake()
        {
            base.Awake();
            this.animator = GetComponent<Animator>();
            this.cardsToGive = Dungeon.Random.IntRange(this.rangeOfCardsToGive.min, this.rangeOfCardsToGive.max);
        }

        public void OpenChest()
        {
            if (LastInteraction && LastInteraction.Agent.Equals(GameManager.Instance.Player))
            {
                FindObjectOfType<CardsManager>().GivePlayerCard(cardsToGive);
            }
            interactable = false;
            this.animator.SetTrigger("Open");
        }

        void OnValidate()
        {
            this.rangeOfCardsToGive.min = Mathf.Min(this.rangeOfCardsToGive.min, this.rangeOfCardsToGive.max);
        }
    }
}