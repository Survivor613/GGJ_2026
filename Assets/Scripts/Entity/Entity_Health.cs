using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Entity_Health : MonoBehaviour, IDamagable
{
    private Slider healthBar;
    private Entity_VFX entityVfx;
    private Entity entity;
     
    [SerializeField] protected float maxHp = 100;
    [SerializeField] protected float currentHp;
    [SerializeField] protected bool isDead;

    [Header("On Damage Knockback")]
    [SerializeField] private float knockbackDuration = .2f;
    [SerializeField] private Vector2 onDamageKnockback = new Vector2(0.75f, 1.25f);

    [Space]
    [SerializeField] private float heavyKnockThreshold = .3f;
    [SerializeField] private float heavyKnockbackDuration = .5f;
    [SerializeField] private Vector2 onHeavyDamageKnockback = new Vector2(3.5f, 3.5f);

    [Space]
    // 无需 Duration，后续赋值 duration = -1 代表收到 Hazard Ground Knockback
    [SerializeField] private Vector2 onHazardGroundKnockback = new Vector2(0, 5);

    protected virtual void Awake()
    {
        entity = GetComponent<Entity>();
        entityVfx = GetComponent<Entity_VFX>();
        healthBar = GetComponentInChildren<Slider>();

        currentHp = maxHp;
        UpdateHealthBar();
    }
    
    public virtual void TakeDamage(float damage, Transform damageDealer)
    {
        if (isDead)
            return;

        Vector2 knockback = CalculateKnockback(damage, damageDealer);
        float duration = CalculateDuration(damage, damageDealer);

        entityVfx?.PlayOnDamageVfx();
        entity?.ReceiveKnockback(knockback, duration);
        ReduceHp(damage);
    }

    public float GetHp() => currentHp;

    public void ReduceHp(float damage)
    {
        currentHp -= damage;
        UpdateHealthBar();

        if (currentHp <= 0)
            Die();
    }

    public void ResetHp()
    {
        currentHp = maxHp;
        isDead = false;
    }

    protected virtual void Die()
    {
        isDead = true;
        entity.EntityDeath();
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        healthBar.value = currentHp / maxHp;
    }

    private Vector2 CalculateKnockback(float damage, Transform damageDealer)
    {
        Vector2 knockback;

        if (damageDealer.CompareTag("Hazard Ground"))
        {
            knockback = onHazardGroundKnockback;
            return knockback;
        }
        else
        {
            int direction = transform.position.x < damageDealer.position.x ? -1 : 1;
            knockback = IsHeavyDamage(damage) ? onHeavyDamageKnockback : onDamageKnockback;
            knockback.x *= direction;
            return knockback;
        }
        
    }

    private float CalculateDuration(float damage, Transform damageDealer)
    {
        if (damageDealer.CompareTag("Hazard Ground"))
            return -1;  // Signifying that entity is knocked back by harzard ground
        else
            return IsHeavyDamage(damage) ? heavyKnockbackDuration : knockbackDuration;
    }

    private bool IsHeavyDamage(float damage) => damage / maxHp > heavyKnockThreshold;
}
