---
name: visual-check
description: 画面自查 —— 完成会改变游戏画面的功能后，用视觉模型截图验证画面是否合格。当用户说"自查画面""截图看看""检查画面"时使用。
---

# 画面自查（Visual Check）

在完成/修改了**会呈现在游戏画面上的功能**后执行，用视觉模型把截图翻译成文字，逐项验证画面是否符合预期。

## 何时使用

- 新敌人/Boss、子弹、特效、UI、地图、升级卡、结算界面等任何改变画面的功能
- 纯数值/纯逻辑改动若会反映到 HUD 或结算界面，也应检查
- 纯内部逻辑、画面不可见时跳过，说明跳过理由即可

## 前置检查

1. 确认 UnityMCP 已连接：读 `mcpforunity://instances` 和 `mcpforunity://editor/state`
2. 确认 `Tools/describe_image.py` 可用（key 在 `Tools/.vision_key`，已配置好）

## 步骤

### 1. 整理预期画面清单
从本次功能需求列出可验证的点，例如：
- 应该出现的物体、颜色、位置
- 应该显示的 UI 文字、数值
- 不应该出现的东西（报错、重叠、缺失）

### 2. 推进游戏到能看到该功能的画面
- 需要运行态时：`manage_editor` action=play 进入 Play 模式
- 用 `execute_code` 模拟点击推进流程：点 StartButton → 点构筑卡（Card0/1/2）→ 触发目标功能
- 若是场景搭建/编辑模式可见内容：直接截图 Scene 视图即可
- **注意**：开局 timeScale=0，必须点 Start + 选构筑卡才真正开打；玩家站桩不移动会被敌人快速磨死，需要截战斗画面时临时 `AddMaxHP(9999)` 保命

### 3. 截图
- `manage_camera` action=screenshot output_folder="Assets/Screenshots" screenshot_file_name="check_<功能名>"
- 截图是异步保存，保存后即可读取文件

### 4. 描述画面
- 运行 `py -3 Tools/describe_image.py <截图路径>`
- 省略路径时自动取 Assets/Screenshots/ 下最新一张

### 5. 对比判定
- 逐项对照预期清单，标注：✅符合 / ❌不符合 / ⚠️存疑
- 给出结论：通过 or 不通过
- 不通过 → 定位原因、修复、重截重查

### 6. 汇报
- 说明：看到了什么、与预期差异、是否通过、哪些需要开发者确认
- 结束前 `manage_editor` action=stop 退出 Play 模式，还原编辑器状态

## 边界（重要）

- AI 只能判断**结构性/功能性**画面：东西在不在、对不对、有没有报错
- **手感、节奏、数值手感、审美** AI 无法判断 → 必须明确请开发者试玩确认
- 若视觉描述过于笼统（如角色太小分不清类型），可换 `glm-4v-plus` 模型，或说明只验证 UI 层
- 截图文件保留在 Assets/Screenshots/ 供开发者对照，无需删除
