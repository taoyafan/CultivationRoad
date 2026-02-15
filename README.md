# Cultivation Road 项目架构

## 项目概述

Cultivation Road 是一款基于 Unity 开发的卡牌战斗游戏，采用模块化架构设计，包含完整的动作系统、时间系统和卡牌系统。

## 文档结构

项目的核心代码位于 `Assets/_Scripts` 目录下，采用功能模块化组织：

```
_Scripts/
├── Creators/          # 视图创建器，负责动态生成游戏对象视图
├── Data/              # 数据模型，定义游戏核心数据结构
├── Effects/           # 效果实现，卡牌和技能的具体效果
├── Enums/             # 枚举类型，定义各种状态和类型
├── Extensions/        # 扩展方法，增强现有类型功能
├── GameActions/       # 游戏动作，定义所有游戏事件和交互
├── General/           # 通用系统和工具
│   ├── ActionSystem/  # 动作系统核心实现
│   └── Util/          # 通用工具类
├── Interfaces/        # 接口定义，规范组件行为
├── Models/            # 业务模型，游戏核心逻辑
├── PerkConditions/    # 天赋条件，定义天赋激活条件
├── Systems/           # 游戏系统，管理游戏核心机制
├── TargetModes/       # 目标模式，定义技能和卡牌的目标选择
└── Views/             # 视图组件，负责游戏界面显示
```

### 核心模块说明

#### 1. 动作系统 (Action System)

动作系统是游戏的核心事件处理机制，基于事件驱动架构，允许游戏组件通过注册和触发动作进行通信。

**核心组件：**
- `GameAction` - 所有游戏动作的基类，定义了动作的生命周期
- `ActionSystem` - 动作系统的核心管理器，负责动作的执行和订阅
- `ReactionTiming` - 定义动作反应的时机（PRE/POST）

**主要功能：**
- 动作的执行和订阅机制
- 支持动作的预反应（PreReactions）、执行反应（PerformReactions）和后反应（PostReactions）
- 允许通过 `AttachPerformer` 注册动作处理器
- 支持同步和异步执行动作（`Perform` 和 `PerformAndWait` 方法）

**工作流程：**
1. 创建继承自 `GameAction` 的具体动作类
2. 通过 `ActionSystem.AttachPerformer<T>(performer)` 注册动作处理器
3. 通过 `ActionSystem.Instance.Perform(action)` 执行动作
4. 系统按顺序执行预反应、动作处理器和后反应
5. 支持使用协程等待动作执行完成

#### 2. 时间系统 (Time System)

时间系统管理游戏中所有需要计时执行的动作，实现了一个基于倒计时的动作队列。

**核心功能：**
- 管理带倒计时的 `GameAction` 队列
- 按倒计时时间自动排序动作
- 支持手动暂停/恢复时间流
- 在执行动作时自动暂停时间，动作完成后恢复
- 支持通过 `PauseTimeGA` 动作暂停时间
- 实现了动作的顺序执行，使用协程等待每个动作完成

**工作流程：**
1. 通过 `TimeSystem.Instance.AddAction(action, countdown)` 添加带倒计时的动作
2. 系统按倒计时排序动作队列
3. 当动作倒计时结束时，系统暂停时间并执行动作
4. 支持使用 `PerformAndWait` 确保动作按顺序执行
5. 动作执行完成后恢复时间流

#### 3. 卡牌系统 (Card System)

卡牌系统是游戏的核心玩法机制，包括卡牌数据、模型和视图。

**核心组件：**
- `CardData` - 卡牌数据模型，定义卡牌的基本属性和效果
- `Card` - 卡牌业务模型，管理卡牌的状态和行为
- `CardView` - 卡牌视图组件，负责卡牌的视觉表现
- `CardStatus` - 定义卡牌的各种状态（如 InHand、Casting、CastSuccess 等）

**主要功能：**
- 支持卡牌的绘制、使用、激活等操作
- 实现了卡牌的释放时间（CastTime）机制
- 支持卡牌效果的链式执行
- 提供了完整的卡牌状态管理

