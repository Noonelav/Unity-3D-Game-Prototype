# 项目结构指南

## 📁 Assets 目录组织

```
Assets/
├── _Project/                    # 项目核心资源
│   ├── Scripts/                # 所有项目脚本
│   │   ├── Player/            # 玩家相关
│   │   └── Interaction/       # 交互系统
│   ├── Scenes/                # 项目场景
│   ├── Prefabs/               # 项目预制体
│   │   └── UI/
│   ├── Materials/             # 统一材质库
│   │   ├── Environment/       # 环境材质
│   │   ├── Character/         # 角色材质
│   │   ├── Props/             # 道具材质
│   │   └── UI/                # UI材质
│   └── Settings/              # 项目设置（URP、Physics等）
│
├── _ThirdParty/               # 第三方资源包（勿修改）
│   ├── A_piece_of_nature/
│   ├── BattleRoyaleDuoPAPBR/
│   ├── Skyden_Games/
│   ├── Survival Tools/
│   ├── Game Tools/
│   └── TextMesh_Pro/
│
└── TutorialInfo/              # Unity教程信息（可删除）
```

## 📌 重要说明

### 项目资源 (_Project/)
- ✅ **可以修改** - 这是项目的核心资源
- 脚本、场景、自定义预制体都在这里
- 材质统一管理，便于维护

### 第三方资源 (_ThirdParty/)
- ⚠️ **避免修改** - 这些是外部资源包
- 如需更新这些包，保持原始结构
- 如果要使用这些资源，复制到 _Project 中

## 🎨 材质管理规则

| 类型 | 位置 | 说明 |
|-----|-----|-----|
| 环境材质 | `_Project/Materials/Environment/` | 地形、树木、石头等 |
| 角色材质 | `_Project/Materials/Character/` | 玩家、敌人等 |
| 道具材质 | `_Project/Materials/Props/` | 箱子、武器等可交互物体 |
| UI材质 | `_Project/Materials/UI/` | 按钮、背景等UI元素 |

## 🔧 脚本组织规则

```
_Project/Scripts/
├── Player/
│   ├── PlayerController.cs
│   ├── PlayerAnimator.cs
│   └── WeaponSystem.cs
├── Interaction/
│   ├── InteractableObject.cs
│   └── InteractionManager.cs
├── Common/               # 通用工具和管理器
│   ├── GameManager.cs
│   └── ...
└── UI/                   # UI脚本
    ├── MainMenu.cs
    └── ...
```

## ✨ 后续开发建议

1. **添加新场景** → 放在 `_Project/Scenes/`
2. **创建新脚本** → 按功能分类放在 `_Project/Scripts/` 相应子文件夹
3. **创建新材质** → 按类型放在 `_Project/Materials/` 相应子文件夹
4. **使用第三方资源** → 复制到 `_Project/` 中修改

---

更新时间：2026-03-23
