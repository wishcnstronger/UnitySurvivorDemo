

## Codely Structured Memories

### User

### Feedback

### Project
- [2026-08-13 19:20:33] 角色/怪物/首领的精灵图与碰撞箱已统一调整（2026-08-13）。精灵图视觉大小通过修改 pixelsPerUnit 实现，预制体 scale=1.0。GameSetup 场景覆写：playerScale=1.0（代码默认 1.0），playerRadius=0.6，groundColor/playAreaColor/borderColor 的 alpha=0（透明，让 DungeonMap 背景显示），groundSize=52×52，playAreaSize=44×44。各实体参数：玩家 sprite PPU=683→视觉1.5单位，碰撞半径0.6（有效0.6，为视觉80%）；普通敌人 PPU=1024→1.0，CircleCollider半径0.4；方块怪 PPU=853→1.2，BoxCollider size 1.0×1.0；首领 PPU=341→3.0，CircleCollider半径1.2。血条：普通怪 barWidth=0.8/barOffsetY=0.6/barHeight=0.12，方块怪 barWidth=1.0/barOffsetY=0.7，首领 barWidth=2.5/barOffsetY=1.8/barHeight=0.2。DungeonMap Quad：position z=1, scale [50,50,1], sortingOrder=-1。**Why:** 之前所有精灵图仅0.5世界单位但碰撞箱远大于视觉（玩家碰撞半径2.1 vs 视觉0.75），导致未接触就受伤。**How to apply:** 碰撞箱均设为视觉半径的80%以兼顾公平与手感；改精灵图大小调 PPU，碰撞箱与血条会因 scale=1.0 不受影响。 - [2026-08-12 23:28:00] 全部 8 个 sprite 正在重新生成（第三批），目标统一像素地牢风格。弹道/物品 3 个用 huoshan_seedream+pixel_16bit 成功生成像素风：PlayerBulletSprite（蓝能量球+拖尾）、EnemyBulletSprite（红能量球+螺旋）、XPOrbSprite（绿宝石），路径在 TJGenerators/History/ 待复制。角色/怪物 5 个用 huoshan_seedream 未生成像素风（矢量/3D/扁平），改用 frontier-game-design 重新生成中（sprite_9~13）。**Why:** huoshan_seedream 的 pixel_16bit style_id 对角色/怪物类不生效，frontier-game-design 对像素风格 prompt 响应更好。**How to apply:** 生成像素角色 sprite 时优先用 frontier-game-design 并在 prompt 开头强加 "16-bit pixel art sprite, retro SNES style, pixelated, no anti-aliasing, transparent background"；小物品/弹道用 huoshan_seedream 即可。




- [2026-08-12 20:23:34] UI 系统全部程序化生成（无预制体），位于 Assets/SurvivorDemo/Scripts/UI/。新增 UIDungeonTheme.cs 提供统一地牢色板（GoldBorder/GoldText/StoneBorder/DungeonBlue/WarmWhite 等）和工具方法（CreateBorderSprite/CreateRoundedSprite/StyleButton/AddHoverScale/AddOutline），各 UI 脚本均引用。**Why:** 之前开始界面纯白按钮、升级卡片纯色黑字，风格不统一。**How to apply:** 修改 UI 配色时通过 UIDungeonTheme 常量取色，不要硬编码 new Color；新增 UI 元素复用 CreateBorderSprite 做边框、AddHoverScale 做悬停效果。

### Reference

