using UnityEngine;
using UnityEngine.Tilemaps;

public class BreakableTilemap : MonoBehaviour, IDamagable
{
    private Tilemap tilemap;

    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public void TakeDamage(float damage, Transform damageDealer)
    {
        Vector3 hitPosition = damageDealer.Find("TargetCheck").position;
        Vector3Int cellPos = tilemap.WorldToCell(hitPosition);

        if (tilemap.HasTile(cellPos))
        {
            tilemap.SetTile(cellPos, null);

            // 惩罚逻辑
            //Player player = damageDealer.GetComponent<Player>();
            //if (player != null)
            //{
            //    player.ApplySpeedPenalty();
            //}

            // 视觉反馈优化：在瓦片中心生成特效，而不是在 Player 手上
            //Vector3 tileWorldPos = tilemap.GetCellCenterWorld(cellPos);
            // vfx.CreateOnHitVFX(tileWorldPos); 
        }
    }
}