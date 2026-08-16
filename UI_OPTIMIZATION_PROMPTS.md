# UI 优化规范 Prompt 文档

> 目标：将程序化生成的 UI 提升到贴近 Vampire Survivors 成品质感
> 方案：混合（关键视觉元素 AI 生成 + 基础面板程序化保留）
> 优先级：升级界面 > HUD > 开始/结束界面
> 主题：保留地牢金/石色调，提高对比度和饱和度

---

## 一、像素字体方案

### 1.1 字体选型要求

```
- 风格：16-bit 像素风，retro SNES/GBA 风格
- 必须支持中文（至少 GB2312 常用 3500 字）
- 字符集覆盖：数字、大小写英文、中文常用字、标点符号
- 授权：免费可商用（SIL OFL 或 CC0）
- 格式：TTF 或 OTF
```

### 1.2 推荐字体

| 字体名 | 说明 | 授权 |
|--------|------|------|
| ZPix | 最像素(https://github.com/SolidZORO/zpix-pixel-font) 完整中文支持 | SIL OFL |
| FangZheng | 方正像素 12px | 个人免费 |
| CUBE TYPE | 日系像素风，中文覆盖不全 | 商用需授权 |

### 1.3 Unity 导入配置

```
1. 将 TTF 放入 Assets/SurvivorDemo/Fonts/
2. Import Settings:
   - Font Size: 16 (基础像素尺寸)
   - Character: Unicode
   - Generation Mode: Default
3. UIFont.cs 修改 Get() 方法：
   cachedFont = Resources.Load<Font>("Fonts/ZPix");
   替代当前的 Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 36)
4. 各 UI 脚本中 fontSize 需要按像素字体重新调参
   - 标题: 32-40 (像素字体视觉偏大)
   - 正文: 16-20
   - 数值: 14-18
```

---

## 二、升级界面 — AI 美术资源生成 Prompt

### 2.1 升级图标（11 种类型）

**统一风格前缀（所有图标共用）：**

```
16-bit pixel art icon, retro SNES style, pixelated, no anti-aliasing,
transparent background, 64x64 canvas, single centered subject,
dark dungeon RPG theme, limited color palette (max 8 colors per icon),
crisp pixel edges, game asset sprite
```

**逐个图标 Prompt：**

| # | 类型 | 英文 | Prompt |
|---|------|------|--------|
| 1 | 攻速提升 | FireRate | `16-bit pixel art icon, retro SNES style, pixelated, no anti-aliasing, transparent background, 64x64 canvas, a flaming sword with motion lines indicating speed, orange-red flames, motion blur streaks, dark dungeon RPG theme, limited 8-color palette, crisp pixel edges, game asset sprite` |
| 2 | 子弹数量 | BulletCount | `16-bit pixel art icon, retro SNES style, pixelated, no anti-aliasing, transparent background, 64x64 canvas, three golden bullets stacked fan-shape, metallic shine highlights, dark dungeon RPG theme, limited 8-color palette, crisp pixel edges, game asset sprite` |
| 3 | 穿透 | Penetration | `16-bit pixel art icon, retro SNES style, pixelated, no anti-aliasing, transparent background, 64x64 canvas, a silver arrow piercing through a stone slab, impact fragments, dark dungeon RPG theme, limited 8-color palette, crisp pixel edges, game asset sprite` |
| 4 | 攻击力 | Damage | `16-bit pixel art icon, retro SNES style, pixelated, no anti-aliasing, transparent background, 64x64 canvas, a crossed battle axe and sword, steel metallic, dark dungeon RPG theme, limited 8-color palette, crisp pixel edges, game asset sprite` |
| 5 | 移动速度 | MoveSpeed | `16-bit pixel art icon, retro SNES style, pixelated, no anti-aliasing, transparent background, 64x64 canvas, a winged leather boot, small white wing feathers, speed motion lines behind, dark dungeon RPG theme, limited 8-color palette, crisp pixel edges, game asset sprite` |
| 6 | 生命提升 | MaxHP | `16-bit pixel art icon, retro SNES style, pixelated, no anti-aliasing, transparent background, 64x64 canvas, a red heart with golden border outline, small spark highlight, dark dungeon RPG theme, limited 8-color palette, crisp pixel edges, game asset sprite` |
| 7 | 护甲强化 | Armor | `16-bit pixel art icon, retro SNES style, pixelated, no anti-aliasing, transparent background, 64x64 canvas, a steel knight shield with golden trim, rivets, dark dungeon RPG theme, limited 8-color palette, crisp pixel edges, game asset sprite` |
| 8 | 拾取范围 | MagnetRange | `16-bit pixel art icon, retro SNES style, pixelated, no anti-aliasing, transparent background, 64x64 canvas, a U-shaped horseshoe magnet, red and silver, magnetic field lines radiating, dark dungeon RPG theme, limited 8-color palette, crisp pixel edges, game asset sprite` |
| 9 | 射程 | Range | `16-bit pixel art icon, retro SNES style, pixelated, no anti-aliasing, transparent background, 64x64 canvas, a concentric target crosshair with arrow at center, rangefinder scope, dark dungeon RPG theme, limited 8-color palette, crisp pixel edges, game asset sprite` |
| 10 | 经验加成 | XPBoost | `16-bit pixel art icon, retro SNES style, pixelated, no anti-aliasing, transparent background, 64x64 canvas, a glowing green gem crystal, faceted, radiant light rays, dark dungeon RPG theme, limited 8-color palette, crisp pixel edges, game asset sprite` |
| 11 | 暴击 | Crit | `16-bit pixel art icon, retro SNES style, pixelated, no anti-aliasing, transparent background, 64x64 canvas, a golden five-pointed star with impact burst lines, critical hit symbol, dark dungeon RPG theme, limited 8-color palette, crisp pixel edges, game asset sprite` |

### 2.2 卡片装饰边框

```
16-bit pixel art UI frame border, retro SNES style, pixelated, no anti-aliasing,
transparent background, 320x420 canvas, ornate dungeon stone frame with golden corners,
decorative pixel corner ornaments (torch brackets, skull motifs),
inner dark stone texture, outer gold trim,
designed as a card frame for a level-up selection screen,
limited 12-color palette, crisp pixel edges, game UI asset
```

> **注意**：此边框生成后，程序化代码只需将其作为 Sprite 设置到卡片 Image 上，
> 稀有度色条仍用程序化 Color 叠加（顶部色条 + 边框 tint）。

### 2.3 升级面板标题横幅

```
16-bit pixel art banner ribbon, retro SNES style, pixelated, no anti-aliasing,
transparent background, 800x80 canvas, ornate golden scroll banner with stone texture,
dungeon RPG style, decorative pixel ornaments at both ends (small flames, chains),
centered empty space for text overlay,
limited 10-color palette, crisp pixel edges, game UI asset
```

### 2.4 稀有度色条装饰

```
16-bit pixel art horizontal strip, retro SNES style, pixelated, no anti-aliasing,
transparent background, 300x14 canvas, a decorative gem-studded bar,
ornate pixel pattern with small gem inlays, dark dungeon RPG style,
designed to be tinted by rarity color at runtime,
limited 6-color palette (grayscale base for tinting), crisp pixel edges, game UI asset
```

---

## 三、HUD 优化（参考 Vampire Survivors 布局）

### 3.0 整体布局对比

```
Vampire Survivors HUD 布局（参考）:
┌────────────────────────────────────────────┐
│              0:32                           │ ← 顶部居中：存活时间（大字）
│        ████████████████████░░░░             │ ← 顶部：经验条（近全宽）
│  Lv.5                                      │ ← 经验条左侧：等级
│                                  ☠ 128     │ ← 右上角：击杀计数
│                                            │
│                                            │
│              [玩家]                        │
│            ┌─────┐                         │ ← 角色头顶：HP 条（世界空间）
│            │HP条  │                         │
│            └─────┘                         │
└────────────────────────────────────────────┘

当前布局问题:
- HP 和时间都在屏幕角落，HP 没有跟随角色
- 经验条在底部，不符合 VS 的顶部布局
- 没有击杀计数
```

### 3.1 新布局方案

```
目标布局：
┌──────────────────────────────────────────────┐
│                0:32                          │ ← A. 顶部居中：存活时间
│   ████████████████████████████░░░░░░░░░      │ ← B. 顶部：经验条（90% 屏宽居中）
│  Lv.5                              ☠ 128    │ ← C. 经验条左侧等级  D. 右上角击杀数
│                                              │
│               [玩家]                         │
│              ┌──────┐                        │ ← E. 角色头顶：HP 条（世界空间跟随）
│              │ 45/100│                        │
│              └──────┘                        │
└──────────────────────────────────────────────┘

要素分解：
A. 存活时间 — 顶部居中，大号像素字，带半透明暗色背景条
B. 经验条 — 顶部全宽（90%屏宽），蓝色填充，满级变金
C. 等级 — 经验条左侧，金色 "Lv.N" 文字
D. 击杀计数 — 右上角，骷髅图标 + 数字
E. HP 条 — 跟随角色的世界空间血条（不是屏幕固定 UI）
```

### 3.2 各要素详细规范

#### A. 存活时间（顶部居中）

```
位置: anchorMin/Max = (0.5, 1)，pivot = (0.5, 1)
anchoredPosition = (0, -12)
尺寸: 200×44
背景: 半透明深色圆角条 (HudBg, alpha=0.8)
文字: WarmWhite, 字号 32（像素字体），居中
格式: "M:SS"（分:秒，秒补零）
无图标（VS 原作纯数字最简洁）
```

#### B. 经验条（顶部全宽）

```
位置: anchorMin = (0.05, 1), anchorMax = (0.95, 1)，pivot = (0.5, 1)
anchoredPosition = (0, -56)
高度: 22px
结构（三层，从下到上）:
  1. 边框层: StoneBorder 色，尺寸 = 经验条 + 2×borderWidth(3px)
  2. 填充层: DungeonBlue 色，左对齐，宽度随经验值从右端收缩
  3. 高光层: 1px 白色半透明线在填充顶部（模拟金属反光）
满级时填充色蓝→金渐变
经验数值: 条内右侧，字号 16，白色，alpha=0.85
```

#### C. 等级显示（经验条左侧）

```
位置: anchorMin/Max = (0.05, 1)，pivot = (1, 0.5)
anchoredPosition = (-8, -67)  // 经验条左端外侧
文字: "Lv.N"，GoldText，字号 24，右对齐
不再使用圆形徽章（VS 原作等级就是经验条旁的纯文字）
```

#### D. 击杀计数（右上角）

```
位置: anchorMin/Max = (1, 1)，pivot = (1, 1)
anchoredPosition = (-16, -12)
尺寸: 160×44
背景: 半透明深色圆角条 (HudBg, alpha=0.8)
结构: 骷髅图标(左 28×28) + 数字(右)
数据源: GameStats.kills
```

#### E. HP 条（角色头顶，世界空间跟随）

```
实现: WorldSpace Canvas，挂在 Player 下
sortingOrder: 50（低于屏幕 UI 的 90）

尺寸（世界单位）:
- hpBarWidth = 1.5（约等于玩家视觉宽度）
- hpBarHeight = 0.18
- borderWidth = 0.03

位置: localPosition = (0, 1.2, 0)  // 玩家头顶上方

结构（三层）:
  1. 边框层: StoneBorder
  2. 延迟白条: 白色半透明 (1,1,1,0.6)，受伤时停留原值指数衰减
  3. 填充层: HSV 变色（满血绿→半血黄→残血红），平滑过渡

保留现有逻辑:
- ghostLerpSpeed = 4f
- Color.HSVToRGB(0.33f * hpRatio, 0.9f, 1f)
- Lerp(currentColor, targetColor, 10f * deltaTime)
```

### 3.3 配色优化

```
当前配色 → 目标配色（保留地牢主题，提高对比度）

PanelBg:    rgba(0.08, 0.08, 0.12, 0.9)  → rgba(0.06, 0.05, 0.10, 0.92)  // 更暗更紫
GoldBorder: rgba(0.85, 0.65, 0.2, 1)     → rgba(0.95, 0.72, 0.15, 1)     // 更亮更饱和
GoldText:   rgba(1, 0.85, 0.3, 1)        → rgba(1, 0.88, 0.35, 1)        // 更亮
StoneText:  rgba(0.7, 0.7, 0.75, 1)      → rgba(0.78, 0.76, 0.82, 1)     // 提高可读性
HudBg:      rgba(0.06, 0.06, 0.1, 0.72) → rgba(0.04, 0.03, 0.08, 0.80)  // 更暗（顶部条用）
DungeonBlue:rgba(0.3, 0.5, 0.9, 1)      → rgba(0.25, 0.55, 1.0, 1)     // 更亮更饱和
BtnNormal:  rgba(0.20, 0.16, 0.10, 1)   → rgba(0.18, 0.14, 0.08, 1)     // 更暗
BtnHover:   rgba(0.32, 0.26, 0.14, 1)   → rgba(0.38, 0.30, 0.12, 1)     // 更亮更暖
```

### 3.4 AI 生成资源 Prompt — HUD 图标

#### 骷髅图标（击杀计数用）

```
16-bit pixel art icon, retro SNES style, pixelated, no anti-aliasing,
transparent background, 32x32 canvas, a small white skull seen from front,
empty eye sockets, simple jaw teeth, dungeon RPG style,
designed as a kill counter icon for a game HUD,
limited 4-color palette (white, light gray, gray, dark gray),
crisp pixel edges, game UI asset
```

### 3.5 代码实现规范

#### PlayerHUD.cs 改造要点

```
核心改动：拆分为两个 Canvas

1. ScreenSpaceOverlay Canvas (sortingOrder=90) — 顶部信息栏
   ├─ A. 时间 (顶部居中)
   ├─ B. 经验条 (顶部全宽)
   ├─ C. 等级 (经验条左侧)
   └─ D. 击杀计数 (右上角)

2. WorldSpace Canvas (sortingOrder=50) — 角色头顶 HP 条
   ├─ HP 边框
   ├─ HP 延迟白条
   ├─ HP 填充
   └─ HP 数值（可选）

- CreateUI() → 拆分为 CreateTopBarUI() + CreateWorldSpaceHPBar()
- Refresh() 分两部分: 屏幕空间(时间/经验/等级/击杀) + 世界空间(HP)
- 新增 killsText.text = GameStats.kills.ToString()
- WorldSpace Canvas 作为 Player 子物体自动跟随移动
```

#### WorldSpace HP 条代码

```csharp
GameObject hpCanvasObj = new GameObject("HPBarCanvas");
hpCanvasObj.transform.SetParent(transform, false); // Player 子物体

Canvas hpCanvas = hpCanvasObj.AddComponent<Canvas>();
hpCanvas.renderMode = RenderMode.WorldSpace;
hpCanvas.sortingOrder = 50;

RectTransform hpCanvasRect = hpCanvasObj.GetComponent<RectTransform>();
hpCanvasRect.localPosition = new Vector3(0f, 1.2f, 0f); // 头顶
hpCanvasRect.sizeDelta = new Vector2(hpBarWidth, hpBarHeight + borderWidth * 2f);
hpCanvasRect.localScale = Vector3.one;
// hpBarWidth = 1.5f, hpBarHeight = 0.18f, borderWidth = 0.03f
```

#### 击杀计数代码

```csharp
GameObject killBgObj = new GameObject("KillBg");
killBgObj.transform.SetParent(canvasObj.transform, false);
RectTransform kBgRect = killBgObj.AddComponent<RectTransform>();
kBgRect.anchorMin = new Vector2(1f, 1f);
kBgRect.anchorMax = new Vector2(1f, 1f);
kBgRect.pivot = new Vector2(1f, 1f);
kBgRect.anchoredPosition = new Vector2(-16f, -64f); // 时间面板下方
kBgRect.sizeDelta = new Vector2(160f, 44f);
Image kBgImage = killBgObj.AddComponent<Image>();
kBgImage.sprite = UIDungeonTheme.CreateRoundedSprite(UIDungeonTheme.HudBg, 64, 8f);
kBgImage.type = Image.Type.Sliced;

// Refresh() 中:
if (killsText != null)
    killsText.text = GameStats.kills.ToString();
```

### 3.6 视觉细节改进

```
1. 经验条：满级时变色（蓝 → 金）+ 微光闪烁动画
2. 等级文字：经验条左端，金色，与经验条视觉一体
3. 所有条：添加 1px 高光线（顶部 1px 亮色，模拟金属反光）
4. 顶部时间/击杀面板：半透明圆角背景，不遮挡游戏视野
5. 世界空间 HP 条：无背景面板，纯条+边框，简洁不遮挡视野
6. 世界空间 HP 条数值：可选不显示（VS 原作角色头顶 HP 条无数值）
```

---

## 四、统一面板样式规范（所有界面共用）

> 升级界面、开始界面、结束界面统一使用 AI 生成的背景图作为面板背景。
> 代码只需创建一个 Image 加载背景 Sprite，文字/按钮叠加在其上方。
> 不再用程序化 CreateGradientBorderSprite 拼面板。

### 4.1 背景图方案

```
每个全屏界面生成一张完整的面板背景图，包含:
- 外金色边框 + 内深色石质底纹 + 装饰角饰
- 顶部预留标题区域（纯背景，无文字，文字由代码叠加）
- 底部预留按钮区域

代码侧只需:
  Image bg = panel.AddComponent<Image>();
  bg.sprite = UIArtCache.GetPanelBg(PanelType.StartScreen); // 加载 AI 背景图
  bg.type = Image.Type.Sliced; // 用 Sprite.border 保证边框不被拉伸

程序化生成的 CreateGradientBorderSprite / CreateRoundedSprite 仅用于:
- HUD 顶部小条（时间/击杀数背景）
- 升级卡片（卡片层级仍程序化，面板层级用 AI 背景图）
```

### 4.2 各界面背景图尺寸

```
界面         背景图尺寸      Sprite.border(四边)   用途
──────────────────────────────────────────────────────────────
升级界面     1200×700       12px                  升级面板底
开始界面     800×400        12px                  开始面板底
结束界面     600×650        12px                  结束面板底
通用按钮     240×80         8px                   按钮底（三个界面共用）

背景图必须设置 Sprite.border，配合 Image.Type.Sliced 使边框在任何尺寸下不被拉伸。
```

### 4.3 统一背景图视觉规范

```
所有背景图共同视觉特征:
- 16-bit 像素风，retro SNES style
- 外边框: 金色像素装饰边框（带角饰：火把/骷髅/铆钉）
- 内填充: 深色石质底纹（暗紫灰 #0F0D18），带细微裂纹和噪点
- 四角: 像素装饰角饰（金色，对称）
- 无文字（文字由代码叠加）
- 透明背景（仅面板区域有内容，外部透明）
- 边框区域与中心区域有明确视觉区分（边框亮，中心暗）

各界面差异:
- 升级界面背景: 标准金色边框 + 深色底纹
- 开始界面背景: 标准金色边框 + 深色底纹 + 顶部微暖光（火把光效）
- 结束界面背景: 标准金色边框 + 深色底纹 + 顶部深红色调（区别于其他界面）
```

### 4.4 统一按钮样式

```
按钮背景图（AI 生成，三界面共用）:
- 240×80 像素风按钮底图
- 深色石质底 + 金色边框 + 像素角饰
- 设置 Sprite.border=8px，Sliced 拉伸

按钮叠加:
- Image.sprite = AI按钮背景图 (Sliced)
- Button colors: normal=白色(Image不tint), hover=微亮黄tint, pressed=暗化tint
- 文字: GoldText, 字号 36, 居中
- 悬停: AddHoverScale(1.06f)

注意: 按钮不再用程序化 CreateBorderSprite/CreateSolidSprite
```

### 4.5 统一标题样式

```
所有界面标题:
- 像素字体 + AddTextEffect
- 位置: 面板顶部，距顶 30px
- 分隔线: 标题下方，CreateDividerSprite(GoldBorder)

各界面标题:
- 升级界面: GoldText, 48px, "升级！选择一项强化"
- 开始界面: GoldText, 64px, "幸存者 Demo"
- 结束界面: 浅红 rgba(1,0.35,0.35), 56px, "游戏结束"
```

### 4.6 AI 生成资源 Prompt

#### 升级界面背景图

```
16-bit pixel art game UI panel background, retro SNES style, pixelated, no anti-aliasing,
transparent background outside the panel, 1200x700 canvas,
ornate golden pixel border frame (12px thick) with decorative corner ornaments
(small torch flames at top corners, skull motifs at bottom corners),
inner dark dungeon stone texture with subtle cracks and noise,
very dark purple-gray base (#0F0D18),
center area empty for content overlay, no text,
limited 12-color palette (gold, dark gold, dark stone, stone gray, black, dark purple-gray),
crisp pixel edges, game UI panel asset, designed for Sliced sprite with 12px border
```

#### 开始界面背景图

```
16-bit pixel art game UI panel background, retro SNES style, pixelated, no anti-aliasing,
transparent background outside the panel, 800x400 canvas,
ornate golden pixel border frame (12px thick) with decorative corner ornaments
(small torch flames at all four corners with subtle warm glow),
inner dark dungeon stone texture with subtle cracks,
warm torch light gradient from top corners fading to dark center,
very dark purple-gray base (#0F0D18),
center area empty for content overlay, no text,
limited 10-color palette (gold, dark gold, dark stone, stone gray, warm orange glow, black),
crisp pixel edges, game UI panel asset, designed for Sliced sprite with 12px border
```

#### 结束界面背景图

```
16-bit pixel art game UI panel background, retro SNES style, pixelated, no anti-aliasing,
transparent background outside the panel, 600x650 canvas,
ornate golden pixel border frame (12px thick) with decorative corner ornaments
(broken chain links at bottom corners, small skull at top center),
inner dark dungeon stone texture with dark red tint at top section,
cracked stone fragments scattered at bottom,
very dark purple-gray base (#0F0D18) with dark red overlay at top,
center area empty for content overlay, no text,
limited 12-color palette (gold, dark gold, dark stone, blood red, dark red, black),
crisp pixel edges, game UI panel asset, designed for Sliced sprite with 12px border
```

#### 通用按钮背景图

```
16-bit pixel art button background, retro SNES style, pixelated, no anti-aliasing,
transparent background outside the button, 240x80 canvas,
dark stone button base with golden pixel border trim (8px thick),
small decorative pixel rivets at four corners,
dungeon RPG style, no text, designed for game UI buttons,
limited 6-color palette (gold, dark gold, dark stone, stone gray, black, dark purple-gray),
crisp pixel edges, game UI asset, designed for Sliced sprite with 8px border
```

---

## 五、开始界面优化

### 5.1 布局规范

```
Canvas: ScreenSpaceOverlay, sortingOrder=120
参考分辨率: 1920×1080

结构（AI背景图 + 文字/按钮叠加）:
┌═══════════════════════════════════╗
│ ┊         幸存者 Demo           ┊ │  ← 标题: GoldText, 64px, 距顶 30px
│ ┊      ───────────────────      ┊ │  ← 分隔线: GoldBorder, 500px宽
│ ┊       WASD 移动 · 自动攻击     ┊ │  ← 副标题: StoneText, 28px
│ ┊                               ┊ │
│ ┊          ┌─────────┐          ┊ │  ← Start 按钮: 240×80 (AI按钮背景图)
│ ┊          │ Start   │          ┊ │
│ ┊          └─────────┘          ┊ │
└═══════════════════════════════════┘
  ↑ 金色像素边框 + 深色石质底纹 = AI生成的一整张背景图

背景图: 800×400, UIArtCache.GetPanelBg(PanelType.StartScreen)
全屏遮罩: OverlayBg
```

### 5.2 AI 生成资源 Prompt

#### 标题艺术字

```
16-bit pixel art title text logo, retro SNES style, pixelated, no anti-aliasing,
transparent background, 700x120 canvas,
text reads "SURVIVOR" in ornate pixel font,
golden letters with dark stone shadow, small torch flame decorations at both ends,
dungeon RPG title screen aesthetic,
limited 8-color palette (gold, dark gold, stone gray, dark stone, black),
crisp pixel edges, game title asset
```

> 面板背景图见第四章 4.6 的「开始界面背景图」prompt。

### 5.3 代码改造要点

```
StartScreenUI.cs — CreateUI() 改造

⚠️ 必须删除旧 UI 元素，防止新旧重叠:

1. 删除旧的程序化面板:
   - 删除 CreateGradientBorderSprite 面板 Image
   - 删除旧的 panel GameObject 及其所有子物体
   - 删除旧的 btnBorder Image（金色实心外框）

2. 新建 AI 背景图面板:
   GameObject panel = new GameObject("Panel");
   panel.transform.SetParent(canvasObject.transform, false);
   RectTransform panelRect = panel.AddComponent<RectTransform>();
   // 居中锚定
   panelRect.anchorMin = new Vector2(0.5f, 0.5f);
   panelRect.anchorMax = new Vector2(0.5f, 0.5f);
   panelRect.pivot = new Vector2(0.5f, 0.5f);
   panelRect.anchoredPosition = Vector2.zero;
   panelRect.sizeDelta = new Vector2(800f, 400f);
   
   Image panelImage = panel.AddComponent<Image>();
   panelImage.sprite = UIArtCache.GetPanelBg(PanelType.StartScreen);
   panelImage.type = Image.Type.Sliced;
   panelImage.color = Color.white;

3. 标题/副标题/分隔线: 保持现有代码，父物体改为新 panel

4. 按钮改用 AI 按钮背景图:
   Image btnImage = btnObj.AddComponent<Image>();
   btnImage.sprite = UIArtCache.ButtonBg;
   btnImage.type = Image.Type.Sliced;
   // 不再需要单独的 btnBorder 外框
```

---

## 六、游戏结束界面优化

### 6.1 布局规范

```
Canvas: ScreenSpaceOverlay, sortingOrder=110
参考分辨率: 1920×1080

结构（AI背景图 + 深红标题条 + 统计 + 按钮）:
┌═══════════════════════════════════╗
│ ┊░░░░░░░░░ 游戏结束 ░░░░░░░░░░░░┊ │  ← 深红标题条: 600×90, 渐变红（程序化，叠加在背景图上）
│ ┊                               ┊ │
│ ┊       存活时间：2:35           ┊ │  ← 统计行: 白色, 36px
│ ┊     ───────────────────      ┊ │  ← 分隔线
│ ┊         等级：12               ┊ │  ← 统计行: 浅蓝, 36px
│ ┊     ───────────────────      ┊ │
│ ┊        击杀数：128             ┊ │  ← 统计行: 金色, 36px
│ ┊     ───────────────────      ┊ │
│ ┊  伤害 12 · 攻速 ×2.5 · 子弹 3  ┊ │  ← 构筑属性: 白色, 24px
│ ┊                               ┊ │
│ ┊          ┌─────────┐          ┊ │  ← 重新开始按钮: 240×80 (AI按钮背景图)
│ ┊          │重新开始  │          ┊ │
│ ┊          └─────────┘          ┊ │
└═══════════════════════════════════┘
  ↑ 金色像素边框 + 深色石质底纹 = AI生成的一整张背景图

背景图: 600×650, UIArtCache.GetPanelBg(PanelType.GameOver)
深红标题条: 程序化 CreateGradientBorderSprite 叠加在背景图顶部
```

### 6.2 统计行布局规范

```
三行统计 + 一行构筑属性:

位置 (anchorY)  内容           字号  颜色
──────────────────────────────────────────
0.62           存活时间：M:SS   36    白色 (1, 1, 1)
0.50           等级：N          36    浅蓝 (0.5, 0.9, 1)
0.38           击杀数：N         36    金色 (1, 0.85, 0.3)
0.20           构筑属性摘要      24    白色

分隔线位置 (yOffset = (anchorY - 0.5) × 650):
- Divider1: 39px    (时间与等级之间)
- Divider2: -39px   (等级与击杀之间)
- Divider3: -136px  (击杀与构筑之间)

每条分隔线: 440×2, CreateDividerSprite(Divider色)
```

### 6.3 AI 生成资源 Prompt

#### 游戏结束标题横幅

```
16-bit pixel art title text, retro SNES style, pixelated, no anti-aliasing,
transparent background, 500x90 canvas,
text reads "GAME OVER" in cracked pixel font,
blood red gradient letters with dark stone shadow,
shattered stone fragments at bottom,
dungeon RPG game over screen aesthetic,
limited 8-color palette (blood red, dark red, stone gray, black),
crisp pixel edges, game UI asset
```

> 面板背景图见第四章 4.6 的「结束界面背景图」prompt。

### 6.4 代码改造要点

```
GameOverUI.cs — CreateUI() 改造

⚠️ 必须删除旧 UI 元素，防止新旧重叠:

1. 删除旧的程序化面板和外框:
   - 删除 frame GameObject（金色实心外框 624×674）
   - 删除旧的 panel GameObject 及其所有子物体
     （Image/CreateGradientBorderSprite/标题条/统计行/分隔线/按钮 全部重建）
   - 删除旧的 btnBorder Image

2. 新建 AI 背景图面板（替换原来的 frame + panel 双层结构）:
   panel = new GameObject("GameOverPanel");
   RectTransform pRect = panel.AddComponent<RectTransform>();
   pRect.anchorMin = new Vector2(0.5f, 0.5f);
   pRect.anchorMax = new Vector2(0.5f, 0.5f);
   pRect.pivot = new Vector2(0.5f, 0.5f);
   pRect.sizeDelta = new Vector2(600f, 650f);
   pRect.anchoredPosition = Vector2.zero;
   
   Image pImage = panel.AddComponent<Image>();
   pImage.sprite = UIArtCache.GetPanelBg(PanelType.GameOver);
   pImage.type = Image.Type.Sliced;
   pImage.color = Color.white;
   // 不再需要单独的 frame 外框（背景图自带边框）

3. 深红标题条: 保持程序化，叠加在背景图上方
   - CreateGradientBorderSprite(透明, 红0.50/0.10/0.10, 红0.35/0.05/0.05, 64, 0)
   - 位置: 面板顶部 600×90

4. 统计行/分隔线/构筑属性: 保持现有代码，父物体改为新 panel

5. 按钮改用 AI 按钮背景图:
   - 删除旧的 btnBorder Image（金色实心外框）
   - Image btnImage = btnObj.AddComponent<Image>();
   - btnImage.sprite = UIArtCache.ButtonBg;
   - btnImage.type = Image.Type.Sliced;

6. 弹出动画: 保持 ScaleIn (0.85→1.0, ease-out cubic)
7. panel SetActive 同显同隐: 保持现有逻辑（不再有 frame 需要同步）
```

---

## 七、代码实现指南

### 7.1 资源加载架构

```csharp
// 新增：UIArtCache.cs — 统一加载 AI 生成的美术资源
public static class UIArtCache
{
    // 面板背景图类型
    public enum PanelType { Upgrade, StartScreen, GameOver }

    // 升级图标（按 UpgradeType 索引）
    private static Sprite[] upgradeIcons;
    
    // 各界面面板背景图
    private static Sprite upgradePanelBg;
    private static Sprite startScreenPanelBg;
    private static Sprite gameOverPanelBg;
    
    // 通用按钮背景图
    private static Sprite buttonBg;
    
    // 骷髅图标（击杀计数用）
    private static Sprite skullIcon;
    
    public static Sprite GetPanelBg(PanelType type)
    {
        if (upgradePanelBg == null) LoadAll();
        switch (type)
        {
            case PanelType.Upgrade:     return upgradePanelBg;
            case PanelType.StartScreen:  return startScreenPanelBg;
            case PanelType.GameOver:     return gameOverPanelBg;
            default: return null;
        }
    }
    
    public static Sprite ButtonBg
    {
        get { if (buttonBg == null) LoadAll(); return buttonBg; }
    }
    
    public static Sprite GetUpgradeIcon(UpgradeConfig.UpgradeType type)
    {
        if (upgradeIcons == null) LoadAll();
        return upgradeIcons[(int)type];
    }
    
    private static void LoadAll()
    {
        upgradePanelBg     = Resources.Load<Sprite>("UI/UpgradePanelBg");
        startScreenPanelBg = Resources.Load<Sprite>("UI/StartScreenPanelBg");
        gameOverPanelBg    = Resources.Load<Sprite>("UI/GameOverPanelBg");
        buttonBg           = Resources.Load<Sprite>("UI/ButtonBg");
        skullIcon          = Resources.Load<Sprite>("UI/SkullIcon");
        
        upgradeIcons = new Sprite[UpgradeConfig.TypeCount];
        upgradeIcons[(int)UpgradeConfig.UpgradeType.FireRate]    = Resources.Load<Sprite>("UI/FireRateIcon");
        upgradeIcons[(int)UpgradeConfig.UpgradeType.BulletCount]  = Resources.Load<Sprite>("UI/BulletCountIcon");
        // ... 其余类型
    }
}
```

### 7.2 UpgradeUI.cs 改造要点

```csharp
// SetCard 方法改造：加入图标
private void SetCard(Button button, Text titleText, Text descText, 
                     Image iconImage, UpgradeConfig.UpgradeDefinition def)
{
    // ... 现有稀有度色边框逻辑保留 ...
    
    // 新增：设置图标
    iconImage.sprite = UIArtCache.GetUpgradeIcon(def.type);
    iconImage.preserveAspect = true;
    iconImage.color = Color.white;
    
    // 图标位置：卡片上半部分中央，尺寸 80x80
    // 稀有度色叠加：图标的发光层用稀有度色 tint
}
```

### 7.3 卡片布局改造

```
当前卡片布局（纯文字）:
┌─────────────────┐
│  [稀有度色条]     │
│                 │
│   标题文字       │
│   (类型名+稀有度) │
│                 │
│   描述文字       │
└─────────────────┘

目标卡片布局（加图标）:
┌─────────────────┐
│  [稀有度色条]     │
│      ┌─────┐    │
│      │图标 │    │  ← 80x80 像素图标，居中
│      └─────┘    │
│   类型名(大字)    │
│   稀有度(小字彩色) │
│  ──分隔线──      │
│   描述文字       │
└─────────────────┘

尺寸调整：
- 卡片宽度: 320px → 300px（三张总宽 900px + 间距，适配面板 1200px）
- 卡片高度: 420px → 440px（给图标腾空间）
- 图标区域: 上边距 50px，尺寸 80x80
- 标题下移: 50px → 140px
- 描述位置: 30px → 30px（底部不变）
```

### 7.4 UIFont.cs 改造

```csharp
public static class UIFont
{
    private static Font cachedFont;
    
    public static Font Get()
    {
        if (cachedFont != null) return cachedFont;
        
        // 优先加载项目内像素字体
        cachedFont = Resources.Load<Font>("Fonts/ZPix");
        
        // 兜底：系统字体
        if (cachedFont == null)
            cachedFont = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 16);
        
        return cachedFont;
    }
}
```

### 7.5 PlayerHUD.cs 改造要点

```
核心改动：拆分为两个 Canvas

1. ScreenSpaceOverlay Canvas (sortingOrder=90) — 顶部信息栏
   ├─ 时间 (顶部居中)
   ├─ 经验条 (顶部全宽 90%)
   ├─ 等级 (经验条左侧)
   └─ 击杀计数 (右上角，骷髅图标+数字)

2. WorldSpace Canvas (sortingOrder=50) — 角色头顶 HP 条
   ├─ HP 边框 (世界单位: 1.5×0.24)
   ├─ HP 延迟白条 (世界单位: 1.5×0.18)
   ├─ HP 填充 (世界单位: 1.5×0.18，HSV变色)
   └─ HP 数值文字 (可选)

- CreateUI() → CreateTopBarUI() + CreateWorldSpaceHPBar()
- WorldSpace Canvas 作为 Player 子物体，自动跟随移动
- HP 条尺寸从屏幕像素改为世界单位 (1.5/0.18/0.03)
- HP 条位置: localPosition = (0, 1.2, 0) 玩家头顶
- 新增 killsText 绑定 GameStats.kills
- 删除: 旧左上角布局、心形图标、沙漏图标、HUD背景面板、圆形徽章
```

### 7.6 StartScreenUI.cs 改造要点

```
⚠️ 核心改动：删除旧程序化面板 → 改用 AI 背景图

必须删除的旧 UI 元素（防止新旧重叠）:
- 旧的 panel GameObject（含 CreateGradientBorderSprite Image）
- 旧的 btnBorder Image（金色实心外框 224×74）
- 旧的 btnImage（BtnNormal 色纯色矩形）

新建:
1. 面板: Image.sprite = UIArtCache.GetPanelBg(PanelType.StartScreen), Sliced
   - 尺寸 800×400，居中
   - 不再需要外框 Image（背景图自带金色边框）

2. 按钮: Image.sprite = UIArtCache.ButtonBg, Sliced
   - 尺寸 240×80
   - 不再需要 btnBorder 外框 Image
   - Button colors: normal=白, hover=微亮, pressed=暗化

3. 标题/副标题/分隔线: 保持现有代码，父物体改为新 panel

4. 全屏遮罩: 保持 OverlayBg
```

### 7.7 GameOverUI.cs 改造要点

```
⚠️ 核心改动：删除旧 frame+panel 双层结构 → 改用 AI 背景图

必须删除的旧 UI 元素（防止新旧重叠）:
- frame GameObject（金色实心外框 624×674）
- 旧的 panel GameObject（含 CreateGradientBorderSprite Image）
- 旧的 btnBorder Image（金色实心外框 244×84）
- 旧的 btnImage（BtnNormal 色纯色矩形）
- 旧的 TitleStrip（CreateGradientBorderSprite 渐变红标题条）
- 旧的统计行/分隔线/构筑属性（全部重建到新 panel 下）

新建:
1. 面板: Image.sprite = UIArtCache.GetPanelBg(PanelType.GameOver), Sliced
   - 尺寸 600×650，居中
   - 不再需要 frame 外框（背景图自带金色边框）
   - 不再需要 frame SetActive 同步（只有 panel 一个物体）

2. 深红标题条: 保持程序化，叠加在背景图上方
   - CreateGradientBorderSprite(透明, 红0.50/0.10/0.10, 红0.35/0.05/0.05)
   - 位置: 面板顶部 600×90

3. 统计行/分隔线/构筑属性: 保持现有代码，父物体改为新 panel

4. 按钮: Image.sprite = UIArtCache.ButtonBg, Sliced
   - 不再需要 btnBorder 外框

5. 弹出动画: 保持 ScaleIn (0.85→1.0)
6. panel SetActive: 保持现有逻辑
```

### 7.8 UpgradeUI.cs 面板改造

```
⚠️ 核心改动：删除旧程序化面板 → 改用 AI 背景图

必须删除的旧 UI 元素（防止新旧重叠）:
- 旧的 panel GameObject（含 CreateGradientBorderSprite Image 1200×700）
- 旧的 overlay Image（纯色半透明遮罩）→ 保留遮罩但确保不与面板重叠

新建:
1. 面板: Image.sprite = UIArtCache.GetPanelBg(PanelType.Upgrade), Sliced
   - 尺寸 1200×700，居中
   - 不再需要外框 Image（背景图自带金色边框）

2. 标题/分隔线: 保持现有代码，父物体改为新 panel

3. 卡片: 保持现有渐变边框+稀有度色（卡片层级不用 AI 背景图）
```

---

## 八、生成执行顺序

```
Phase 1: 像素字体
  └─ 下载 ZPix 字体 → 放入 Resources/Fonts/ → 修改 UIFont.cs

Phase 2: 升级图标（11 张）
  └─ 用 frontier-game-design 逐张生成
     统一 prompt 前缀 + 各类型具体描述
     输出: Resources/UI/ 目录下 11 个 Sprite

Phase 3: 面板背景图 + 按钮背景图
  └─ 用 frontier-game-design 生成
     ├─ 升级界面背景图 (1200×700, 12px border)
     ├─ 开始界面背景图 (800×400, 12px border)
     ├─ 结束界面背景图 (600×650, 12px border)
     └─ 通用按钮背景图 (240×80, 8px border)
     输出: Resources/UI/ 目录下 4 个 Sprite

Phase 4: HUD 改造
  ├─ 改造 PlayerHUD.cs（拆分顶部UI + 世界空间HP条）
  ├─ 顶部: 时间(居中) + 经验条(全宽) + 等级(左侧) + 击杀数(右上)
  ├─ 世界空间: HP 条跟随 Player，尺寸改世界单位
  ├─ 改造 UIFont.cs（切换像素字体）
  └─ 新增 UIArtCache.cs（统一资源加载）

Phase 5: HUD 辅助图标
  └─ 骷髅图标（击杀计数用，32×32 像素风）

Phase 6: 升级界面改造
  ├─ 改造 UpgradeUI.cs（加图标 + 布局调整）
  └─ 卡片布局: 图标区域 80×80 + 标题下移 + 描述底部

Phase 7: 开始/结束界面改造
  ├─ StartScreenUI.cs: 删除旧程序化面板 → AI 背景图 + AI 按钮图
  ├─ GameOverUI.cs: 删除旧 frame+panel → AI 背景图 + AI 按钮图
  ├─ UpgradeUI.cs: 删除旧程序化面板 → AI 背景图
  ├─ ⚠️ 每个脚本改造时必须先删除旧 UI 元素，防止新旧重叠
  └─ 标题艺术字（AI 生成，可选叠加）
```

---

## 九、Vampire Survivors 参考要点

| 元素 | VS 做法 | 当前状态 | 改进方向 |
|------|---------|----------|----------|
| 字体 | 像素字体 | 微软雅黑 | 导入 ZPix |
| 经验条 | 顶部全宽，蓝色 | 底部，窄 | 移到顶部全宽 |
| HP 条 | 角色头顶（世界空间） | 屏幕角落固定 | 改世界空间跟随角色 |
| 等级 | 经验条左侧纯文字 | 屏幕角落圆形徽章 | 移到经验条左侧，去徽章 |
| 时间 | 顶部居中大字 | 右上角 | 移到顶部居中 |
| 击杀数 | 右上角 | 无 | 新增骷髅图标+数字 |
| 升级卡片 | 图标+名称+描述 | 纯文字 | 加图标+改布局 |
| 面板边框 | 像素装饰边框 | 程序化渐变 | AI 生成完整背景图（含边框+底纹+角饰）|
| 面板背景 | 石质纹理 | 纯色渐变 | AI 背景图直接作为 Image.sprite |
| 按钮装饰 | 像素边框+图标 | 程序化矩形 | AI 生成按钮背景图（Sliced）|
| 配色 | 高对比深色 | 低对比 | 提高饱和度 |
| 文字效果 | 轻描边 | 描边+投影 | 随字体调整 |
| 界面统一性 | 全局一致风格 | 各界面独立配色 | 统一 AI 背景图风格 + 统一按钮 |
| 旧 UI 清理 | N/A | 新旧元素重叠 | ⚠️ 改造时必须删除旧程序化 UI 元素 |
