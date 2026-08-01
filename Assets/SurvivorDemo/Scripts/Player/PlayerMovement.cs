using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 玩家移动控制。
    /// Phase1 只做 WASD 移动，不涉及战斗、升级等后续系统。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerMovement : MonoBehaviour
    {
        private Rigidbody2D rb;
        private PlayerStats stats;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stats = GetComponent<PlayerStats>();
            rb.gravityScale = 0f;
        }

        private void Update()
        {
            // 读取输入
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector2 inputDir = new Vector2(horizontal, vertical).normalized;

            // 设置速度
            rb.velocity = inputDir * stats.MoveSpeed;
        }
    }
}
