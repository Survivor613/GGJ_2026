using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") == true)
        {
            Debug.Log("Respawn Point Saved!");

            collision.gameObject.TryGetComponent(out Player player);
            player.respawnLoc = player.transform.position;
        }
    }
}
