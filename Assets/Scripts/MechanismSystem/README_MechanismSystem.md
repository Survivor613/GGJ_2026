# 🎮 工业级按钮-机关交互系统

## 📋 系统概述

基于**发送者-接收者**解耦模式的触发机关系统，支持：
- ✅ 多物体同时触发（触发计数器）
- ✅ 平滑插值移动（防止瞬移）
- ✅ 父子关系同步（防止滑落）
- ✅ 一对多、多对一扩展
- ✅ Layer + Tag 双重过滤

---

## 🏗️ 组件说明

### 1️⃣ `IActivatable` 接口
所有机关必须实现此接口：
- `void Activate()` - 激活机关
- `void Deactivate()` - 停用机关
- `bool IsActivated` - 当前状态

### 2️⃣ `TriggerButton` - 触发按钮
**功能**: 检测 Player 和 Box，维护触发计数器

**配置**:
| 参数 | 说明 | 推荐值 |
|------|------|-------|
| `Target Mechanisms` | 要控制的机关列表 | 拖入 MovingPlatform |
| `Trigger Layers` | 检测的Layer | Player (7) + Box (10) |
| `Allowed Tags` | 允许触发的Tag | `["Player", "Box"]` |
| `Activated Color` | 按下时颜色 | Green |
| `Deactivated Color` | 未按下时颜色 | Gray |

**关键逻辑**:
```csharp
OnTriggerEnter2D: triggerCount++  // 物体进入
OnTriggerExit2D:  triggerCount--  // 物体离开
triggerCount == 1: 激活所有连接的机关
triggerCount == 0: 停用所有连接的机关
```

### 3️⃣ `MovingPlatform` - 移动平台
**功能**: 平滑移动 + 乘客同步

**配置**:
| 参数 | 说明 | 推荐值 |
|------|------|-------|
| `Travel Offset` | 位移向量（相对初始位置） | `(5, 0, 0)` |
| `Speed` | 移动速度 | `2` |
| `Hold To Active` | 是否需要持续踩踏 | `false` |
| `Passenger Layers` | 检测乘客的Layer | Player + Box |
| `Raycast Distance` | 向上检测距离 | `0.5` |

**移动算法**:
```csharp
// 使用 Vector3.MoveTowards 平滑插值
Vector3 newPos = Vector3.MoveTowards(currentPos, targetPos, speed * deltaTime);

// 乘客同步（使用 Rigidbody2D.MovePosition）
passengerRb.MovePosition(passengerRb.position + deltaMovement);
```

### 4️⃣ `MultiInputMechanism` - 多输入机关
**功能**: 支持"多对一"逻辑（必须激活N个按钮）

**配置**:
| 参数 | 说明 | 推荐值 |
|------|------|-------|
| `Input Buttons` | 所有输入按钮 | 拖入多个 TriggerButton |
| `Target Mechanisms` | 目标机关 | 拖入 MovingPlatform |
| `Logic Type` | 逻辑类型 | `AND` (全部激活) / `OR` (任意激活) |

---

## 🎯 使用场景

### 场景1：单按钮控制单平台（一对一）

1. **创建按钮**:
   - 创建 Empty GameObject → 命名为 `Button_01`
   - 添加 `BoxCollider2D` (勾选 `isTrigger`)
   - 添加 `SpriteRenderer` (显示按钮贴图)
   - 添加 `TriggerButton` 组件
   
2. **创建平台**:
   - 创建 Sprite → 命名为 `Platform_01`
   - 添加 `BoxCollider2D` (不勾选 isTrigger)
   - 添加 `MovingPlatform` 组件
   - 设置 `Travel Offset = (5, 0, 0)`
   
3. **连接**:
   - 在 `TriggerButton` 的 `Target Mechanisms` 中拖入 `Platform_01`

### 场景2：一按钮控制多平台（一对多）

1. 创建 `Button_01`（同上）
2. 创建 `Platform_01`, `Platform_02`, `Platform_03`
3. 在 `TriggerButton` 的 `Target Mechanisms` 列表中拖入所有平台

### 场景3：多按钮控制一门（多对一）

1. **创建两个按钮**:
   - `Button_01` (添加 `TriggerButton`)
   - `Button_02` (添加 `TriggerButton`)
   
2. **创建逻辑中枢**:
   - 创建 Empty GameObject → 命名为 `LogicHub`
   - 添加 `MultiInputMechanism` 组件
   - `Logic Type` 设为 `AND`
   - `Input Buttons` 拖入 `Button_01` 和 `Button_02`
   
