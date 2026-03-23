# Changelog

所有对此项目的显著改动都将记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.0.0/)，
本项目遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

---

## [Unreleased]

### Added
- ✨ 统一的材质库 (`Assets/_Project/Materials/`)
  - Environment/ - 环境材质
  - Character/ - 角色材质
  - Props/ - 道具材质
  - UI/ - UI材质
- 📄 项目结构指南文档 (`Assets/_Project/PROJECT_STRUCTURE.md`)

### Changed
- 🔄 **项目结构重组** (2026-03-23)
  - 创建 `_Project/` 文件夹统一管理项目资源
  - 创建 `_ThirdParty/` 文件夹隔离第三方资源包
  - Scripts, Scenes, Prefabs 统一移动到 `_Project/`
  - 所有材质文件整理到 `_Project/Materials/` 分类管理

### Improved
- 📊 项目架构清晰性 - 区分项目资源和第三方资源
- 🎨 材质管理效率 - 统一材质库便于查找和维护
- 📚 文档完整性 - 添加结构说明和开发指南

---

## [0.1.0] - 2026-03-15

### Added
- 🎮 鼠标瞄准和射击系统
  - 玩家可通过鼠标进行瞄准
  - 实现了射击机制
- 🕹️ 基础游戏框架
  - 玩家控制系统
  - 相机跟随

### Notes
- 项目初始版本
- 包含基础3D游戏场景
- 整合了多个第三方资源包

---

## [0.0.1] - 2026-03-01

### Added
- 🚀 Unity 项目初始化
- 📦 集成第三方资源包
  - A_piece_of_nature - 自然环境资源
  - BattleRoyaleDuoPAPBR - 战斗角色资源
  - Skyden_Games - 低多边形环境资源
  - Survival Tools - 生存工具资源

---

## 项目资源清单

### 目前使用的第三方资源
| 资源包 | 用途 | 位置 |
|------|-----|------|
| A_piece_of_nature | 自然环境 | `_ThirdParty/A_piece_of_nature/` |
| BattleRoyaleDuoPAPBR | 角色和武器 | `_ThirdParty/BattleRoyaleDuoPAPBR/` |
| Skyden_Games | 低多边形环境 | `_ThirdParty/Skyden_Games/` |
| Survival Tools | 生存道具 | `_ThirdParty/Survival Tools/` |
| Game Tools | 编辑器工具 | `_ThirdParty/Game Tools/` |
| TextMesh Pro | 文本渲染 | `_ThirdParty/TextMesh_Pro/` |

### 项目自有资源
| 类型 | 位置 | 说明 |
|-----|-----|-----|
| 脚本 | `_Project/Scripts/` | PlayerController, InteractionManager 等 |
| 场景 | `_Project/Scenes/` | 游戏场景文件 |
| 预制体 | `_Project/Prefabs/` | 项目特定的预制体 |
| 材质 | `_Project/Materials/` | 统一管理的材质库 |
| 设置 | `_Project/Settings/` | 项目配置文件 |

---

## 开发建议

### 短期 (Sprint 1-2)
- [ ] 验证所有材质引用在新位置工作正常
- [ ] 测试第三方资源的兼容性
- [ ] 优化项目加载性能

### 中期 (Sprint 3-4)
- [ ] 完善玩家交互系统
- [ ] 扩展武器系统
- [ ] 优化UI界面

### 长期
- [ ] 添加敌人AI系统
- [ ] 实现游戏关卡系统
- [ ] 添加音效和音乐系统

---

## 贡献者
- 小强 (项目维护)
- Claude - MCP 开发助手

---

## License
[Specify your license here]

---

**最后更新**: 2026-03-23
**维护者**: [@Noonelav](https://github.com/Noonelav)
