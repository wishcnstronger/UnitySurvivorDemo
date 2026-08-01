using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 敌人移动组件。
    /// 每帧朝玩家方向移动，使用最简单的 MoveTowards 方式。
    /// </summary>
    public class EnemyMovement : MonoBehaviour
    {
        /// <summary>移动速度，单位/秒</summary>
        public float moveSpeed = 3f;

        /// <summary>玩家 Transform 引用</summary>
        private Transform player;

        /// <summary>游戏开始时查找玩家</summary>
        private void Start()
        {
            // 通过 Tag 查找玩家
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        /// <summary>每帧朝玩家移动</summary>
        private void Update()
        {
            // 玩家不存在（已死亡或未生成）就停止
            if (player == null)
                return;

            // 朝玩家方向移动，最简单的方式
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }
    }
}
