﻿namespace Finsternis
{
    using UnityEngine;
    using System.Collections;
    using System;
    using System.Collections.Generic;
    using UnityQuery;
    using System.Linq;
    using UnityEngine.UI;

    public class InventoryMenuController : MenuController
    {
        [SerializeField]
        private ConfirmationDialogController confirmationDialog;

        [SerializeField]
        private Text usedPointsText;

        [SerializeField, ReadOnly]
        private GameObject[] visibleUnequippedCards;

        [SerializeField, ReadOnly]
        private GameObject[] visibleEquippedCards;

        private GameObject unequippedControls;
        private GameObject equippedControls;

        private int unequippedSelection = -1;
        private int equippedSelection = -1;

        private Inventory inventory;
        private bool equippedDisplayUpdatePending;
        private bool unequippedDisplayUpdatePending;

        protected override void Awake()
        {
            base.Awake();

            Transform unequippedPanel = transform.Find("UnequippedPanel");
            this.visibleUnequippedCards = GetCards(unequippedPanel);
            this.unequippedControls = unequippedPanel.Find("Controls").gameObject;
            this.unequippedDisplayUpdatePending = true;

            Transform equippedPanel = transform.Find("EquippedPanel");
            this.visibleEquippedCards = GetCards(equippedPanel);
            this.equippedControls = equippedPanel.Find("Controls").gameObject;
            this.equippedDisplayUpdatePending = true;
        }

        private GameObject[] GetCards(Transform cardsPanel)
        {
            var cards = cardsPanel.GetComponentsInChildren<CardDisplay>(true);
            return new GameObject[]
            {
                cards[0].gameObject,
                cards[2].gameObject,
                cards[1].gameObject

            };
        }

        private Inventory GetInventory()
        {
            if (this.inventory)
                return this.inventory;
            else
            {
                if (!this.inventory)
                {
                    this.inventory = GameManager.Instance.Player.GetComponent<Inventory>();
                }
                if (this.inventory)
                {
                    this.inventory.onCardAdded.AddListener(card => UpdateUnequippedDisplay());
                    this.inventory.onCardRemoved.AddListener(card => UpdateUnequippedDisplay());
                    this.inventory.onCardEquipped.AddListener(card => UpdateEquippedDisplay());
                    this.inventory.onCardUnequipped.AddListener(card => UpdateEquippedDisplay());
                }
                return this.inventory;
            }
        }

        public override void BeginOpening()
        {
            base.BeginOpening();

            if (!GetInventory())
            {
                StopAllCoroutines();
                BeginClosing();
                return;
            }

            if(this.unequippedDisplayUpdatePending)
                UpdateUnequippedDisplay();

            if(this.equippedDisplayUpdatePending)
                UpdateEquippedDisplay();

            UpdateCostLabel();
        }

        private void UpdateControlsDisplay(GameObject controls, int cardsCount, int selection)
        {
            controls.Activate();
            if (cardsCount <= 1)
                controls.Deactivate();
            else
            {
                var transform = controls.transform;
                if (cardsCount == 2)
                {
                    transform.GetChild(selection).gameObject.Deactivate();
                    transform.GetChild((selection + 1) % 2).gameObject.Activate();
                }
                else
                {
                    transform.GetChild(0).gameObject.Activate();
                    transform.GetChild(1).gameObject.Activate();
                }
            }
        }

        private void UpdateCostLabel()
        {
            if (!this.inventory)
                return;
            this.usedPointsText.text = this.inventory.TotalEquippedCost + " / " + this.inventory.AllowedCardPoints;
        }

        #region display methods
        private bool ActivateCards(GameObject[] display, IList cardsList, ref int selection)
        {
            foreach (var visibleCard in display)
                visibleCard.SetActive(false);

            if (cardsList.Count == 0)
                return false;

            selection = Mathf.Max(selection, 0);
            return true;
        }

        int UpdateSelection(List<CardStack> cards, GameObject[] visibleCards, int selection)
        {
            if (ActivateCards(visibleCards, cards, ref selection))
            {
                ShowCardDisplay(1, selection, visibleCards, cards);

                if (cards.Count > 1)
                {
                    if (cards.Count == 2) //if only 2 cards are equipped, either the display above or below won't be visible
                    {
                        //if currently selected = 0, display the card "below" (visibleCards[2]) -> 2 - 0 * 2 = 2
                        //if currently selected = 1, display the card "above" (visibleCards[0]) -> 2 - 1 * 2 = 0
                        int visibleCardIndex = 2 - selection * 2;
                        ShowCardDisplay(visibleCardIndex, selection, visibleCards, cards);
                    }
                    else
                    {
                        ShowCardDisplay(0, selection, visibleCards, cards);
                        ShowCardDisplay(2, selection, visibleCards, cards);
                    }
                }
            }
            return selection;
        }

        void UpdateUnequippedDisplay()
        {
            if (!this.inventory || this.visibleEquippedCards == null || !this.isActiveAndEnabled)
            {
                this.unequippedDisplayUpdatePending = true;
                return;
            }

            this.unequippedDisplayUpdatePending = false;

            this.unequippedSelection = UpdateSelection(this.inventory.UnequippedCards, this.visibleUnequippedCards, this.unequippedSelection);

            UpdateControlsDisplay(this.unequippedControls, this.inventory.UnequippedCards.Count, this.unequippedSelection);
        }

        void UpdateEquippedDisplay()
        {
            if (!this.inventory || this.visibleEquippedCards == null || !this.isActiveAndEnabled)
            {
                this.equippedDisplayUpdatePending = true;
                return;
            }

            this.equippedDisplayUpdatePending = false;
            this.equippedSelection = UpdateSelection(this.inventory.EquippedCards, this.visibleEquippedCards, this.equippedSelection);

            UpdateControlsDisplay(this.equippedControls, this.inventory.EquippedCards.Count, this.equippedSelection);
        }

        void ShowCardDisplay(int displayIndex, int selectedIndex, GameObject[] displayArray, List<CardStack> cardsList)
        {
            //if display = 0, index in list = selected - 1
            //if display = 1, index in list = selected
            //if display = 2, index in list = selected + 1
            int listIndex = selectedIndex + displayIndex - 1;

            //Clamp index within list range, making it wrap around
            if (listIndex < 0)
                listIndex = cardsList.Count - 1;
            else if (listIndex >= cardsList.Count)
                listIndex = 0;

            displayArray[displayIndex].SetActive(true);
            var controller = displayArray[displayIndex].GetComponent<CardDisplay>();
            controller.LoadStack(cardsList[listIndex]);
            if (displayIndex == 1)
                this.inventory.RemoveFromNew(controller.Card);
        }
        #endregion

        #region selection methods
        public void MoveUnequipped(float value)
        {
            if (value == 0)
                return;
            else if(this.inventory.UnequippedCards.Count < 3)
            {
                if (value < 0 && this.unequippedSelection == 0)
                    return;
                else if (value > 0 && this.unequippedSelection == 1)
                    return;
            }

            this.unequippedSelection = MoveSelection(this.unequippedSelection, this.inventory.UnequippedCards, value < 0 ? -1 : 1);
            UpdateUnequippedDisplay();
        }

        public void MoveEquipped(float value)
        {
            if (value == 0)
                return;
            else if (this.inventory.EquippedCards.Count < 3)
            {
                if (value < 0 && this.equippedSelection == 0)
                    return;
                else if (value > 0 && this.equippedSelection == 1)
                    return;
            }

            this.equippedSelection = MoveSelection(this.equippedSelection, this.inventory.EquippedCards, value < 0 ? -1 : 1);
            UpdateEquippedDisplay();
        }

        private int MoveSelection(int currentlySelected, IList list, int displayOffset)
        {
            currentlySelected += displayOffset;
            if (currentlySelected < 0)
                currentlySelected = list.Count - 1;
            else if (currentlySelected >= list.Count)
                currentlySelected = 0;

            return currentlySelected;
        }

        public void EquipSelected(bool askForConfirmation)
        {
            var unequipped = this.inventory.UnequippedCards;

            if (unequipped.Count == 0 || !this.inventory.CanEquip(unequipped[this.unequippedSelection].card))
                return;

            if (askForConfirmation)
            {
                var input = GetCachedComponent<InputRouter>();
                input.Disable();
                confirmationDialog.Show(
                    "Equipped cards remain so until the end of the floor.",
                    () => { input.Enable(); EquipSelected(false); this.Selectable.Select(); },
                    () => { input.Enable(); this.Selectable.Select(); },
                    "Equip!",
                    "Cancel");
            }
            else
            {
                var equippedCard = unequipped[this.unequippedSelection].card;
                if (inventory.EquipCard(unequipped[this.unequippedSelection]))
                {
                    UpdateUnequippedSelection(equippedCard);
                    UpdateEquippedSelection(equippedCard);
                    UpdateCostLabel();
                }
            }
        }

        private void UpdateEquippedSelection(Card equippedCard)
        {
            //if the newly equipped card wasn't already selected, select it
            var equippedCards = this.inventory.EquippedCards;

            if (this.equippedSelection >= equippedCards.Count ||
                equippedCard != equippedCards[this.equippedSelection].card)
            {
                this.equippedSelection = this.inventory.EquippedCards.IndexOf(
                           this.inventory.GetStack(this.inventory.EquippedCards, equippedCard));
                UpdateEquippedDisplay();
            }
        }

        private void UpdateUnequippedSelection(Card equippedCard)
        {
            var unequippedCards = this.inventory.UnequippedCards;
            this.unequippedSelection = Mathf.Clamp(this.unequippedSelection, 0, unequippedCards.Count - 1);
            //If the equipped card was removed from the unequipped list, update the unequipped selection
            if (this.unequippedSelection >= unequippedCards.Count || this.unequippedSelection < 0 ||
                unequippedCards[this.unequippedSelection].card != equippedCard)
            {
                UpdateUnequippedDisplay();
            }
        }
        #endregion
    }
}