using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_SFX : MonoBehaviour
{
    protected AudioSource audioSource;

    protected void Awake()
    {
        audioSource = GetComponentInChildren<AudioSource>();
    }

    public virtual void EntityAttackHit()
    {

    }

    public virtual void EntityAttackMiss()
    {

    }

    public virtual void EntityDash()
    {

    }

    public virtual void EntityJump()
    {

    }

    public virtual void EntityJumpLand()
    {

    }

    public virtual void EntityMove()
    {

    }

    public virtual void EntityDeath()
    {

    }

    public virtual void EntityOpen()
    {

    }

    public virtual void EntityPlayerDetected()
    {

    }
}
