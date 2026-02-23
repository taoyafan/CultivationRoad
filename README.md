# Cultivation Road 战斗系统架构

## 整体架构

游戏采用**事件驱动架构**，核心是**时间系统(TimeSystem)** 和 **动作系统(ActionSystem)**。

```
玩家操作/系统事件
    ↓
创建 GameAction
    ↓
TimeSystem.AddAction(action, countdown)  ← 所有动作先进入时间系统
    ↓
TimeSystem 执行队列（倒计时结束）
    ↓
ActionSystem.PerformAndWait(action)     ← 送入动作系统
    ↓
ActionSystem.Flow()
    ├─ PreReactions   (预处理)
    ├─ Performer      (执行动作)
    ├─ PerformReactions  (子动作，由AddReaction添加)
    └─ PostReactions  (后处理)
```

---

## 目录结构

```
Assets/_Scripts/BattleScene/
├── GameActions/        # 动作定义（What: 要做什么）
├── General/            # 核心框架
│   ├── ActionSystem/ # 动作系统
│   └── Util/          # 工具类
├── Systems/            # 系统处理器（How: 怎么做）
├── Effects/            # 卡牌效果定义
├── Models/             # 业务模型
├── Views/              # 视图组件
├── Data/               # 数据资源
├── Interfaces/         # 接口定义
├── StatusEffects/      # 状态效果实现
├── Strategies/         # 敌人AI策略
├── TargetModes/        # 目标选择模式
├── Creators/           # 对象创建器
├── UI/                 # UI组件
├── Enums/              # 枚举定义
├── Extensions/         # 扩展方法
└── PerkConditions/    # 天赋条件
```

---

## 核心概念

### 1. TimeSystem（时间系统）

**职责：** 管理带倒计时的动作队列，实现回合制战斗

**核心流程：**
```csharp
// 1. 添加动作（倒计时 = 释放时间）
TimeSystem.Instance.AddAction(new CastSuccessGA(card), card.CastTime);

// 2. TimeSystem.Update() 每帧减少倒计时

// 3. 倒计时结束，执行动作
private IEnumerator ExecuteAction()
{
    Pause();  // 先暂停
    yield return ActionSystem.PerformAndWait(action);  // 执行
    // 检查是否需要恢复
    if (!needPaused) Resume();
}
```

**代码位置：** `Assets/_Scripts/BattleScene/Systems/TimeSystem.cs`

---

### 2. ActionSystem（动作系统）

**职责：** 执行具体的动作逻辑

**执行流程（Flow 方法）：**
```
1. PreReactions   → 预处理（护盾吸收伤害）
2. Performer     → 执行动作本身（造成伤害）
3. PerformReactions → 子动作（Performer中通过AddReaction添加）
4. PostReactions → 后处理（攻击者返回原位）
```

**代码位置：** `Assets/_Scripts/BattleScene/General/ActionSystem/ActionSystem.cs`

---

### 3. GameAction（动作）

**定义：** 所有游戏事件的载体

**代码位置：** `Assets/_Scripts/BattleScene/GameActions/`

---

### 4. Performer（处理器）

**定义：** 每个动作的具体执行逻辑

**代码位置：** 各 System 类的 OnEnable 方法

---

## 各模块详解

### 1. GameActions（动作定义）

存放所有 `GameAction` 子类，定义**做什么**。

| 文件 | 说明 |
|------|------|
| `CastCardGA.cs` | 玩家开始释放卡牌 |
| `CastSuccessGA.cs` | 卡牌释放完成 |
| `PlayCardGA.cs` | 卡牌效果执行 |
| `DealDamageGA.cs` | 单目标伤害 |
| `AttackGA.cs` | 多目标攻击 |
| `KillEnemyGA.cs` | 敌人死亡 |
| `WinGameGA.cs` / `LoseGameGA.cs` | 战斗结束 |
| `SceneChangeGA.cs` | 切换场景 |

**代码位置：** `Assets/_Scripts/BattleScene/GameActions/`

---

### 2. Systems（系统处理器）

实现各个 Performer，定义**怎么做**。

| 系统 | 职责 | 关键文件 |
|------|------|----------|
| CardSystem | 卡牌生命周期 | `CardSystem.cs` |
| DamageSystem | 伤害处理 | `DamageSystem.cs` |
| EnemySystem | 敌人和AI | `EnemySystem.cs` |
| TimeSystem | 动作队列 | `TimeSystem.cs` |
| StatusEffectSystem | 状态效果 | `StatusEffectSystem.cs` |
| MatchSetupSystem | 战斗初始化/结束 | `MatchSetupSystem.cs` |
| HeroSystem | 英雄管理 | `HeroSystem.cs` |
| ManaSystem | 法力管理 | `ManaSystem.cs` |
| BurnSystem | 灼烧效果 | `BurnSystem.cs` |
| ManualTargetSystem | 手动选择目标 | `ManualTargetSystem.cs` |

