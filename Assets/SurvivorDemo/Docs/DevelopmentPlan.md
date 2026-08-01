# SurvivorDemo 开发计划（精简版）

> 目标：**2周** 完成可运行 Demo
> 原则：**简单 > 完美，直接引用 > 事件总线，能跑 > 架构**

---

## 架构总览

### 不做的事

| 砍掉 | 原因 | 替代方案 |
|------|------|---------|
| EventBus | 增加调试难度，2周用不到 | 直接 `GetComponent` / `FindObjectOfType` |
| 抽象基类 WeaponBase | 2-3把武器不需要继承链 | 一个 Weapon 类，用 WeaponData 配不同行为 |
| 对象池 | Instantiate/Destroy 够用 | 后期卡了再加 |
| Boot 场景 | 多一个场景多一份维护 | MainMenu 直接启动 |
| GameManager 单例 | 杀鸡用牛刀 | 每个场景一个入口脚本 |
| AudioManager | 优先游戏性 | 武器/受伤直接 PlayOneShot |
| 多个敌人类型 | 2种够展示差异 | 蝙蝠(快)+骷髅(肉) |

### 最终脚本数量：10 个

```
Scripts/
├── GameplayManager.cs         — 游戏入口，控制 GameOver 流程
├── Player/
│   ├── PlayerMovement.cs      — WASD 移动
│   ├── PlayerStats.cs         — HP、XP、等级、速度、护甲（都是用 [SerializeField] 调）
│   └── PlayerWeapon.cs        — 自动索敌 + 按冷却发射子弹
├── Enemy/
│   ├── Enemy.cs               — 移动追玩家 + 扣血 + 死亡 + 掉落 XP
│   └── EnemySpawner.cs        — 定时生成 + 随时间变难
├── Weapon/
│   └── Projectile.cs          — 直线飞行 + 命中造成伤害
├── Pickup/
│   └── XPGem.cs               — 向玩家飞行 + 触碰加经验
└── UI/
    ├── HUD.cs                 — 计时/HP条/经验条/等级
    ├── LevelUpUI.cs           — 升级弹3个选项
    └── GameOverUI.cs          — 结算 + 重开
```

### 只有 2 个 ScriptableObject（策划调数值用）

```
Data/
├── WeaponData.cs    — 伤害、攻速、子弹速度、子弹数量
└── EnemyData.cs     — 血量、速度、伤害、经验值
```

---

## Phase 1: 玩家移动 (Day 1-2)

### 目标
一个圆形角色在场景里用 WASD 自由移动，摄像机跟随。

### 步骤

1. **清理项目**
   - 删除 `TutorialInfo/`
   - 删除 `Readme.asset`
   - 删除 `UOSLauncherEncrypt/`（或保留，不影响）

2. **创建目录**
   ```
   Assets/SurvivorDemo/
   ├── Scenes/
   ├── Scripts/
   ├── Prefabs/
   ├── Config/
   │   └── Weapons/
   │   └── Enemies/
   ├── Art/Sprites/
   └── Audio/
   ```

3. **创建 Gameplay 场景**
   - 基于现有 SampleScene 修改
   - 保存为 `Scenes/Gameplay.unity`

4. **创建 PlayerMovement.cs** → `Scripts/Player/`
   ```csharp
   // 职责：读取 Input，设置 Rigidbody2D.velocity
   // 暴露 [SerializeField] float moveSpeed = 5f
   // FixedUpdate 中: rb.velocity = inputDir * moveSpeed
   ```

5. **创建 PlayerStats.cs** → `Scripts/Player/`
   ```csharp
   // 职责：玩家运行时数据容器
   // [SerializeField] float maxHP = 100
   // [SerializeField] float moveSpeed = 5
   // [SerializeField] float armor = 0
   // [SerializeField] float magnetRange = 5
   // currentHP, currentXP, level 运行时修改
   // TakeDamage(), AddXP(), HealToFull() 等方法
   ```

6. **创建 Player 预制体**
   - 空 GameObject 命名为 "Player"
   - 加 Rigidbody2D（Gravity Scale = 0）
   - 加 CircleCollider2D（IsTrigger = true）
   - 挂 PlayerMovement + PlayerStats
   - 加 SpriteRenderer（临时用 Unity 内置 Circle sprite 或创建纯色方块）
   - Layer = Player

7. **摄像机跟随**
   - 在 MainCamera 上挂一个简单脚本或直接在 Player 上引用
   - 最简单方案：把 Camera 拖成 Player 子物体（硬跟随）
   - 或用 LateUpdate 中 `transform.position = playerPos + offset`

