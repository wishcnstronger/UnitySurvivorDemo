# Vampire Survivors Like Demo 开发计划

## 项目目标

本项目为 Unity 2D 生存类 Roguelike Demo。

开发目标：

1. 熟悉 Unity 游戏开发完整流程
2. 理解游戏核心系统实现方式
3. 掌握 AI 辅助游戏开发流程
4. 制作可用于作品集展示的完整 Demo

项目定位：

参考《Vampire Survivors》等生存类游戏。

核心循环：

玩家移动
↓
遭遇敌人
↓
自动攻击
↓
击败敌人
↓
获得经验
↓
升级选择技能
↓
强化角色
↓
挑战更强敌人


---

# Phase 0 项目初始化

状态：✅ 已完成

内容：

- 创建 Unity 项目
- 配置 VS Code
- 配置 Claude Code
- 配置 AI 开发环境
- 建立项目结构


---

# Phase 1 玩家系统

状态：✅ 已完成


## 功能目标

实现玩家基础移动。


## 已完成内容

- Player 创建
- 玩家对象配置
- Rigidbody2D 配置
- WASD 移动


## 学习内容

- Unity Scene
- GameObject
- Component
- MonoBehaviour
- Rigidbody2D
- Input


---

# Phase 2 敌人系统

状态：🚧 进行中


## 功能目标

实现基础敌人行为。


## 开发内容

### Enemy

- 创建 Enemy Prefab
- 添加碰撞组件
- 添加移动逻辑


### Enemy Movement

实现：

- 自动寻找玩家
- 朝玩家移动


### Enemy Spawner

实现：

- 定时生成敌人
- 随机出生位置


## 完成标准

玩家进入游戏后：

- 敌人持续生成
- 敌人自动靠近玩家


---

# Phase 3 战斗系统


状态：⬜ 未开始


## 功能目标

实现玩家自动攻击。


## 开发内容

### Weapon System

- 武器对象
- 攻击范围
- 攻击频率


### Projectile

- 子弹生成
- 子弹移动
- 子弹碰撞


### Damage

- 伤害计算
- Enemy死亡


## 完成标准

玩家无需操作即可攻击敌人。


---

# Phase 4 角色成长系统


状态：⬜ 未开始


## 功能目标

实现 Roguelike 成长体验。


## 开发内容

### Experience

- 敌人掉落经验
- 玩家拾取经验


### Level System

- 等级提升
- 升级界面


### Skill Choice

实现：

升级时三选一：

例如：

- 攻击力提升
- 攻击速度提升
- 移动速度提升


## 完成标准

玩家可以通过成长强化角色。


---

# Phase 5 游戏流程系统


状态：⬜ 未开始


## 开发内容

### 开始界面

- Start按钮
- 游戏入口


### 游戏结束

- 玩家死亡
- Game Over界面
- Restart


### UI

- HP显示
- 等级显示
- 时间显示


---

# Phase 6 商店与扩展系统


状态：⬜ 未开始


## 开发内容

- 游戏内货币
- 商店界面
- 永久升级


---

# Phase 7 美术优化


状态：⬜ 未开始


## 开发内容

替换：

- 玩家模型
- 怪物模型
- 特效
- UI


优化：

- 动画
- 音效
- 手感


---

# 最终目标

完成一个：

- 可运行
- 有完整循环
- 有成长系统
- 有UI
- 有基本美术表现

的 Unity 游戏 Demo。