**代码位置：** `Assets/_Scripts/BattleScene/Systems/`

---

### 3. Effects（卡牌效果）

定义卡牌的具体效果，实现 `Effect` 接口。

| 效果 | 说明 |
|------|------|
| `DealDamageEffect` | 造成伤害 |
| `DealOnceDamageEffect` | 一次性伤害 |
| `DrawCardsEffect` | 抽牌 |
| `AddStatusEffectEffect` | 添加状态效果 |
| `DestroySelfEffect` | 销毁自身 |

**代码位置：** `Assets/_Scripts/BattleScene/Effects/`

---

### 4. Models（业务模型）

核心业务逻辑类。

| 模型 | 说明 |
|------|------|
| `Card.cs` | 卡牌业务模型，管理状态和效果 |
| `Effect.cs` | 效果基类 |
| `StatusEffect.cs` | 状态效果基类 |
| `TargetMode.cs` | 目标选择模式基类 |
| `AutoTargetEffect.cs` | 自动目标效果 |

**代码位置：** `Assets/_Scripts/BattleScene/Models/`

---

### 5. Views（视图组件）

负责视觉表现，继承 `MonoBehaviour`。

| 组件 | 说明 |
|------|------|
| `CardView.cs` | 卡牌视图 |
| `CardsView.cs` | 卡牌组视图（手牌/战场） |
| `EnemyView.cs` | 敌人视图 |
| `HeroView.cs` | 英雄视图 |
| `DamagePopup.cs` | 伤害数字弹出 |
| `TimeLineView.cs` | 时间轴视图 |
| `CombatantView.cs` | 战斗者基类 |

**代码位置：** `Assets/_Scripts/BattleScene/Views/`

---

### 6. Data（数据资源）

数据模型，通常为 ScriptableObject。

| 数据 | 说明 |
|------|------|
| `CardData.cs` | 卡牌数据 |
| `EnemyData.cs` | 敌人数据 |
| `HeroData.cs` | 英雄数据 |
| `PerkData.cs` | 天赋数据 |

**代码位置：** `Assets/_Scripts/BattleScene/Data/`

---

### 7. Interfaces（接口定义）

定义规范。

| 接口 | 说明 |
|------|------|
| `Damageable.cs` | 可受伤接口（敌人/英雄） |
| `IEnemyStrategy.cs` | 敌人策略接口 |
| `IHaveCaster.cs` | 施法者接口 |

**代码位置：** `Assets/_Scripts/BattleScene/Interfaces/`

---

### 8. StatusEffects（状态效果实现）

具体的状态效果类。

| 效果 | 说明 |
|------|------|
| `ArmorStatus.cs` | 护盾，吸收伤害 |
| `BurnStatus.cs` | 灼烧，持续伤害 |

**代码位置：** `Assets/_Scripts/BattleScene/StatusEffects/`

---

### 9. Strategies（敌人AI策略）

实现不同的敌人行为模式。

| 策略 | 说明 |
|------|------|
| `DefaultEnemyStrategy.cs` | 默认策略 |
| `ShieldEnemyStrategy.cs` | 护盾优先策略 |

**代码位置：** `Assets/_Scripts/BattleScene/Strategies/`

---

### 10. TargetModes（目标选择模式）

定义如何选择目标。

| 模式 | 说明 |
|------|------|
| `HeroTM.cs` | 玩家本体 |
| `AllEnemyTM.cs` | 所有敌人 |
| `RandomEnemyTM.cs` | 随机敌人 |
| `ManualTM.cs` | 手动选择 |
| `NoTM.cs` | 无目标 |

**代码位置：** `Assets/_Scripts/BattleScene/TargetModes/`

---

### 11. Creators（对象创建器）

负责动态创建游戏对象。

| 创建器 | 说明 |
|--------|------|
| `CardViewCreator.cs` | 创建卡牌视图 |
| `EnemyViewCreator.cs` | 创建敌人视图 |
| `DamagePopupManager.cs` | 管理伤害数字池 |
| `TimeSlotCreator.cs` | 创建时间槽 |

**代码位置：** `Assets/_Scripts/BattleScene/Creators/`

---

### 12. UI（用户界面）

UI 组件。

| 组件 | 说明 |
|------|------|
| `ManaUI.cs` | 法力值UI |
| `StatusEffectsUI.cs` | 状态效果图标 |
| `PauseContinueButton.cs` | 暂停/继续按钮 |
| `PerkUI.cs` / `PerksUI.cs` | 天赋UI |

**代码位置：** `Assets/_Scripts/BattleScene/UI/`

---

### 13. General（核心框架）