8. **设置 Layers 和物理碰撞**
   - 创建 Layer: `Player`(6), `Enemy`(7), `PlayerProjectile`(8), `Pickup`(9)
   - Edit → Project Settings → Physics2D，关闭所有层间碰撞
   - 只开启需要的：Player↔Enemy, Player↔Pickup, Enemy↔PlayerProjectile

9. **临时地面**
   - 创建大平面 Sprite，颜色灰色，作为地面背景
   - 添加 BoxCollider2D 做边界（可选，先不加也行）

10. **更新 Build Settings** → 只包含 Gameplay 场景

### 验证标准
- [ ] 运行 Gameplay，看到角色和地面
- [ ] WASD 移动，角色移动流畅
- [ ] 摄像机跟随角色
- [ ] Inspector 里改 moveSpeed 立即生效

---

## Phase 2: 敌人系统 (Day 3-4)

### 目标
敌人从屏幕外生成，追踪玩家，碰到玩家造成伤害。

### 步骤

1. **创建 EnemyData.cs** → `Scripts/Data/`
   ```csharp
   [CreateAssetMenu(menuName = "Survivor/EnemyData")]
   public class EnemyData : ScriptableObject
   {
       public string enemyName;
       public float maxHP = 10;
       public float moveSpeed = 3;
       public float damage = 10;     // 每次接触造成的伤害
       public int xpValue = 5;       // 击杀后掉落经验
   }
   ```

2. **创建 EnemyData 实例**
   - `Config/Enemies/Bat.asset` → HP:5, Speed:5, Damage:8, XP:3
   - `Config/Enemies/Skeleton.asset` → HP:15, Speed:2, Damage:15, XP:8

3. **创建 Enemy.cs** → `Scripts/Enemy/`
   ```csharp
   // 一个脚本做三件事：移动 + 生命 + 死亡。够简单。
   // [SerializeField] EnemyData data → 从 Inspector 拖入
   // Start(): 初始化 currentHP
   // Update(): 朝玩家移动 transform.position = Vector2.MoveTowards(...)
   // OnTriggerEnter2D(Collider2D other):
   //   - 碰到 PlayerProjectile → TakeDamage
   //   - 碰到 Player → 每帧扣血（用 OnTriggerStay2D）
   // TakeDamage(float amount): 扣血 → 血归零 Die()
   // Die(): 生成 XP 宝石 → Destroy(gameObject)
   ```

4. **创建 EnemySpawner.cs** → `Scripts/Enemy/`
   ```csharp
   // 挂在场景空物体上
   // [SerializeField] EnemyData[] availableEnemies   — 可生成的敌人类型
   // [SerializeField] float spawnInterval = 2f       — 生成间隔
   // [SerializeField] int maxEnemies = 50            — 场上数量上限
   // [SerializeField] float spawnDistance = 15f      — 生成距离（玩家周围）
   // [SerializeField] float difficultyScaleTime = 30f — 每30秒变难一次
   //
   // Start(): 启动 InvokeRepeating("Spawn", 0, spawnInterval)
   // Spawn(): 在玩家周围圆形外生成 → Instantiate(enemyPrefab)
   // 难度缩放：每30秒 spawnInterval *= 0.9, 敌人属性 *= 1.2
   ```

5. **创建 Enemy 预制体**
   - GameObject → Rigidbody2D (Gravity=0) → CircleCollider2D (IsTrigger)
   - SpriteRenderer（红色圆形，区分于玩家）
   - 挂 Enemy.cs → 拖入 EnemyData
   - Layer = Enemy

6. **完善 PlayerStats.cs 受伤逻辑**
   ```csharp
   // 无敌帧：受伤后短暂无敌（防止每帧都扣血）
   // [SerializeField] float invincibleTime = 0.5f
   // bool isInvincible; Coroutine 控制
   // TakeDamage(float amount):
   //   如果无敌 → return
   //   currentHP -= amount * (1 - armor/100)
   //   启动无敌协程
   //   如果 currentHP <= 0 → Die()
   ```

### 验证标准
- [ ] 敌人按间隔从屏幕外生成
- [ ] 敌人自动追踪玩家
- [ ] 碰到玩家时玩家扣血（不是每帧连扣）
- [ ] 不生成超过 maxEnemies 个敌人
- [ ] 改 EnemyData 的 Speed/HP 秒生效

---

## Phase 3: 战斗 + XP (Day 5-7)

### 目标
玩家自动攻击，子弹打死敌人，打死掉经验宝石，宝石被玩家吸收。

### 步骤

