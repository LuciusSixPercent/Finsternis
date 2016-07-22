﻿using UnityEngine;
using System;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class Entity : MonoBehaviour
{
    public UnityEvent onInteraction;

    public EntityAction lastInteraction;

    public EntityAttribute test;

    [SerializeField]
    protected bool interactable = true;

    protected T CheckAttribute<T>(T attribute, string name) where T : EntityAttribute
    {
        if (!attribute)
            attribute = (T)(GetAttribute(name));
        if (!attribute)
        {
            attribute = gameObject.AddComponent<T>();
            attribute.Alias = name;
        }

        if (!attribute)
            throw new NullReferenceException("Could not load attribute " + name + "\nMaybe it was not set in the inspector?");

        return attribute;
    }

    public EntityAttribute GetAttribute(string name)
    {
        EntityAttribute[] attributes = GetComponents<EntityAttribute>();
        foreach (EntityAttribute attribute in attributes)
            if (attribute.Alias.Equals(name))
                return attribute;
        return null;
    }

    public virtual void Interact(EntityAction action)
    {
        lastInteraction = action;
        if(onInteraction != null)
            onInteraction.Invoke();
    }

    public virtual void AtributeUpdated(EntityAttribute attribute)
    {
    }

    public void Kill()
    {
        Destroy(gameObject);
    }
}