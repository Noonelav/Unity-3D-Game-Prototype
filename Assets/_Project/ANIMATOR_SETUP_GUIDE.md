# Animator 动画设置指南

## 问题描述
角色的动画与实际移动不同步：
- 角色位置根据 WASD 输入正确移动
- 但动画一直循环播放一个固定动作
- 需要角色在移动时播放行走动画，静止时播放待机动画

## 解决方案

### 1. 脚本更新 ✅
已在 `PlayerMovement.cs` 中添加了 Animator 控制：
- 脚本现在会更新 **Speed** 参数
- Speed = 0 表示静止（播放待机动画）
- Speed = 1 表示全速移动（播放行走动画）

### 2. Unity 中的 Animator 配置

#### 步骤 1️⃣：打开 Animator 窗口
```
菜单 → Window → Animation → Animator
```

#### 步骤 2️⃣：在 Animator 中创建状态机

你需要创建或编辑 Player 的 Animator Controller：

**状态树结构：**
```
Base Layer
├── Idle (待机) - 设置为默认状态
├── Walk (行走)
└── Transitions (过渡)
```

#### 步骤 3️⃣：设置动画参数

1. **打开 Parameters 面板**
   - 点击 Animator 窗口的 "Parameters" 标签

2. **创建 Speed 浮点参数**
   - 点击 "+" → Float
   - 命名为 `Speed`（必须与脚本中的名称一致）
   - 默认值：0

#### 步骤 4️⃣：创建动画状态

**Idle 状态（待机）**
- 从 DefaultMalePBR 资源包中选择待机动画
- 设置为 Loop

**Walk 状态（行走）**
- 从 DefaultMalePBR 资源包中选择行走动画
- 设置为 Loop

#### 步骤 5️⃣：设置状态过渡

**Idle → Walk 过渡**
1. 右键点击 Idle，选择 "Make Transition"
2. 点击 Walk 完成过渡
3. 配置过渡条件：
   - Condition: `Speed > 0.1`
   - Exit Time: 0
   - Duration: 0.1-0.3 (建议 0.2)
   - Blend Mode: Linear

**Walk → Idle 过渡**
1. 右键点击 Walk，选择 "Make Transition"
2. 点击 Idle 完成过渡
3. 配置过渡条件：
   - Condition: `Speed < 0.1`
   - Exit Time: 0
   - Duration: 0.1-0.3 (建议 0.2)
   - Blend Mode: Linear

### 3. 完整配置示例

```
Parameters:
  - Speed (Float) [默认值: 0]

States:
  Idle
    ├─ Animation: DefaultMalePBR/IdleNormal01
    ├─ Speed: 1.0
    └─ Loop: ✓

  Walk
    ├─ Animation: DefaultMalePBR/WalkForward
    ├─ Speed: 1.0
    └─ Loop: ✓

Transitions:
  Idle → Walk
    └─ Speed > 0.1

  Walk → Idle
    └─ Speed < 0.1
```

### 4. 测试步骤

1. **进入 Play 模式**
   - 按 Play 按钮

2. **测试动画同步**
   - 按 W/A/S/D 移动角色
   - 观察 Animator 窗口中 Speed 参数的值
   - 验证角色动画是否正确切换

3. **观察 Inspector**
   - 选中 Player
   - 在 Animator 组件中查看 Speed 参数值
   - Speed 应该在移动时变为 1.0，静止时变为 0.0

### 5. 故障排除

| 问题 | 解决方案 |
|-----|--------|
| 动画仍不同步 | 检查 Animator Controller 是否正确关联到 Player |
| 过渡不工作 | 确保 Speed 参数名称与脚本中的一致（区分大小写） |
| 动画卡顿 | 调整过渡的 Duration 值，尝试 0.1-0.3 之间的值 |
| 速度参数不变 | 检查 Player 是否有 Animator 组件 |

### 6. 从 DefaultMalePBR 中选择合适的动画

**推荐的动画组合：**
- **Idle**: `IdleNormal01_AR_Anim` 或 `IdleBattle01_AR_Anim`
- **Walk**: `WalkFwd_AR_Anim` 或 `MoveFwd_AR_Anim`

或根据你的游戏风格从以下选择：
```
BattleRoyaleDuoPAPBR/Animation/AssaultRifleStance/
  - IdleNormal01_AR_Anim.fbx
  - MoveFwd_AR_Anim.fbx
  - MoveBwd_AR_Anim.fbx
  - ...等多个动画文件
```

### 7. 进阶：多方向移动

如果需要处理前进、后退、左右移动的不同动画：

```csharp
// 修改 PlayerMovement.cs 中的 UpdateAnimator()
void UpdateAnimator()
{
    if (animator == null) return;

    animator.SetFloat("Speed", currentMovement.magnitude);
    animator.SetFloat("Direction", GetMovementDirection());
}

float GetMovementDirection()
{
    // 根据移动方向返回不同值
    // 用于区分前进、后退等动画
}
```

然后在 Animator 中设置多个 Float 参数和相应的过渡。

---

## 相关文件
- 脚本: `Assets/_Project/Scripts/Player/PlayerMovement.cs`
- 资源: `Assets/_ThirdParty/BattleRoyaleDuoPAPBR/`
- Animator: Player Animator Controller (需要创建或编辑)

## 下一步
1. ✅ 在 Unity 中创建/编辑 Animator Controller
2. ✅ 配置 Speed 参数和过渡
3. ✅ 测试动画同步
4. 🔧 (可选) 添加更多动画状态（跑步、跳跃等）

---

**更新时间**: 2026-03-23
