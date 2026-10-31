# 3D RPG Prototype

一个用 **Unity + C#** 完成的 3D 动作 RPG 原型。核心是「点击移动 + 指令式战斗」的一整套循环：
用鼠标指挥角色走位、贴近目标、普攻与技能、清怪升级，跨场景传送时自动存读档。

> 仓库仅包含 **C# 脚本与 ScriptableObject 数据资产**（22 个脚本），不含场景、模型、动作与音效资源。

---

## 已实现的内容

**角色控制**
- 点击地面移动：鼠标射线命中 `Ground` 后派发目标点，角色由 `NavMeshAgent` 寻路前往
- 点击敌人锁定并自动贴近至攻击距离后出手
- 摄像机使用 **Cinemachine FreeLook**，运行时由代码绑定 `Follow` / `LookAt` 到角色骨骼节点
- 鼠标指针随光标下的物体类型变化（地面 / 敌人 / 可交互物 / 传送门），用自定义贴图 + 热点偏移

**战斗**
- 普攻与技能两套距离判定：`attackRange` 内触发 `attack`，`skillRange` 内触发 `skill`
- 伤害公式：`Random.Range(minDamage, maxDamage) × 暴击倍率 − 目标防御`，结果钳制不小于 0
- 暴击：出手前按 `criticalChance` 掷一次随机，命中与否通过 Animator 的 `critical` 参数驱动不同动作表现
- 命中判定交给 **动画事件**（`Hit()`），并用朝向点积校验，背身不会打到人
- 冷却：`coolDown` 计时，为负才允许再次出手

**成长**
- 击杀按 `killPoint` 给经验，经验满则升级
- 升级提升最大生命与经验门槛，等级系数 `1 + (level - 1) × levelBuff` 控制成长曲线
- 面板：等级、血条、经验条实时刷新

**敌人 AI**
- 四状态机：`GUARD`（原地守卫）/ `PATROL`（随机巡逻）/ `CHASE`（追击）/ `DEAD`
- 索敌：`Physics.OverlapSphere(sightRadius)` 检测 `Player` 标签
- 巡逻点：以出生点为中心在 `patrolRange` 内随机取点，再用 `NavMesh.SamplePosition` 吸附到可行走区域
- 丢失目标后有 `lookAtTime` 的记忆时间，原地环顾再决定回巡逻还是回守卫
- 两类敌人：
  - `Grunt`：近身踢击，把玩家击退并触发眩晕
  - `Golem`：踢击带伤害，另有远程技能——抓取石头投掷
- 死亡：取消 `Enemy` 标签、关闭碰撞体、`agent.radius` 归零，2 秒后销毁

**可交互物：投石**
- 石头作为投射物，用三态区分命中对象：`HitPlayer` / `HitEnemy` / `HitNothing`
- `Golem` 投出后默认砸向玩家；玩家可以在石头落地静止（速度趋近 0 → `HitNothing`）后**反打回去**，命中 Golem 造成伤害并爆裂

**跨场景与存读档**
- `TransitionPoint` 标记传送门（同场景 / 跨场景），`TransitionDestination` 标记落点与用途
- 跨场景流程：存档 → 淡出 → 异步加载 → 在落点实例化角色 → 淡入 → 读档
- 同场景传送：临时关闭 `NavMeshAgent`，直接设置位置与朝向，再重新启用（避免寻路把角色拽回去）
- 存档：`JsonUtility` 序列化角色数据写入 `PlayerPrefs`，同时记录当前场景名，用于「继续游戏」

**架构**
- `Singleton<T>`：泛型单例基类，统一处理实例去重与销毁清理
- 观察者模式：`IEndGameObserver` 注册到 `GameManager`，玩家死亡时统一广播，敌人收尾、场景切回主菜单各自响应
- 数据驱动：`CharacterData_SO` / `AttackData_SO` 都是 ScriptableObject，属性在 Inspector 里配，代码不写死数值

---

## 操作

| 操作 | 功能 |
| --- | --- |
| 鼠标左键 点击地面 | 移动到该点 |
| 鼠标左键 点击敌人 | 靠近并攻击 |
| 鼠标左键 点击石头 | 靠近并击打（可反打 Golem 投来的石头） |
| 鼠标左键 点击传送点 | 前往下一个区域 / 场景 |
| `S` | 保存当前角色数据 |
| `L` | 读取存档 |
| `Esc` | 返回主菜单 |

> `S` / `L` 是调试期留下的快捷键，正式版本应移除或改为菜单入口。

---

## 目录结构

