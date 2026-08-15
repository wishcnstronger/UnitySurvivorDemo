using UnityEngine;
using UnityEngine.UI;

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
        private Color groundColor = new Color(0.1f, 0.1f, 0.1f);

        [SerializeField, Tooltip("地面大小（底层画布，仅比可移动区域大一圈边框）")]
        private Vector2 groundSize = new Vector2(38f, 38f);

        [Header("可移动区域")]
        [SerializeField, Tooltip("可移动区域大小（占据画面大部分）")]
        private Vector2 playAreaSize = new Vector2(36f, 36f);

        [SerializeField, Tooltip("可移动区域颜色")]
        private Color playAreaColor = new Color(0.22f, 0.22f, 0.22f);

        [SerializeField, Tooltip("边界框颜色")]
        private Color borderColor = new Color(0.4f, 0.4f, 0.4f);

        [SerializeField, Tooltip("边界框厚度")]
        private float borderThickness = 0.3f;

        [Header("玩家")]
        [SerializeField, Tooltip("玩家颜色")]
        private Color playerColor = Color.cyan;

        [SerializeField, Tooltip("玩家半径")]
        private float playerRadius = 0.6f;

        [SerializeField, Tooltip("玩家整体缩放（视觉与碰撞一起等比放大）")]
        private float playerScale = 1.0f;

        [SerializeField, Tooltip("玩家子弹整体缩放（试玩反馈：1.5 过大，回调到 1.2）")]
        private float bulletScale = 1.2f;

        [Header("摄像机")]
        [SerializeField, Tooltip("摄像机背景色")]
        private Color cameraBackground = new Color(0.2f, 0.2f, 0.2f);

        [SerializeField, Tooltip("摄像机正交大小")]
        private float cameraSize = 8f;

        private Camera mainCam;

        /// <summary>玩家引用（本脚本自己创建，直接缓存，避免每帧按名字查找）</summary>
        private Transform playerTransform;

        /// <summary>子弹模板缓存（独立根物体，重开时复用，不重复创建）</summary>
        private GameObject bulletTemplate;

        /// <summary>开始界面（首次启动显示，重开不再展示，重开时销毁）</summary>
        private StartScreenUI startScreen;

        private void Awake()
        {
            // 停在开始界面：时间流速为 0，等玩家点 Start 才正式开始。
            // 开始界面期间 deltaTime≈0，敌人不移动/不生成、计时不走、玩家不能动。
            Time.timeScale = 0f;

            // 复位全局状态（static 变量在重开/重进播放模式下不会自动清空，必须显式归零）
            GameStats.kills = 0;
            GameStats.playTime = 0f;

            CreateGround();
            CreatePlayArea();
            SetupEnemySpawnerBounds();
            GameObject player = CreatePlayer();
            SetupCamera();
            SetupCombatSystems();

            // 开始界面：挂独立 GameObject（不挂 Player），sortingOrder=120 最高层
            GameObject startScreenObj = new GameObject("StartScreenUI");
            startScreen = startScreenObj.AddComponent<StartScreenUI>();

            // 注入初始构筑选择：点 Start 后弹出三选一（选完才开始游戏）
            startScreen.levelUp = player.GetComponent<LevelUpManager>();
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

        /// <summary>
        /// 创建可移动区域：边界框 + 可移动区域底色。
        /// 边界框比可移动区域大一圈，可移动区域叠在上方，形成视觉边界。
        /// </summary>
        private void CreatePlayArea()
        {
            // 边界框（略大于可移动区域，形成可见边框）
            GameObject border = new GameObject("PlayAreaBorder");
            SpriteRenderer borderSr = border.AddComponent<SpriteRenderer>();
            borderSr.sprite = CreateSquareSprite(borderColor);
            borderSr.sortingOrder = 0;
            float borderSize = playAreaSize.x + borderThickness * 2f;
            border.transform.localScale = new Vector3(borderSize, playAreaSize.y + borderThickness * 2f, 1f);
            border.transform.position = new Vector3(0f, 0f, 0.5f);

            // 可移动区域底色（叠在边界框上方，比地面浅，与边界框形成层次）
            GameObject playArea = new GameObject("PlayArea");
            SpriteRenderer playSr = playArea.AddComponent<SpriteRenderer>();
            playSr.sprite = CreateSquareSprite(playAreaColor);
            playSr.sortingOrder = 1;
            playArea.transform.localScale = new Vector3(playAreaSize.x, playAreaSize.y, 1f);
            playArea.transform.position = new Vector3(0f, 0f, 0.4f);
        }

        /// <summary>将可移动区域边界注入场景中的 EnemySpawner，使怪物生成区域与玩家移动区域一致</summary>
        private void SetupEnemySpawnerBounds()
        {
            EnemySpawner spawner = FindObjectOfType<EnemySpawner>();
            if (spawner != null)
            {
                spawner.boundX = playAreaSize.x / 2f;
                spawner.boundY = playAreaSize.y / 2f;
            }
        }

        /// <summary>创建玩家并返回其 GameObject（Awake 用返回值注入初始构筑选择）</summary>
        private GameObject CreatePlayer()
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
            Sprite heroSprite = Resources.Load<Sprite>("Sprites/SurvivorHero");
            if (heroSprite != null)
            {
                sr.sprite = heroSprite;
                sr.color = Color.white;
            }
            else
            {
                sr.sprite = CreateCircleSprite(playerColor, 32);
            }
            sr.sortingOrder = 1;

            // 组件
            player.AddComponent<PlayerStats>();

            // 移动：设置可移动区域边界
            PlayerMovement movement = player.AddComponent<PlayerMovement>();
            movement.boundX = playAreaSize.x / 2f;
            movement.boundY = playAreaSize.y / 2f;

            // 武器组件，并自动创建子弹模板给它用
            PlayerWeapon weapon = player.AddComponent<PlayerWeapon>();
            weapon.bulletPrefab = CreateBulletTemplate();

            // 升级系统：界面 + 流程管理（先加界面，LevelUpManager 会自动找到它）
            UpgradeUI upgradeUI = player.AddComponent<UpgradeUI>();
            LevelUpManager levelUp = player.AddComponent<LevelUpManager>();
            levelUp.upgradeUI = upgradeUI;

            // 生命系统：受伤 / 无敌 / 常驻 HUD / 游戏结束结算
            player.AddComponent<PlayerHealth>();
            player.AddComponent<PlayerHUD>();

            // 游玩计时：每帧累加 GameStats.playTime（HUD 右上角时间与结算存活时间用）
            player.AddComponent<GameTimer>();

            GameOverUI gameOverUI = player.AddComponent<GameOverUI>();
            gameOverUI.gameSetup = this; // 注入重开回调（点击按钮时调用 ResetGame）

            // 初始位置
            player.transform.position = Vector3.zero;

            // 整体缩放：视觉与碰撞体一起等比放大
            player.transform.localScale = new Vector3(playerScale, playerScale, 1f);

            // 缓存玩家引用，供摄像机跟随使用
            playerTransform = player.transform;

            return player;
        }

        /// <summary>
        /// 创建一颗隐藏的子弹模板（黄色圆形），返回引用。
        /// PlayerWeapon 生成子弹时以它为模板克隆。
        /// </summary>
        private GameObject CreateBulletTemplate()
        {
            // 已有模板则复用：它是独立根物体，重开销毁玩家不会带走它
            if (bulletTemplate != null)
                return bulletTemplate;

            GameObject bullet = new GameObject("BulletTemplate");
            bullet.SetActive(false); // 隐藏，只在被发射时激活

            // 整体缩放：子弹视觉与碰撞体一起等比放大
            bullet.transform.localScale = new Vector3(bulletScale, bulletScale, 1f);

            // 渲染：加载玩家子弹 sprite，回退到黄色圆形
            SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
            Sprite bulletSprite = Resources.Load<Sprite>("Sprites/PlayerBulletSprite");
            if (bulletSprite != null)
            {
                sr.sprite = bulletSprite;
                sr.color = Color.white;
            }
            else
            {
                sr.sprite = CreateCircleSprite(Color.yellow, 16);
            }
            sr.sortingOrder = 2;

            // 子弹飞行组件
            bullet.AddComponent<Bullet>();

            bulletTemplate = bullet;
            return bullet;
        }

        /// <summary>
        /// 重新开始一局。
        /// 场景是运行时全程序化搭建的，不能用 SceneManager.LoadScene 重载，
        /// 所以手动清空场上所有物体并重建玩家（Ground 保留）。
        /// </summary>
        public void ResetGame()
        {
            // 1. 复位全局状态：
            //    timeScale 不复位 → 新局永远静止；static 击杀数/游玩时间不会自动清空 → 必须显式归零
            Time.timeScale = 1f;
            GameStats.kills = 0;
            GameStats.playTime = 0f;

            // 2. 销毁开始界面（只在首次启动出现过，重开直接开打，不再展示）
            if (startScreen != null)
            {
                startScreen.gameObject.SetActive(false);
                Destroy(startScreen.gameObject);
                startScreen = null;
            }

            // 3. 销毁旧玩家：先停用再销毁。
            //    Destroy 延迟到帧末生效，停用后旧玩家的 Update 不再运行，
            //    防止它在本帧剩余时间继续发射残留子弹污染新局
            GameObject oldPlayer = GameObject.Find("Player");
            if (oldPlayer != null)
            {
                oldPlayer.SetActive(false);
                Destroy(oldPlayer);
            }

            // 4. 一次性销毁所有敌人 / 玩家子弹 / 敌人子弹 / 经验宝石
            //    （只在重开时调用一次，不是每帧路径，可用 FindObjectsOfType）
            foreach (EnemyMovement enemy in FindObjectsOfType<EnemyMovement>())
                Destroy(enemy.gameObject);

            foreach (Bullet bullet in FindObjectsOfType<Bullet>())
                Destroy(bullet.gameObject);

            // 敌人子弹（三角怪发射）也是场上投掷物，重开必须一起清掉，否则会打到新玩家
            foreach (EnemyBullet bullet in FindObjectsOfType<EnemyBullet>())
                Destroy(bullet.gameObject);

            foreach (XPOrb orb in FindObjectsOfType<XPOrb>())
                Destroy(orb.gameObject);

            // 首领矩形伤害区域（boss 死亡后可能残留）
            foreach (SpriteRenderer sr in FindObjectsOfType<SpriteRenderer>())
                if (sr.gameObject.name == "BossRectZone")
                    Destroy(sr.gameObject);

            // 5. 销毁所有运行时创建的 UI Canvas（EventSystem 不是 Canvas，自动保留复用）
            foreach (Canvas canvas in FindObjectsOfType<Canvas>())
                Destroy(canvas.gameObject);

            // 6. 重建玩家与摄像机（地面保留），playerTransform 缓存更新为新玩家
            CreatePlayer();
            SetupCamera();
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

            // 战斗手感：顿帧控制器
            if (mainCam.GetComponent<HitStopController>() == null)
                mainCam.gameObject.AddComponent<HitStopController>();
        }

        /// <summary>创建音效与特效管理单例（仅首次）</summary>
        private void SetupCombatSystems()
        {
            if (AudioManager.Instance == null)
            {
                GameObject audioObj = new GameObject("AudioManager");
                audioObj.AddComponent<AudioManager>();
            }

            if (CombatVFX.Instance == null)
            {
                GameObject vfxObj = new GameObject("CombatVFX");
                vfxObj.AddComponent<CombatVFX>();
            }
        }

        private void LateUpdate()
        {
            // 摄像机硬跟随 Player（直接使用缓存的引用，不再每帧按名字查找）
            if (mainCam == null)
                return;

            // 引用失效（重开旧玩家销毁）时重新查找一次，防止摄像机永久罢工
            if (playerTransform == null)
            {
                GameObject player = GameObject.Find("Player");
                if (player != null)
                    playerTransform = player.transform;
            }

            if (playerTransform != null)
            {
                Vector3 pos = playerTransform.position;
                mainCam.transform.position = new Vector3(pos.x, pos.y, -10f);
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
