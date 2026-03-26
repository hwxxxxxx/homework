# UNITY_CURRENT_STATE

## 基本信息
- Unity 版本：2022
- 当前主场景：GameScene

---

## 场景对象

### Player
- 使用 CharacterController
- 挂有 PlayerController（移动）
- 挂有 PlayerCombat（战斗）
- 挂有 PlayerStats（生命、受伤、死亡）

子物体：
- CameraRoot（相机跟随点）
- WeaponHolder（武器挂点）
    - FirePoint（射线发射点）

---

### Camera
- Main Camera
- CM FreeLook（Cinemachine）
    - Follow：CameraRoot
    - LookAt：CameraRoot

---

### Input
- GameInput（场景中唯一输入入口对象）

---

### Weapon
- WeaponHolder 上挂有 HitscanWeapon
    - 使用 FirePoint 作为发射点
    - 使用 Main Camera 作为瞄准参考
    - 命中判定支持从子碰撞体向父物体查找 IDamageable（可命中敌人子节点碰撞体）

---

### Enemy（V1）
- 敌人物体挂有 NavMeshAgent
- 敌人物体挂有 EnemyBase（受伤入口与死亡收口）
- 敌人物体挂有 EnemyStats（血量与死亡事件）
- 敌人物体挂有 EnemyAIController（Idle / Chase / Attack / Dead）
- 敌人物体挂有 EnemyCombat（攻击间隔、攻击距离、可选视线检测）
- 敌人通过 Tag=Player 的目标查找玩家并追击/攻击
- 敌人和玩家伤害统一走 IDamageable 接口

---

### UI
- Canvas
    - AmmoText（弹药显示）
    - Crosshair（屏幕中心准星）

- HUDController
    - 挂有 PlayerHUD