3. **创建门**:
   - 创建 `Door` (添加 `MovingPlatform`)
   - 在 `LogicHub` 的 `Target Mechanisms` 中拖入 `Door`

### 场景4：箱子压按钮开门

1. **配置按钮**:
   - `TriggerButton` 的 `Allowed Tags` 设为 `["Player", "Box"]`
   - `Trigger Layers` 勾选 `Player (7)` 和 `Box (10)`
   
2. **配置 Box 预制体**:
   - 确保 Box 有以下组件:
     - `Rigidbody2D` (Gravity Scale = 1)
     - `BoxCollider2D`
     - Layer = `Box (10)`
     - 建议添加 Tag `"Box"` (需要在 `TagManager` 中添加)

---

## 🔧 项目配置检查

### Tag 配置 (`ProjectSettings/TagManager.asset`)
```
Tags:
  - Player  (已存在)
  - Box     (建议添加)
```

### Layer 配置 (`ProjectSettings/TagManager.asset`)
```
Layers:
  - 6: Ground
  - 7: Player  (已存在)
  - 10: Box    (已存在)
```

### Box 预制体配置 (`Assets/Prefab/Box.prefab`)
建议修改：
```yaml
m_TagString: Box  # 当前是 "Untagged"，建议改为 "Box"
m_Layer: 10       # ✅ 已正确配置
```

---

## 🐛 常见问题

### Q1: 按钮检测不到 Player/Box？
**A**: 检查以下配置:
1. `TriggerButton` 的 `Trigger Layers` 是否勾选了正确的 Layer
2. `Allowed Tags` 是否包含 `"Player"` 和 `"Box"`
3. Player/Box 的 Tag 和 Layer 是否正确设置

### Q2: 平台移动时 Player 会滑落？
**A**: 
1. 确保 `MovingPlatform` 的 `Passenger Layers` 包含 Player 所在的 Layer
2. 检查 `Raycast Distance` 是否足够（推荐 0.5）
3. Player 必须有 `Rigidbody2D` 组件

### Q3: Box 压不下按钮？
**A**:
1. 确保 Box 有 `Rigidbody2D` (Gravity Scale > 0)
2. 确保 Box 的 Layer 是 `10 (Box)`
3. 确保 `TriggerButton` 的 `Trigger Layers` 勾选了 Box Layer

### Q4: 多按钮逻辑不工作？
**A**:
1. 确保 `MultiInputMechanism` 的 `Input Buttons` 列表中拖入了所有按钮
2. 检查 `Logic Type` 设置（AND/OR）
3. 确保每个按钮都正确配置了 `onActivate/onDeactivate` 事件

---

## 📊 性能优化建议

1. **触发计数器**: 自动防止重复触发，无需额外优化
2. **射线检测**: 使用 `FixedUpdate` + 缓存，性能开销极小
3. **乘客同步**: 使用 `Rigidbody2D.MovePosition`，避免物理穿透
4. **接口缓存**: `Awake` 时缓存 `IActivatable` 接口，避免运行时查找

---

## 🎓 扩展性

### 添加自定义机关

1. 创建新脚本，实现 `IActivatable` 接口:
```csharp
public class CustomDoor : MonoBehaviour, IActivatable
{
    public bool IsActivated { get; private set; }
    
    public void Activate()
    {
        // 开门逻辑
        Debug.Log("门打开了！");
    }
    
    public void Deactivate()
    {
        // 关门逻辑（可选）
    }
}
```

2. 将此组件添加到 GameObject
3. 在 `TriggerButton` 的 `Target Mechanisms` 中拖入此 GameObject

---

## 📝 代码审查要点

✅ **已实现的工业级特性**:
- [x] 触发计数器 (`triggerCount`)
- [x] Layer + Tag 双重过滤
- [x] 平滑插值移动 (`Vector3.MoveTowards`)
- [x] 乘客同步 (`Rigidbody2D.MovePosition`)
- [x] 接口解耦 (`IActivatable`)
- [x] 一对多支持
- [x] 多对一支持 (`MultiInputMechanism`)
- [x] 视觉反馈 (`Sprite` + `Color`)
- [x] 编辑器可视化 (`OnDrawGizmosSelected`)
- [x] 调试日志系统

---

## 📧 技术支持

如遇问题，请检查：
1. Console 日志（所有组件都有详细调试输出）
2. Scene 视图中的 Gizmos（选中按钮/平台可看到连接线和检测区域）
3. Inspector 中的参数配置

Good luck! 🎮✨