#### 4. 效果系统 (Effect System)

效果系统定义了游戏中所有可执行的效果，如伤害、治疗、状态效果等。

**核心组件：**
- `Effect` - 所有效果的基类
- 具体效果类（如 `DealDamageEffect`、`DrawCardsEffect`、`AddStatusEffectEffect` 等）

**主要功能：**
- 定义各种游戏效果的具体实现
- 支持效果的参数化配置
- 提供效果的执行和管理机制
- 支持效果的链式执行（通过 `GetNextEffect` 方法）

#### 5. 状态效果系统 (Status Effect System)

状态效果系统管理游戏中各种状态效果，如护盾、灼烧等持续效果。

**核心组件：**
- `StatusEffect` - 所有状态效果的基类
- 具体状态效果类（如 `ArmorStatus`、`BurnStatus` 等）
- `StatusEffectSystem` - 状态效果系统的管理器
- `StatusEffectsUI` - 状态效果UI组件

**主要功能：**
- 管理状态效果的添加、移除和堆叠
- 实现状态效果的持续时间和效果
- 提供状态效果的UI显示
- 支持状态效果与伤害系统的交互（如护盾吸收伤害）

**状态效果类型：**
- `ArmorStatus` - 护盾效果，吸收一定比例的伤害
- `BurnStatus` - 灼烧效果，持续造成伤害

**工作流程：**
1. 通过 `AddStatusEffectEffect` 添加状态效果
2. 状态效果系统管理效果的持续时间和堆叠
3. UI系统显示当前激活的状态效果
4. 伤害系统与状态效果系统交互（如护盾吸收伤害）

#### 6. 敌人策略系统 (Enemy Strategy System)

敌人策略系统实现了敌人的AI行为，采用策略模式设计，支持多种不同的敌人行为策略。

**核心组件：**
- `IEnemyStrategy` - 敌人策略接口
- `DefaultEnemyStrategy` - 默认敌人策略
- `ShieldEnemyStrategy` - 护盾优先策略
- `CardReleaseDecision` - 卡牌释放决策模型

**策略模式设计：**
- **接口定义**：`IEnemyStrategy` 定义了策略的统一接口
- **具体策略**：不同的策略类实现具体的AI行为逻辑
- **上下文**：`EnemyView` 作为策略的上下文，负责调用策略

**策略优先级逻辑（ShieldEnemyStrategy）：**
1. **第一优先级**：如果场上没有护盾卡牌，优先释放护盾卡牌
2. **第二优先级**：如果场上卡牌数量不足3个，优先释放持续伤害卡牌
3. **第三优先级**：释放其他非护盾卡牌

**目标选择机制：**
- 攻击卡牌：目标为玩家本体
- 护盾卡牌：目标为敌人本体
- 通过 `CardReleaseDecision` 封装卡牌索引和目标信息

**卡牌识别逻辑：**
- **护盾卡牌**：`AddStatusEffectEffect` 且 `StatusEffect` 为 `ArmorStatus`
- **攻击卡牌**：`DealDamageEffect` 或 `DealOnceDamageEffect`
- **持续伤害卡牌**：`DealDamageEffect`

## 关键设计模式

### 1. 单例模式 (Singleton)

多个核心系统（如 `ActionSystem` 和 `TimeSystem`）采用单例模式实现，确保全局只有一个实例，便于访问和管理。

### 2. 事件驱动模式 (Event-Driven)

动作系统基于事件驱动架构，通过 `GameAction` 作为事件载体，实现组件间的解耦通信。

### 3. 协程异步模式 (Coroutine Asynchronous)

大量使用 Unity 协程实现异步操作，特别是在动作执行和时间管理方面，确保游戏流畅运行。

### 4. 命令模式 (Command Pattern)

`GameAction` 系统本质上是命令模式的实现，将游戏操作封装为对象，支持排队、延迟执行和撤销等操作。

