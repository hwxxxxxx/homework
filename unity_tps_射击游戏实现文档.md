# AI 协作开发指导文档

## 1. 项目定位

这是一个基于 Unity 2022 开发的第三人称射击游戏原型，目标是在一个月内完成一个可运行、可通关、可扩展的 V1 版本。

V1 版本必须具备完整单局流程：
- 进入游戏
- 在多个战斗区推进
- 与敌人战斗
- 获得强化
- 进入 Boss 战
- 胜利或失败结算

V1 不实现以下内容：
- 基地系统
- NPC 系统
- 对话系统
- 局外成长
- 联机
- 开放世界
- 随机地图
- 大量武器与复杂技能树

后续版本需要预留接口，以便扩展：
- Roguelike 随机强化
- 基地系统
- 解锁系统
- 局外成长

## 2. 当前目标

当前目标不是堆功能，而是建立清晰、低耦合、可扩展的项目结构。所有实现都必须优先考虑模块边界，避免把输入、逻辑、表现、流程混写在同一个脚本中。

## 3. 架构原则

1. 输入、逻辑、表现分离。
2. 单个脚本只负责一个明确职责。
3. 公共能力通过统一接口连接。
4. 数据与行为尽量分离。
5. 流程控制与实体行为分离。
6. V1 先做最小正确实现，但接口必须考虑未来扩展。
7. 不引入不必要的大型框架，不做过度设计。
8. UI 只负责显示，不直接承担核心逻辑。
9. 能挂在场景级对象上的系统不要挂在 Player 上。
10. 所有新增功能都必须说明职责、依赖对象、挂载位置和未来扩展点。

## 4. 系统分层

### 4.1 基础层
负责提供全局公共能力。

包含：
- GameInput：统一输入读取
- 公共接口，例如 IDamageable
- 通用数据结构、枚举、常量、工具类

要求：
- 不依赖具体业务模块
- 不直接操作 UI
- 不直接操作关卡流程

### 4.2 实体层
负责游戏对象本体行为。

包含：
- Player
- Enemy
- Boss
- Weapon
- Skill

要求：
- 只处理对象自身行为
- 不直接承担全局流程控制
- 不直接管理关卡推进

### 4.3 流程层
负责关卡与游戏状态推进。

包含：
- GameFlowManager
- LevelManager
- WaveManager
- GateController
- BuffManager
- SpawnManager

要求：
- 不负责玩家移动或敌人内部 AI 细节
- 只负责什么时候刷怪、什么时候开门、什么时候进入下一阶段、什么时候胜利失败

### 4.4 表现层
负责显示和反馈。

包含：
- MainMenu UI
- HUD
- Buff Selection UI
- Victory UI
- Fail UI
- 特效、音效、镜头反馈

要求：
- 不承担核心战斗逻辑
- 不直接计算伤害
- 不直接决定流程状态

## 5. 已确定的核心玩法

单局流程如下：
- 进入游戏
- 玩家进入战斗区
- 清理敌人
- 获得一次强化选择
- 进入下一战斗区
- 重复上述过程
- 进入 Boss 战
- 胜利或失败

战斗核心如下：
- 第三人称移动
- 射击
- 装弹
- 技能
- 受伤与死亡

强化核心如下：
- V1 只实现固定 3 选 1
- 不实现随机池、稀有度、复杂联动
- 但接口需要允许未来替换为随机强化池

## 6. 当前和后续系统规划

### 6.1 已完成内容
- 项目初始化
- Git 管理
- 基础场景结构
- 第三人称玩家移动
- Cinemachine 相机
- 输入封装 GameInput
- 第一版射击系统
- 弹夹、装弹、弹药 UI
- 屏幕中心准星
- 基于相机中心点的命中逻辑

### 6.2 当前下一步
下一步实现敌人系统第一版。

目标：
- 敌人能被攻击
- 敌人能追击玩家
- 敌人能攻击玩家
- 敌人能死亡
- 构成基础战斗闭环

### 6.3 后续开发顺序
1. 敌人系统第一版
2. 游戏流程系统
3. 技能系统
4. Buff 系统
5. Boss 系统
6. UI 扩展与结算
7. 打磨与调试

## 7. 模块设计要求

### 7.1 输入模块
模块：GameInput

职责：
- 统一提供 Move、Look、Jump、Run、Fire、Aim、Reload、Skill、Interact、Pause 等输入接口

要求：
- 其他系统不得直接散落使用 Input.GetAxis 或 Input.GetKey
- 如果以后切换到 Unity New Input System，应优先修改 GameInput，而不是大面积修改其他模块

### 7.2 玩家模块
Player 建议结构：
- PlayerController：移动
- PlayerCombat：战斗输入转发
- PlayerStats：生命、属性、受伤、死亡
- PlayerSkillController：技能
- CharacterController：Unity 组件

