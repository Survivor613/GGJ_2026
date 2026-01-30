using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour, IDamagable
{
    private Rigidbody2D rb => GetComponent<Rigidbody2D>();
    private Animator anim => GetComponentInChildren<Animator>();
    private Entity_VFX vfx => GetComponent<Entity_VFX>();
    private Entity_SFX sfx => GetComponent<Entity_SFX>();

    [Header("Open Details")]
    [SerializeField] private Vector2 knockback = new Vector2(0, 5);

    public void TakeDamage(float damage, Transform damageDealer)
    {
        if (anim.GetBool("chestOpen") == false)
        {
            vfx.PlayOnDamageVfx();
            anim.SetBool("chestOpen", true);
            rb.velocity = knockback;
            rb.angularVelocity = Random.Range(-200, 200);

            sfx.EntityOpen(); // Chest Open SFX
        }

        // Drop items
    }
}
