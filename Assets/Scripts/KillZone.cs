using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检查碰撞体是否是玩家
        Player player = collision.GetComponent<Player>();

        if (player != null)
        {
            // 直接调用你写好的 EntityDeath
            // 这会触发：播放音效、切到 deadState 状态机、通知 UI 显示死亡界面
            player.EntityDeath();
        }
    }
}