1. **创建 WeaponData.cs** → `Scripts/Data/`
   ```csharp
   [CreateAssetMenu(menuName = "Survivor/WeaponData")]
   public class WeaponData : ScriptableObject
   {
       public string weaponName;
       public float damage = 10;
       public float cooldown = 0.5f;       // 攻击间隔(秒)
       public float projectileSpeed = 10f;
       public float projectileLifetime = 2f;
       public int projectileCount = 1;      // 每次发射几个
       public float spreadAngle = 0f;       // 散射角度
       public GameObject projectilePrefab;  // 子弹预制体
   }
   ```

2. **创建 WeaponData 实例**
   - `Config/Weapons/Knife.asset` → Damage:10, Cooldown:0.5, Speed:12, Count:1

3. **创建 PlayerWeapon.cs** → `Scripts/Player/`
   ```csharp
   // 挂在 Player 上
   // [SerializeField] WeaponData[] weapons;  — 当前持有的武器
   // [SerializeField] float searchRadius = 20f;
   //
   // Update():
   //   找到最近的 Enemy（Physics2D.OverlapCircle）
   //   如果没有敌人 → return
   //   遍历每个武器：
   //     检查 cooldownTimer >= weapon.cooldown
   //     满足条件 → 发射子弹 → cooldownTimer 归零
   //     不满足 → cooldownTimer += Time.deltaTime
   //
   // Fire(WeaponData weapon, Vector2 targetDir):
   //   计算发射方向 = (targetPos - transform.position).normalized
   //   如果有 spreadAngle → 均匀分角
   //   Instantiate(projectilePrefab) → 设置方向 + 伤害
   ```

4. **创建 Projectile.cs** → `Scripts/Weapon/`
   ```csharp
   // [SerializeField] float speed = 10f;
   // [SerializeField] float lifetime = 2f;
   // float damage;  — 发射时由 PlayerWeapon 设置
   //
   // Start(): Destroy(gameObject, lifetime)
   // Update(): transform.Translate(Vector2.right * speed * Time.deltaTime)
   //   （子弹预制体面朝右，发射时旋转到目标方向）
   //   或者更好的方案：Rigidbody2D.velocity = direction * speed
   //
   // OnTriggerEnter2D(Collider2D other):
   //   if (other.CompareTag("Enemy") || other.layer == Enemy)
   //     other.GetComponent<Enemy>().TakeDamage(damage)
   //     Destroy(gameObject)
   ```

5. **创建 Projectile 预制体**
   - GameObject → Rigidbody2D → CircleCollider2D (IsTrigger)
   - SpriteRenderer（白色小圆形）
   - 挂 Projectile.cs
   - Layer = PlayerProjectile

6. **创建 XPGem.cs** → `Scripts/Pickup/`
   ```csharp
   // [SerializeField] float magnetSpeed = 5f;
   // [SerializeField] float collectRadius = 0.3f;  — 碰到玩家判定距离
   // int xpAmount;  — 生成时由 Enemy 设置
   //
   // Update(): 
   //   如果玩家在 magnetRange 内 → 向玩家飞
   //   如果距离玩家 < collectRadius → 拾取
   //   
   // Collect():
   //   playerStats.AddXP(xpAmount)
   //   Destroy(gameObject)
   ```

7. **创建 XP 宝石预制体**
   - 小绿色菱形/圆形
   - CircleCollider2D (IsTrigger)
   - 挂 XPGem.cs
   - Layer = Pickup

8. **修改 Enemy.cs → 死亡掉落**
   ```csharp
   // Die():
   //   在死亡位置 Instantiate(xpGemPrefab)
   //   设置 xpGem.xpAmount = data.xpValue
   //   Destroy(gameObject)
   ```

9. **修改 PlayerStats.cs → 升级逻辑**
   ```csharp
   // AddXP(int amount):
   //   currentXP += amount
   //   检查 currentXP >= xpToNextLevel:
   //     LevelUp()
   //
   // LevelUp():
   //   level++
   //   currentXP -= xpToNextLevel
   //   xpToNextLevel = 计算下一级所需（公式: 10 + level * 5）
   //   HP回满
   //   弹出 LevelUpUI
   //   Time.timeScale = 0  ← 暂停游戏
   ```

### 验证标准
- [ ] 敌人靠近时角色自动射击
- [ ] 子弹命中敌人 → 敌人扣血 → HP 归零消失
- [ ] 敌人死亡生成绿色 XP 宝石
- [ ] 玩家靠近宝石 → 宝石飞向玩家 → 经验增加
- [ ] 经验满 → 升级提示出现（先用 Debug.Log 确认，UI 在 Phase 4 做）
- [ ] 改 WeaponData 伤害/攻速 → 立即生效

