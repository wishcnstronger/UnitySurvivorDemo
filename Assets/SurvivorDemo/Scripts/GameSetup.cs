using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// Phase1 临时场景搭建脚本。
    /// 运行时自动创建 Player、地面、设置摄像机。
    /// 后续阶段替换为预制体后删除此脚本。
    /// </summary>
    public class GameSetup : MonoBehaviour
    {
        [Header("地面")]
        [SerializeField, Tooltip("地面颜色")]
        private Color groundColor = new Color(0.15f, 0.15f, 0.15f);

        [SerializeField, Tooltip("地面大小")]
        private Vector2 groundSize = new Vector2(40f, 40f);

        [Header("玩家")]
        [SerializeField, Tooltip("玩家颜色")]
        private Color playerColor = Color.cyan;

        [SerializeField, Tooltip("玩家半径")]
        private float playerRadius = 0.5f;

        [Header("摄像机")]
        [SerializeField, Tooltip("摄像机背景色")]
        private Color cameraBackground = new Color(0.1f, 0.1f, 0.12f);

        [SerializeField, Tooltip("摄像机正交大小")]
        private float cameraSize = 8f;

        private Camera mainCam;

        private void Awake()
        {
            CreateGround();
            CreatePlayer();
            CreateTestEnemy(); // Phase2 测试用，生成一个敌人
            SetupCamera();
        }

        /// <summary>生成一个测试敌人，验证 Enemy 系统正常工作</summary>
        private void CreateTestEnemy()
        {
            GameObject enemy = new GameObject("Enemy_Test");

            // SpriteRenderer —— 红色圆形
            SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(Color.red, 32);
            sr.sortingOrder = 1;

            // Rigidbody2D
            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // CircleCollider2D
            CircleCollider2D col = enemy.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;

            // EnemyMovement
            EnemyMovement movement = enemy.AddComponent<EnemyMovement>();
            movement.moveSpeed = 2f;

            // 生成在玩家右上方
            enemy.transform.position = new Vector3(5f, 3f, 0f);
        }

        private void CreateGround()
        {
            GameObject ground = new GameObject("Ground");
            SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
            sr.sprite = CreateSquareSprite(groundColor);
            sr.drawMode = SpriteDrawMode.Simple;
            ground.transform.localScale = new Vector3(groundSize.x, groundSize.y, 1f);
            ground.transform.position = Vector3.zero;
            // Layer 设置等 TagManager 正确加载后再用，Phase1 不需要碰撞层
        }

        private void CreatePlayer()
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player"; // Enemy 通过 Tag 查找 Player

            // 碰撞体
            CircleCollider2D col = player.AddComponent<CircleCollider2D>();
            col.radius = playerRadius;
            col.isTrigger = true;

            // 刚体
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // 渲染
            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sprite = CreateCircleSprite(playerColor, 32);
            sr.sortingOrder = 1;

            // 组件
            player.AddComponent<PlayerStats>();
            player.AddComponent<PlayerMovement>();

            // 初始位置
            player.transform.position = Vector3.zero;
        }

        private void SetupCamera()
        {
            mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camObj = new GameObject("MainCamera");
                mainCam = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
                camObj.tag = "MainCamera";
            }

            mainCam.orthographic = true;
            mainCam.orthographicSize = cameraSize;
            mainCam.backgroundColor = cameraBackground;
            mainCam.transform.position = new Vector3(0f, 0f, -10f);
        }

        private void LateUpdate()
        {
            // 摄像机硬跟随 Player
            if (mainCam != null)
            {
                GameObject player = GameObject.Find("Player");
                if (player != null)
                {
                    Vector3 pos = player.transform.position;
                    mainCam.transform.position = new Vector3(pos.x, pos.y, -10f);
                }
            }
        }

        /// <summary>创建纯色正方形 Sprite</summary>
        private Sprite CreateSquareSprite(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        /// <summary>创建纯色圆形 Sprite（程序化生成）</summary>
        private Sprite CreateCircleSprite(Color color, int resolution)
        {
            Texture2D tex = new Texture2D(resolution, resolution);
            float center = resolution / 2f;
            float radius = center - 1f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    tex.SetPixel(x, y, dist <= radius ? color : Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
        }
    }
}
