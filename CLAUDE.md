# Unity Demo 开发规范

## 项目目标

这是一个 Unity 2D Vampire Survivors Like Demo。

目标：

- 学习 Unity 开发流程
- 制作可放入作品集的 Demo
- 理解每一步实现，而不是仅生成代码

---

## 开发原则

每次只完成一个 Phase。

不要提前实现后续系统。

不要为了"方便"一次生成整个游戏。

优先保证代码简单、清晰、易理解。

---

## 技术要求

使用：

- MonoBehaviour
- Rigidbody2D
- Collider2D
- Prefab

不要使用：

- ECS
- EventBus
- Behaviour Tree
- A*
- NavMesh
- 状态机（除非后续需要）

---

## 代码规范

所有脚本：

- 添加中文注释
- 类职责单一
- public变量方便Inspector调整
- 命名规范

---

## 回复要求

每次开发结束后说明：

1. 创建了哪些文件
2. 修改了哪些文件
3. 为什么这样设计
4. Unity中新出现的概念
5. 我下一步应该做什么

不要一次开发多个Phase。