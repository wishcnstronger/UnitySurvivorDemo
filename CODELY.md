

## Codely Structured Memories

### User

### Feedback

### Project
- [2026-08-16 20:53:32] 角色/怪物/首领的精灵图与碰撞箱已统一调整（2026-08-13，2026-08-16 更新放大）。精灵图视觉大小通过修改 pixelsPerUnit 实现。2026-08-16 放大：playerScale 1.0→1.5（GameSetup.cs 默认值）；普通敌人（圆形/三角/方块）预制体 localScale 设为 1.5×1.5；首领预制体 localScale 设为 1.3×1.3。血条 offsetY 已随 scale 同步上调（普通怪 0.6→0.9，方块怪 0.7→1.05，首领 1.8→2.34）。GameSetup 场景覆写参数以代码默认值为准。**Why:** 精灵图视觉太小，放大后更易辨认。**How to apply:** 改实体大小调预制体 localScale（不再用 PPU 调大小，scale=1.0 旧方案已废弃）；血条 offsetY 需随 scale 等比上调。 - [2026-08-12 23:28:00] 全部 8 个 sprite 正在重新生成（第三批），目标统一像素地牢风格。弹道/物品 3 个用 huoshan_seedream+pixel_16bit 成功生成像素风：PlayerBulletSprite（蓝能量球+拖尾）、EnemyBulletSprite（红能量球+螺旋）、XPOrbSprite（绿宝石），路径在 TJGenerators/History/ 待复制。角色/怪物 5 个用 huoshan_seedream 未生成像素风（矢量/3D/扁平），改用 frontier-game-design 重新生成中（sprite_9~13）。**Why:** huoshan_seedream 的 pixel_16bit style_id 对角色/怪物类不生效，frontier-game-design 对像素风格 prompt 响应更好。**How to apply:** 生成像素角色 sprite 时优先用 frontier-game-design 并在 prompt 开头强加 "16-bit pixel art sprite, retro SNES style, pixelated, no anti-aliasing, transparent background"；小物品/弹道用 huoshan_seedream 即可。





- [2026-08-16 20:32:53] UI 系统按 UI_OPTIMIZATION_PROMPTS.md 全部优化完成。已完成：① 像素字体 ZPix（Resources/Fonts/ZPix.ttf）；② 配色提亮（UIDungeonTheme 色板更新）；③ 11 张升级图标+骷髅/心形/沙漏图标（Resources/UI/）；④ UIArtCache.cs 改用 PanelType 枚举+GetPanelBg()+ButtonBg；⑤ 4 张 AI 背景图（UpgradePanelBg/StartScreenPanelBg/GameOverPanelBg/ButtonBg），均 Sprite+Point filter+白底去透明；⑥ StartScreenUI/GameOverUI/UpgradeUI 均改为单 Image 加载 AI 背景图（Sliced），删除旧 frame+panel 双层结构；⑦ GameOverUI 删除 frame 外框，按钮改用 AI ButtonBg；⑧ PlayerHUD 改为 VS 风格：顶部居中时间+顶部全宽经验条(蓝→满级金)+经验条左侧等级文字(去徽章)+右上角击杀计数(骷髅+数字)+角色头顶世界空间 HP 条(WorldSpace Canvas, 延迟白条+HSV变色)。**Why:** MD 第四章重写为 AI 完整背景图方案替代程序化边框。**How to apply:** 面板用 UIArtCache.GetPanelBg(PanelType.X) 加载，按钮用 UIArtCache.ButtonBg；AI 生成图白底问题用脚本去除（亮度>0.85且饱和度<0.15→透明）；改图后需重新设 Sprite 类型。
- [2026-08-16 20:53:37] [2026-08-16 20:53:00] 玩家头顶 HP 条+首领火焰特效+开始界面改动（2026-08-16）。① PlayerHUD 世界空间 HP 条 bug 修复：localScale 从 0.01 改为 Vector3.one，HP 条现在以 1.5 世界单位宽度正确显示在玩家头顶（之前因 scale 过小完全不可见）；② 首领矩形技能添加火焰特效：从特效库下载 ExplosionDecalFire（Combat/Decals），BossMonster.cs 新增 fireVfxPrefab 字段，矩形技能激活阶段生成火焰粒子，按矩形区域大小缩放，1.5 秒销毁；③ 开始界面标题改为"地牢幸存者 Demo"；④ Start 按钮悬停效果增强：AddHoverColor 方法在悬停时给 Image 加暖色 tint(1.2,1.0,0.7)，离开恢复白色，AddHoverScale 放大倍数 1.06→1.08。**Why:** 用户反馈玩家血量未显示、实体太小、首领技能无特效、标题名称不对。**How to apply:** WorldSpace Canvas 的 localScale 必须为 Vector3.one 而非 0.01（0.01 会让 UI 缩到不可见）；特效库下载的预制体用 SerializedObject 分配到预制体字段（编译后才能用 C# 脚本访问新字段）。
- [2026-08-16 21:20:02] 怪物生成与数值系统已按 VS 风格改造（2026-08-16）。EnemySpawner：spawnInterval 1.2s/minSpawnInterval 0.15s/递减0.02/s；每波 2+floor(t/15) max12；权重动态调整（圆形75%→55%，三角12%→25%，方块13%→20%，随时间递增远程/冲锋比例）；HP 指数缩放 1.15^min，伤害线性 1+min×0.3，速度缓增 min(2.5,1+min×0.08)；maxEnemies=150 + 静态计数器 ActiveEnemyCount。EnemyMovement 新增 ScaleSpeed()。EnemyHealth.Die() 递减计数器。GameSetup.Awake/ResetGame 重置计数器。预制体基础数值：圆形 HP2/速度2.5/伤害6/XP3，三角 HP3/速度2.2/伤害5/子弹4/间隔2.5s/XP8，方块 HP5/速度1.8/伤害8/冲锋15/冷却5s/XP10。**Why:** 之前怪物生成太慢太少、远程怪太多、后期速度不提升导致缺乏 VS 式蜂拥压力。**How to apply:** 改怪物数值调预制体字段；改缩放公式改 EnemySpawner.SpawnEnemy()；新增怪物类型时在 GetSpawnWeights() 和 enemyPrefabs 列表中同步添加。

### Reference