```
3DRPG_Prototype/
├── Animation Behavior/
│   └── StopAgent.cs                动画行为：进入状态时停止寻路
├── Character Stats/
│   ├── MonoBehavior/
│   │   └── CharacterStats.cs       运行时角色属性与伤害结算
│   └── ScriptableObject/
│       └── CharacterData_SO.cs     角色数据模板（血量 / 防御 / 等级 / 经验）
├── Characters/
│   ├── PlayerController.cs         玩家：点击移动、追击、攻击、死亡
│   ├── EnemyController.cs          敌人 FSM 基类
│   └── Enemy/
│       ├── Grunt.cs                小兵：击退 + 眩晕
│       ├── Golem.cs                精英：击退 + 投石
│       └── Rock.cs                 投射物三态逻辑
├── Combat/
│   └── AttackData_SO.cs            攻击数据（射程 / 冷却 / 伤害区间 / 暴击）
├── Game Data/
│   ├── Attack Data/                攻击数据资产（.asset）
│   └── Character Data/             角色数据资产（.asset）
├── Manager/
│   ├── GameManager.cs              全局状态、玩家注册、结束广播
│   ├── MouseManager.cs             光标提示、点击事件派发
│   └── SaveManager.cs              存档 / 读档
├── Tool/
│   ├── ExtensionMethod.cs          Transform 朝向扩展方法
│   ├── IEndGameObserver.cs         游戏结束观察者接口
│   └── Singleton.cs                泛型单例基类
├── Transition/
│   ├── SceneController.cs          同场景 / 跨场景切换调度
│   ├── TransitionDestination.cs    落点标记
│   └── TransitionPoint.cs          传送门标记
└── UI/
    ├── HealthBarUI.cs              世界空间血条（面向摄像机、延时隐藏）
    ├── MainMenu.cs                 主菜单
    ├── PlayerHealthUI.cs           等级 / 血量 / 经验面板
    └── SceneFader.cs               场景淡入淡出
```

---

## 几个值得说明的设计

**1. 数据模板用 Instantiate，不改原资产**

`CharacterStats` 的 `templateData` 是共享的 ScriptableObject。运行时 `Instantiate(templateData)` 复制出
`characterData` 实例，所有数值改动都落在副本上——否则一个敌人掉血，所有同类敌人的资产都会被永久改脏。
这是 SO 做数据时最容易踩的坑。

**2. 伤害交给动画事件，不是代码里定时**

`Hit()` 挂在攻击动作的关键帧上，动作打到哪儿、伤害就在哪儿结算。
再加一层朝向点积校验（`dot ≥ 0.5`），转身过程中的攻击不会隔空命中背后目标。

**3. 同场景传送要临时关掉 NavMeshAgent**

直接 `transform.position = 落点` 之后，`NavMeshAgent` 会用自己的寻路把角色拽回旧路径。
所以流程是：禁用 Agent → 设位置和朝向 → 启用 Agent。

**4. 追击状态有"记忆时间"**

丢失目标后不立刻回巡逻，而是原地停留 `lookAtTime` 秒。玩家绕障碍躲一下，敌人不会瞬间失忆转身就走，
压迫感会明显不同。

---

## 如何运行

仓库只提供脚本与数据资产，**没有可直接运行的工程**。要跑起来需要补齐：

1. 新建 Unity 工程（版本：*（待补充实际使用版本）*）
2. 将各目录放入 `Assets/`，推荐开启 **Assembly Definition** 按目录拆分（可选）
3. 安装依赖包：
   - **Cinemachine**（摄像机；`GameManager` 直接引用了 `CinemachineFreeLook`）
4. 场景侧配置：
   - Tag：`Player`、`Enemy`、`Ground`、`Attackable`、`Portal`
   - 烘焙 NavMesh
   - 角色预制体挂 `NavMeshAgent`、`Animator`、`CharacterStats`
   - Animator 参数需包含：`speed`(float)、`walk` / `chase` / `follow` / `critical` / `death` / `win`(bool)、`attack` / `skill` / `dizzy` / `hit`(trigger)
   - 在攻击动画的关键帧上添加 Animation Event 指向 `Hit()`，`Golem` 另需 `ThrowRock()`、`KickOff()`
   - 世界空间 Canvas 命名为 `HealthBar Canvas`，供 `HealthBarUI` 自动查找

---

## 已知限制与可改进

- **索敌每帧调用 `Physics.OverlapSphere`**：会产生数组分配，敌人数量上来后 GC 压力明显。
  可改为定时检测（如 0.2s 一次）或使用 `NonAlloc` 版本。
- **存档只落地角色属性与场景名**：敌人状态、任务进度、掉落物均未持久化。
- **UI 用 `transform.GetChild(index)` 取引用**：层级一改就静默失效，建议改为序列化字段直接引用。
- **调试快捷键混在正式逻辑里**：`S` / `L` 直接写在 `SaveManager.Update()`，需要抽离。
- **敌人 AI 尚未拆成独立状态类**：目前都在 `EnemyController.SwitchStates()` 的 switch 里，
  随着状态增多会膨胀，可迁移到与玩家侧一致的策略/状态对象。
- **旧版 Input Manager**：迁移到新 Input System 后更易做按键重绑定与手柄支持。
- **无对象池**：石头每次投掷都 `Instantiate`、命中即 `Destroy`。

---

## 说明

个人练习 / 原型项目，代码与数据资产仅用于学习与交流。