---

## Phase 4: UI + 升级 (Day 8-9)

### 目标
HUD 显示状态，升级时弹出选项面板，GameOver 时结算。

### 步骤

1. **创建 HUD.cs** → `Scripts/UI/`
   ```csharp
   // 挂在 Canvas 上
   // [SerializeField] TMP_Text timerText      — 左上角计时
   // [SerializeField] TMP_Text levelText       — 等级
   // [SerializeField] Slider expBar            — 经验条
   // [SerializeField] Slider hpBar             — HP条
   //
   // Update(): 
   //   从 PlayerStats 读取数据，刷新 UI
   //   timerText = FormatTime(Time.timeSinceLevelLoad)
   //   expBar.value = currentXP / xpToNextLevel
   //   hpBar.value = currentHP / maxHP
   ```
   > 注意：直接用 `FindObjectOfType<PlayerStats>()` 获取引用，简单直接。

2. **创建 LevelUpUI.cs** → `Scripts/UI/`
   ```csharp
   // [SerializeField] GameObject panel;           — 升级面板根节点
   // [SerializeField] Button[] optionButtons;     — 3个选项按钮
   // [SerializeField] TMP_Text[] optionTexts;     — 3个选项文字
   //
   // 升级选项类型（简单枚举）:
   //   AddWeapon,           — 获得一把新武器
   //   DamageUp,            — 所有武器伤害 +20%
   //   SpeedUp,             — 移速 +15%
   //   MaxHPUp,             — 最大HP +25%
   //   CooldownDown,        — 所有武器冷却 -15%
   //   MagnetUp,            — 磁铁范围 +30%
   //
   // Show():
   //   随机从池子里抽3个不重复的选项
   //   给每个按钮绑定事件
   //   panel.SetActive(true)
   //   Time.timeScale = 0
   //
   // OnOptionChosen(int type):
   //   应用效果，panel.SetActive(false)
   //   Time.timeScale = 1
   ```
   > 武器列表从 `PlayerWeapon` 的 `[SerializeField] WeaponData[] allPossibleWeapons` 中读取。

3. **创建 GameOverUI.cs** → `Scripts/UI/`
   ```csharp
   // [SerializeField] GameObject panel
   // [SerializeField] TMP_Text survivalTimeText
   // [SerializeField] TMP_Text enemiesKilledText
   // [SerializeField] TMP_Text finalLevelText
   // [SerializeField] Button restartButton
   //
   // Show(float time, int kills, int level):
   //   panel.SetActive(true)
   //   设置各文本
   //   Time.timeScale = 0
   //
   // restartButton.onClick → SceneManager.LoadScene("Gameplay")
   ```

4. **创建 GameplayManager.cs** → `Scripts/`
   ```csharp
   // 挂在场景空物体 "GameManager" 上
   // 引用 PlayerStats, HUD, LevelUpUI, GameOverUI（Inspector拖入）
   //
   // 监听 PlayerStats.currentHP:
   //   当 HP <= 0 → 调用 GameOverUI.Show()
   //
   // 最简单方案：在 Update 里检查
   //   if (playerStats.currentHP <= 0 && !isGameOver)
   //     GameOver()
   ```

5. **创建 MainMenu 场景**
   - 简单 Canvas + 标题 Text + 开始按钮
   - 开始按钮 → `SceneManager.LoadScene("Gameplay")`

6. **搭建 Canvas（Gameplay 场景）**
   - UI → Canvas（Screen Space Overlay）
   - 创建 HUD 元素（左上计时+等级+XP条，左下HP条）
   - 创建 LevelUpUI 面板（初始隐藏）
   - 创建 GameOverUI 面板（初始隐藏）

### 验证标准
- [ ] HUD 实时显示：计时器、HP条、XP条、等级
- [ ] 升级时弹出3个选项，点击后生效（伤害/速度/HP真的有变化）
- [ ] 选择"获得新武器"后，角色多一把武器同时射击
- [ ] HP归零 → 弹出GameOver面板
- [ ] 点击重新开始 → 完整重开
- [ ] 主菜单 → 开始游戏 → 游戏内 → 死亡 → 重开，全流程通

---

## Phase 5: 打磨 + 发布 (Day 10)

### 目标
补齐细节，确保能 Build 出可玩的版本。

### 步骤