要求：
- PlayerController 只负责移动
- PlayerCombat 只负责将输入转发给武器或技能
- PlayerStats 不负责移动或 UI
- PlayerSkillController 不负责武器逻辑

### 7.3 武器模块
建议结构：
- WeaponBase
- HitscanWeapon
- 后续可扩展 WeaponData

职责：
- WeaponBase：定义武器共同行为，例如射速、弹夹、装弹、对外状态查询
- HitscanWeapon：实现射线武器开火逻辑

要求：
- 武器不直接读取玩家输入
- 武器不直接控制 UI
- 武器通过接口对目标造成伤害

### 7.4 伤害模块
接口：IDamageable

职责：
- 提供统一受伤入口

要求：
- 武器、技能、敌人攻击都尽量通过该接口传递伤害
- 不要在武器代码里写死目标类型判断

### 7.5 敌人模块
建议结构：
- EnemyBase
- EnemyStats
- EnemyAIController
- EnemyCombat

职责：
- EnemyBase：基础对象管理
- EnemyStats：血量、死亡
- EnemyAIController：状态机与行为切换
- EnemyCombat：攻击实现

V1 状态：
- Idle
- Chase
- Attack
- Dead

V1 敌人类型：
- MeleeEnemy
- RangedEnemy
- BossEnemy

要求：
- AI 逻辑与数值逻辑分开
- 敌人不直接处理关卡推进
- 敌人死亡后由流程层决定是否开门或进入下一阶段

### 7.6 流程模块
建议结构：
- GameFlowManager
- LevelManager
- WaveManager
- GateController
- SpawnManager
- BuffManager

职责：
- 管理游戏状态
- 管理关卡推进
- 管理刷怪
- 管理开门
- 管理强化选择时机

要求：
- 不把流程逻辑塞进 Player 或 Enemy
- 不把刷怪逻辑写在关卡物体的零散脚本中

### 7.7 强化模块
建议结构：
- BuffData
- BuffManager
- 后续可扩展 BuffPool

V1 要求：
- 固定 3 选 1
- 少量简单数值强化
- 不做复杂联动

必须预留的接口：
- GetAvailableBuffs()
- ApplyBuff()

未来扩展方向：
- 随机池
- 稀有度
- 权重
- 局外解锁

### 7.8 数据模块
建议结构：
- ScriptableObject 配置
- GameDataManager
- 后续扩展 SaveData、UnlockManager、MetaProgress

职责：
- 存放配置数据
- 管理未来的存档和解锁接口

要求：
- V1 不做完整存档系统
- 但要预留 Save() / Load() / Unlock() / IsUnlocked() 等接口位置

### 7.9 UI 模块
建议结构：
- MainMenuUI
- PlayerHUD
- BuffSelectionUI
- VictoryUI
- FailUI

职责：
- 只显示当前状态
- 响应流程层指令切换显示

要求：
- UI 不要直接承担核心状态变更
- HUD 读取武器或玩家状态进行显示
- BuffSelectionUI 通过 BuffManager 提供数据

## 8. 当前代码方向要求

当前代码必须保持以下链路：
- 输入通过 GameInput 统一进入
- 玩家战斗输入通过 PlayerCombat 转发
- 武器通过 WeaponBase 对外提供开火和装弹接口
- 命中逻辑由 HitscanWeapon 负责
- 伤害通过 IDamageable 传递
- HUD 通过读取状态更新显示

所有新增脚本都必须明确：
1. 这个脚本负责什么
2. 这个脚本不负责什么
3. 这个脚本依赖哪些对象
4. 这个脚本挂在哪个对象上
5. 未来如何扩展而不推翻现有结构

## 9. 对 AI 的实现要求

AI 协作生成代码时必须遵守：
- 不要把多个职责塞进一个脚本
- 不要为了省事直接让 UI 控制核心逻辑
- 不要在 PlayerController 中加入战斗、UI、Buff 等无关逻辑
- 不要在 Weapon 中直接读取 Input
- 不要在 Enemy 中直接控制关卡流程
- 不要引入没有必要的大型框架
- 不要过度抽象，保持当前项目体量下的清晰性
- 所有代码都要能直接挂到 Unity 2022 项目中运行
- 所有新脚本都要说明挂载位置和 Inspector 需要拖拽的引用

## 10. 当前最优先目标

当前最优先目标是先完成一个完整可玩的 V1 闭环：
- 玩家移动与战斗
- 敌人对抗
- 关卡推进
- 强化选择
- Boss 战
- 胜利失败结算

在 V1 完成前，不新增以下内容：
- 基地系统
- NPC 对话
- 局外成长
- 解锁 UI
- 随机地图
- 复杂剧情

## 11. 最终要求

所有实现都必须服务于以下目标：
- 可运行
- 可通关
- 可调试
- 可扩展
- 结构清晰
- 低耦合
- 不因后续加入 rogue 和基地系统而推翻现有代码

