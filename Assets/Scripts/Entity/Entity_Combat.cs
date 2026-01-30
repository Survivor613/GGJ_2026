using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity_Combat : MonoBehaviour
{
    protected Entity_SFX sfx;
    protected Entity_VFX vfx;
    public float damage = 10;

    [Header("Target detection")]
    [SerializeField] private Transform targetCheck;
    [SerializeField] private float targetCheckRadius = 1;
    [SerializeField] private LayerMask whatIsTarget;

    private void Awake()
    {
        vfx = GetComponent<Entity_VFX>();
        sfx = GetComponent<Entity_SFX>();
    }

    public void PerformAttack()
    {
        // 打到东西
        foreach (var target in GetDetectedColliders())
        {
            //sfx?.EntityAttackHit(); // AttackHit SFX

            // 通过 IDamagable的interface, 实现Entity_Health的TakeDamage与Chest的TakeDamage两个截然不同的逻辑的统一
            IDamagable damagable = target.GetComponent<IDamagable>();

            if (damagable == null)
                continue;

            damagable.TakeDamage(damage, transform);
            vfx.CreateOnHitVFX(target.transform);
             
        }
            
        // 空挥
        //sfx?.EntityAttackMiss(); // AttackMiss SFX
    }

    protected Collider2D[] GetDetectedColliders()
    {
        return Physics2D.OverlapCircleAll(targetCheck.position, targetCheckRadius, whatIsTarget);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(targetCheck.position, targetCheckRadius);
    }
}