1. **加 2-3 把新武器**
   - 在 Project 右键 Create → WeaponData
   - `Config/Weapons/Whip.asset` → 大范围慢攻速
   - `Config/Weapons/FireWand.asset` → 3发散射
   - 拖入 Player 的 allPossibleWeapons 列表

2. **加 2-3 种新敌人**
   - `Config/Enemies/Wolf.asset` → 高速低血
   - `Config/Enemies/Ghost.asset` → 中速中血
   - 拖入 EnemySpawner 的 availableEnemies 列表

3. **简单音效**
   - 网上找免费音效（或 Project 内临时生成）
   - 在 Projectile 的 Start() 里 `GetComponent<AudioSource>().PlayOneShot(shootClip)`
   - 在 PlayerStats.TakeDamage() 里播放受伤音
   - 在 Enemy.Die() 里播放死亡音
   - 在 XPGem.Collect() 里播放拾取音

4. **校准数值**
   - 确保前30秒能活
   - 确保2分钟之后压力逐渐增大
   - 确保升级感觉有收益

5. **Build 测试**
   - File → Build Settings → Build
   - 确保独立 exe 能正常运行
   - 分辨率适配（1024×768 → 1920×1080）

### 验证标准
- [ ] 有多把武器可选
- [ ] 有多种敌人
- [ ] 有基本音效反馈
- [ ] 能 Build exe 给他人试玩
- [ ] 2分钟、5分钟存活体感不同（难度递增）

---

## 时间预算

```
Day  1-2 : Player 移动 + 场景搭建 + 碰撞层级
Day  3-4 : Enemy + EnemySpawner + 接触伤害 + 无敌帧
Day  5-7 : PlayerWeapon + Projectile + Enemy死亡 + XP宝石 + 拾取
Day  8-9 : HUD + LevelUpUI + GameOverUI + MainMenu + 全流程打通
Day   10 : 多加武器/敌人 + 简单音效 + 数值校准 + Build
```

---

## 通信模式：直接用引用，不用事件

```csharp
// 想拿 PlayerStats？直接找。
PlayerStats stats = FindObjectOfType<PlayerStats>();

// 或者 Inspector 拖入（更推荐）
[SerializeField] PlayerStats playerStats;

// Enemy 碰到 Player？
void OnTriggerStay2D(Collider2D other) {
    if (other.CompareTag("Player"))
        other.GetComponent<PlayerStats>().TakeDamage(damage);
}

// Player 死了要弹 GameOver？
// GameplayManager.Update() 里检查
if (playerStats.currentHP <= 0) gameOverUI.Show();
```

**不用事件总线。不用单例。不用 ScriptableObject 事件。就拖引用 + FindObjectOfType。**

---

## 碰撞矩阵

```
PlayerProjectile → Enemy   : 子弹命中敌人
Player → Enemy             : 敌人撞到玩家（用 physics settings 关闭/开启）
Player → Pickup            : 玩家拾取宝石
```

Physics2D Settings 中：
- 只勾选 `Enemy ↔ PlayerProjectile` 产生 trigger 回调
- `Player ↔ Pickup` 产生 trigger 回调
- Player ↔ Enemy 通过代码层控制（也可以勾选，OnTriggerStay 检测）
- 其他全部关闭

---

## 数值速查（策划调数值入口）

| 参数 | 默认值 | 在哪里调 |
|------|--------|---------|
| 玩家速度 | 5 | PlayerStats.moveSpeed |
| 玩家HP | 100 | PlayerStats.maxHP |
| 玩家护甲 | 0 | PlayerStats.armor |
| 无敌时间 | 0.5s | PlayerStats.invincibleTime |
| 磁铁范围 | 5 | PlayerStats.magnetRange |
| 经验升级 | 10 + lv×5 | PlayerStats 公式 |
| 蝙蝠HP | 5 | EnemyData(Bat).maxHP |
| 蝙蝠速度 | 5 | EnemyData(Bat).moveSpeed |
| 骷髅HP | 15 | EnemyData(Skeleton).maxHP |
| 骷髅速度 | 2 | EnemyData(Skeleton).moveSpeed |
| 飞刀伤害 | 10 | WeaponData(Knife).damage |
| 飞刀攻速 | 0.5s | WeaponData(Knife).cooldown |
| 飞刀射速 | 12 | WeaponData(Knife).projectileSpeed |
| 生成间隔 | 2s | EnemySpawner.spawnInterval |
| 场上敌人上限 | 50 | EnemySpawner.maxEnemies |
| 难度提升间隔 | 30s | EnemySpawner.difficultyScaleTime |
| 索敌范围 | 20 | PlayerWeapon.searchRadius |
