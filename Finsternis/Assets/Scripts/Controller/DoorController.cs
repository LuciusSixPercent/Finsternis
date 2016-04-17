﻿using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DoorController : MonoBehaviour
{

    public void Open()
    {
        Vector3 damageSource = transform.position - GetComponent<Character>().lastAttacker.transform.position;

        Animator anim = GetComponent<Animator>();
        anim.SetTrigger("opening");
        anim.SetInteger("direction", (Vector3.Angle(transform.forward, damageSource) < 90) ? 1 : -1);
    }

}
