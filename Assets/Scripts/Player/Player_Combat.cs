using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Combat : Entity_Combat
{
    [Header("Counter attack details")]
    [SerializeField] private float counterRecovery = .25f;
    [SerializeField] private float counterAttackDamage = 15;

    public bool CounterAttackPerformed()
    {
        bool hasPerformedCounter = false;

        foreach (var target in GetDetectedColliders())
        {
            // 通过 ICounterable的interface, 实现不同Enemy的HandleCounter逻辑的统一
            ICounterable counterable = target.GetComponent<ICounterable>();

            if (counterable == null)
                continue;

            if (counterable.CanBeCountered) // 处于可招架状态 (感叹号)
            {
                // 收到伤害(所有Enemy), 进入僵直(特定类, 如Enemy_Skeleton)
                counterable.HandleCounter(counterAttackDamage, transform);
                vfx.CreateOnHitVFX(target.transform);
                //sfx.EntityAttackHit();

                hasPerformedCounter = true;
            }
        }

        return hasPerformedCounter;
    }

    public float GetCounterRecoveryDuration() => counterRecovery;
}