| 组件 | 说明 |
|------|------|
| `ActionSystem.cs` | 动作系统核心 |
| `GameAction.cs` | 动作基类 |
| `ReactionTiming.cs` | 反应时机枚举 |
| `MovementTracker.cs` | 移动追踪（DOTween辅助） |
| `Singleton.cs` | 单例基类 |

**代码位置：** `Assets/_Scripts/BattleScene/General/`

---

## 战斗流程示例

### 玩家使用卡牌攻击敌人

```
1. 玩家点击卡牌
   ↓
2. 创建 CastCardGA
   ↓
3. TimeSystem.AddAction(CastCardGA, 0)
   ↓ 立即执行
4. CastCardGA.Performer (CardSystem)
   ├─ 从手牌移除
   ├─ ActionSystem.AddReaction(SpendManaGA)
   └─ TimeSystem.AddAction(CastSuccessGA, card.CastTime)
   ↓ 等待释放时间...
5. CastSuccessGA.Performer
   └─ TimeSystem.AddAction(PlayCardGA, 0)
   ↓
6. PlayCardGA.Performer (CardSystem)
   └─ 创建 DealDamageGA
   ↓
7. DealDamageGA 执行：
   ├─ PRE: ProtectBeforeDamage（StatusEffectSystem）
   ├─ Performer: 伤害+动画（DamageSystem）
   │       ↓
   │    敌人血量<=0 → KillEnemyGA
   └─ POST: ProtectAfterDamage
   ↓
8. KillEnemyGA.Performer (EnemySystem)
   ├─ 销毁敌人卡牌
   ├─ 敌人缩放动画 → 等待 → 销毁
   └─ TimeSystem.AddAction(WinGameGA, 0)
   ↓
9. WinGameGA.Performer (MatchSetupSystem)
   ├─ ClearAllActions()
   ├─ 重置游戏状态
   ├─ ResumeTimeGA
   └─ SceneChangeGA
```

---

## 代码查找指南

### 想添加新卡牌效果？
1. **效果类** → `Assets/_Scripts/BattleScene/Effects/`
2. **卡牌数据** → `Assets/_Scripts/BattleScene/Data/CardData.cs`

### 想修改敌人AI？
1. **策略接口** → `Assets/_Scripts/BattleScene/Interfaces/IEnemyStrategy.cs`
2. **策略实现** → `Assets/_Scripts/BattleScene/Strategies/`
3. **敌人系统** → `Assets/_Scripts/BattleScene/Systems/EnemySystem.cs`

### 想添加新状态效果？
1. **效果基类** → `Assets/_Scripts/BattleScene/Models/StatusEffect.cs`
2. **效果实现** → `Assets/_Scripts/BattleScene/StatusEffects/`
3. **效果系统** → `Assets/_Scripts/BattleScene/Systems/StatusEffectSystem.cs`

### 想添加新动作？
1. **定义动作** → `Assets/_Scripts/BattleScene/GameActions/`
2. **注册处理器** → 对应 System 类的 `OnEnable`
3. **实现逻辑** → Performer 方法

### 想添加新目标选择模式？
- **基类** → `Assets/_Scripts/BattleScene/Models/TargetMode.cs`
- **实现** → `Assets/_Scripts/BattleScene/TargetModes/`

### 想修改战斗结束流程？
- **战斗初始化** → `Assets/_Scripts/BattleScene/Systems/MatchSetupSystem.cs`
- **时间系统** → `Assets/_Scripts/BattleScene/Systems/TimeSystem.cs`

### 想修改UI显示？
- **卡牌视图** → `Assets/_Scripts/BattleScene/Views/CardView.cs`
- **卡牌组** → `Assets/_Scripts/BattleScene/Views/CardsView.cs`
- **UI组件** → `Assets/_Scripts/BattleScene/UI/`

---

## 关键设计规范

### 1. 动作必须通过 TimeSystem 执行

```csharp
// 正确
TimeSystem.Instance.AddAction(new WinGameGA(), 0f);

// 错误（直接执行可能打乱顺序）
ActionSystem.Instance.Perform(new WinGameGA());
```

### 2. 战斗结束必须恢复时间

```csharp
// WinGameGA.Performer 中
TimeSystem.Instance.AddAction(new ResumeTimeGA(), 0f);
```

### 3. DOTween 清理

```csharp
// 方法1：等待动画完成
transform.DOScale(Vector3.zero, 0.25f);
yield return new WaitForSeconds(0.25f);
transform.DOKill();
Destroy(gameObject);

// 方法2：OnDestroy 中清理
private void OnDestroy()
{
    transform.DOKill();
}

// 方法3：场景切换时清理
DOTween.KillAll();
DOTween.Clear(true);

// 战斗场景开始时
DOTween.Clear(true);
```

### 4. 单例模式

核心系统继承 `Singleton<T>`：
- 场景切换时自动销毁和重建
- 通过反射自动查找 null 的 SerializeField
