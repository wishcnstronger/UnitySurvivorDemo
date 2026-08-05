using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 玩家移动控制。
    /// WASD 移动 + 限制在可移动区域内（由 GameSetup 创建时注入边界）。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerMovement : MonoBehaviour
    {
        /// <summary>可移动区域半宽（世界坐标，由 GameSetup 注入）</summary>
        [HideInInspector]
        public float boundX = 15f;

        /// <summary>可移动区域半高（世界坐标，由 GameSetup 注入）</summary>
        [HideInInspector]
        public float boundY = 15f;

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
            // 死亡后停止移动
            if (stats.CurrentHP <= 0f)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            // 读取输入
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            Vector2 inputDir = new Vector2(horizontal, vertical).normalized;

            // 设置速度
            rb.velocity = inputDir * stats.MoveSpeed;

            // 限制在可移动区域内：物理引擎移动后可能越界，每帧钳制位置
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, -boundX, boundX);
            pos.y = Mathf.Clamp(pos.y, -boundY, boundY);
            transform.position = pos;
        }
    }
}
