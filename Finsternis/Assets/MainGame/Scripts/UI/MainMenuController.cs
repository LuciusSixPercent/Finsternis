﻿
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private GameObject _optionsContainer;

    [SerializeField]
    private GameObject _centerDisplay;

    [SerializeField]
    private EventSystem _eventSystem;

    [SerializeField]
    [Range(0, 1)]
    private float _slerpAmount = 0.1f;

    [SerializeField]
    [Range(0, 360)]
    private float _rotationThreshold = 1;

    private GameObject _selectedButton;

    private Dictionary<string, CanvasGroup> groups;
    private bool creditsLoaded;

    void Awake()
    {
        if (!_optionsContainer)
            _optionsContainer = transform.GetChild(0).gameObject;

        if (!_eventSystem)
            _eventSystem = FindObjectOfType<EventSystem>();

        _selectedButton = EventSystem.current.firstSelectedGameObject;

        groups = new Dictionary<string, CanvasGroup>();
    }

    public void StartNewGame(string sceneName)
    {
        GameManager.Instance.LoadScene(sceneName);
    }

    public void ContinueGame()
    {
    }

    public void OpenAlbum()
    {
    }

    public void ShowOptions()
    {
    }

    public void RollCredits()
    {
    }

    public void Exit()
    {
        GameManager.Instance.Exit();
    }

    public void FocusButton(BaseEventData data)
    {
        Transform toRotate = data.selectedObject.transform;
        Transform parent = toRotate.parent;
        while(parent != null && parent != _optionsContainer.transform)
        {
            toRotate = parent;
            parent = toRotate.parent;
        }
        if (!creditsLoaded && data.selectedObject.name.ToLower().Contains("credits"))
            LoadCredits();

        StartCoroutine(Rotate(Quaternion.Euler(new Vector3(0, 0, 360 - toRotate.localRotation.eulerAngles.z)), data.selectedObject));
    }

    private void LoadCredits()
    {
        creditsLoaded = true;
        var reader = new System.IO.StreamReader(Application.dataPath + "/General/credits.txt");
        _centerDisplay.transform.Find("Display").GetChild(0).Find("btnCreditsDisplay").gameObject.GetComponentInChildren<Text>().text = reader.ReadToEnd();
    }

    private IEnumerator Rotate(Quaternion targetRotation, GameObject currentlySelected)
    {
        if (targetRotation != _optionsContainer.transform.rotation)
        {
            CanvasGroup lastSelectedDisplay = GetDisplay(_selectedButton);

            CanvasGroup currentlySelectedDisplay = GetDisplay(currentlySelected);
            if (currentlySelectedDisplay)
                currentlySelectedDisplay.gameObject.SetActive(true);

            _eventSystem.sendNavigationEvents = false;

            float startAngle = Quaternion.Angle(targetRotation, _optionsContainer.transform.rotation);
            float angle = Quaternion.Angle(targetRotation, _optionsContainer.transform.rotation);
            while (angle > _rotationThreshold)
            {
                _optionsContainer.transform.rotation = Quaternion.Slerp(_optionsContainer.transform.rotation, targetRotation, _slerpAmount);

                if (lastSelectedDisplay)
                    lastSelectedDisplay.alpha = angle / startAngle;

                if (currentlySelectedDisplay)
                    currentlySelectedDisplay.alpha = 1 - angle / startAngle;

                angle = Quaternion.Angle(targetRotation, _optionsContainer.transform.rotation);

                yield return new WaitForEndOfFrame();
            }

            if (lastSelectedDisplay)
                lastSelectedDisplay.gameObject.SetActive(false);

            _selectedButton = currentlySelected;
            _optionsContainer.transform.rotation = targetRotation;
            _eventSystem.sendNavigationEvents = true;
        }
    }

    private CanvasGroup GetDisplay(GameObject button)
    {
        string key = button.name + "Display";
        CanvasGroup lastDisplayGroup = null;

        if (!groups.TryGetValue(key, out lastDisplayGroup))
        {
            Transform t = _centerDisplay.transform.Find("Display").GetChild(0).Find(key);
            if (t)
                lastDisplayGroup = t.GetComponent<CanvasGroup>();

            groups.Add(key, lastDisplayGroup);
        }

        return lastDisplayGroup;
    }
}