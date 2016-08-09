﻿using System.Collections.Generic;
using UnityEngine;

namespace Finsternis
{
    public class Card : ScriptableObject
    {
        public enum RARITY : byte
        {
            common = 0, //0000
            uncommon = 1, //0001
            rare = 2, //0010
            legendary = 4, //0100
            godlike = 8  //1000
        };

        [SerializeField]
        private float rarity;

        [SerializeField]
        private int cost;

        [SerializeField]
        private string description;

        private List<Effect> effects;

        private List<CardName> names;

        public RARITY Rarity { get { return (RARITY)this.rarity; } }

        public int Cost { get { return this.cost; } }

        public string Description { get { return this.description; } }

        public Card()
        {
            effects = new List<Effect>();
            names = new List<CardName>();
        }

        public string UpdateName()
        {
            this.name = "";
            names.ForEach(name =>
            {
                this.name += (GetAdditionalNameString(name) ?? (string.IsNullOrEmpty(this.name) ? "" : " ")) + name.name + " ";
            });
            this.name = this.name.TrimEnd();
            return this.name;
        }

        private string GetAdditionalNameString(CardName name)
        {
            if (name.Type != CardName.NameType.MainName)
            {
                if (names.Count > 1 && names.Count - 2 > 0 && names[names.Count - 2].Type == name.Type)
                    return " and ";
                else if (name.Type == CardName.NameType.PostName)
                {
                    return " " + name.prepositions[Random.Range(0, name.prepositions.Count - 1)] + " ";
                }
            }
            return null;
        }

        public void AppendName(CardName name)
        {
            this.names.Add(name);
            this.rarity += name.Rarity;
            AddEffects(name.Effects);
        }

        public void RemoveName(CardName nameToRemove)
        {
            if (names.RemoveAll(cardName => cardName.Equals(nameToRemove)) > 0)
                RefreshEffects();
        }

        public void RemoveName(string nameStr)
        {
            if (names.RemoveAll(cardName => cardName.name.Equals(nameStr)) > 0)
                RefreshEffects();
        }

        private void RefreshEffects()
        {
            effects.Clear();
            names.ForEach(cardName => AddEffects(cardName.Effects));
        }

        private void RemoveName(System.Func<CardName, bool> condition)
        {
            effects.Clear(); //will have to recompute all effects since some may have been overwritte by the ones in the name that will be removed

            names.RemoveAll((cardName) =>
            {
                if (condition(cardName))
                    return true;

                AddEffects(cardName.Effects); //if this name won't be removed, re-add it's effects to the card
                return false;
            });
        }

        public void AddEffect(Effect effect)
        {
            this.effects.Add(effect);
        }

        public void AddEffects(IEnumerable<Effect> effects)
        {
            foreach (Effect e in effects)
                AddEffect(e);
        }

        public List<Effect> GetEffects() { return new List<Effect>(effects); }

        public override string ToString()
        {
            string effects = "";
            if (this.effects.Count > 0)
            {
                effects += "[";
                this.effects.ForEach((effect) => { effects += effect + "]; ["; });
                effects = effects.Substring(0, effects.Length - 3);
            }
            return base.ToString() + " -> Cost: " + this.cost + ", Effects: {" + effects + "}";
        }
    }
}