### 5. 策略模式 (Strategy Pattern)

敌人策略系统采用策略模式设计，允许动态切换敌人的AI行为策略。

**实现方式：**
- **策略接口**：`IEnemyStrategy` 定义了统一的策略接口
- **具体策略**：不同的策略类（如 `DefaultEnemyStrategy`、`ShieldEnemyStrategy`）实现具体的AI逻辑
- **上下文**：`EnemyView` 作为策略的上下文，通过策略接口调用具体策略

**优势：**
- 支持动态切换敌人行为策略
- 易于扩展新的策略类型
- 策略逻辑独立，便于维护和测试

## 核心工作流程

### 卡牌使用流程

1. 玩家选择手牌中的卡牌
2. 系统创建并执行 `PlayCardGA` 动作
3. 检查并执行卡牌的释放时间（通过 `TimeSystem` 管理）
4. 释放时间结束后，创建并执行 `CastSuccessGA` 动作
5. 执行卡牌的具体效果（如伤害、治疗等）
6. 根据效果创建并执行相应的动作（如 `DealDamageGA`）

### 时间系统工作流程

1. 向 `TimeSystem` 添加带倒计时的动作
2. 系统按倒计时排序动作队列
3. 更新所有动作的倒计时
4. 当动作倒计时结束时：
   - 暂停时间流
   - 执行该动作（使用 `PerformAndWait` 确保完成）
   - 移除已执行的动作
   - 继续执行下一个到期的动作
   - 所有到期动作执行完成后恢复时间流

### 状态效果系统工作流程

1. **状态效果添加**：
   - 通过 `AddStatusEffectEffect` 效果添加状态效果
   - 系统检查目标是否已有相同类型的效果
   - 如果已有，增加堆叠数；如果没有，添加新效果

2. **状态效果显示**：
   - `StatusEffectsUI` 组件监听状态效果变化
   - 根据状态效果类型显示对应的图标和堆叠数
   - 当堆叠数为0时自动移除UI显示

3. **状态效果交互**：
   - 伤害系统在造成伤害前检查目标的状态效果
   - 护盾效果会吸收一定比例的伤害
   - 灼烧效果会在特定时机造成持续伤害

### 敌人策略系统工作流程

1. **策略选择阶段**：
   - `EnemyView` 调用当前策略的 `SelectCardToRelease` 方法
   - 策略根据优先级逻辑选择卡牌和目标
   - 返回 `CardReleaseDecision` 包含卡牌索引和目标信息

2. **卡牌执行阶段**：
   - `EnemySystem` 根据 `CardReleaseDecision` 执行卡牌
   - 使用断言确保 `ManualTargetEffect` 必须设置目标对象
   - 根据卡牌类型选择正确的目标模式

3. **目标选择机制**：
   - 策略负责设置 `card.ManualTarget` 属性
   - `EnemySystem` 使用 `ManualTM` 目标模式执行卡牌
   - 确保攻击卡牌攻击玩家，护盾卡牌保护敌人

## 开发规范

1. **代码风格**：采用 PascalCase 命名类和方法，camelCase 命名变量
2. **模块化设计**：每个功能模块独立封装，降低耦合度
3. **单例使用**：仅核心系统使用单例，其他组件通过依赖注入获取引用
4. **动作系统使用**：所有游戏事件和交互必须通过动作系统处理
5. **时间管理**：需要计时的操作必须通过时间系统管理

## 扩展建议

1. **添加新动作**：创建新的 `GameAction` 子类并注册相应的处理器
2. **添加新效果**：继承 `Effect` 类实现新的效果逻辑
3. **添加新卡牌**：创建新的 `CardData` 资源并配置相应的效果
4. **扩展时间系统**：可以添加时间流速控制、暂停状态保存等功能

## 总结

Cultivation Road 采用模块化、事件驱动的架构设计，具有良好的扩展性和可维护性。动作系统和时间系统构成了游戏的核心框架，为卡牌战斗提供了灵活而强大的支持。