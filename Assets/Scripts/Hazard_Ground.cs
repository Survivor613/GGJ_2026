using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard_Ground : MonoBehaviour
{
    public float damageHazardGround = 15;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 1. 优先筛选 Tag，这样非战斗单位（比如背景装饰）就不会去尝试获取组件，性能更好
        if (!collision.gameObject.CompareTag("Player")
            && !collision.gameObject.CompareTag("Enemy")
            && !collision.gameObject.CompareTag("Chest"))
            return;

        // 2. 尝试获取接口并直接执行伤害
        if (collision.gameObject.TryGetComponent(out IDamagable idamagable))
        {
            idamagable.TakeDamage(damageHazardGround, transform);
        }
    }
}
