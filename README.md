# Godot 2D 平台跳跃教程（C# 版）

一个使用 **Godot 4.7（.NET 版）** 制作的入门级 2D 平台跳跃（Platformer）游戏，脚本全部使用 **C#** 编写——语法与 Unity 通用，适合一边学 Godot 一边积累 C# 经验。
项目不依赖任何外部素材（角色、平台、金币全部用基础节点绘制），克隆即可运行。

## 游戏内容

- 左右移动 + 跳跃（含土狼时间外的简单重力手感调校）
- 6 块悬浮平台 + 1 处深坑（掉下去会回到出生点）
- 8 枚金币：收集全部即胜利
- 实时计分 UI + 胜利提示 + 按 R 重新开始

## 如何运行

1. 从 [godotengine.org](https://godotengine.org/download) 下载 **Godot 4.7 .NET 版**（文件名带 `_mono_`，标准版不支持 C#）
2. 首次打开需要 **.NET 8 SDK**（编译 C# 用），可在 [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) 安装
3. 打开 Godot → 点击「导入」→ 选择本项目的 `project.godot`
4. 首次打开会自动编译，之后点击右上角 ▶ 运行（或按 F5）

## 操作方式

| 按键 | 动作 |
|------|------|
| A / ← | 向左移动 |
| D / → | 向右移动 |
| 空格 / ↑ | 跳跃 |
| R | 重新开始 |

## 项目结构

```
godot2D_Tutorial/
├── project.godot               # 项目配置：主场景、输入映射、重力、程序集名
├── godot2D_Tutorial.csproj     # C# 工程文件（Godot.NET.Sdk 4.7.0 / net8.0）
├── godot2D_Tutorial.sln        # Visual Studio / Rider 解决方案
├── icon.svg                    # 项目图标
├── scenes/
│   ├── main.tscn               # 主场景：关卡布局、金币摆放、UI
│   ├── player.tscn             # 玩家：CharacterBody2D + 外观 + 碰撞体 + 相机
│   └── coin.tscn               # 金币：Area2D 触发器 + 浮动动画
├── scripts/
│   ├── Player.cs               # 移动 / 跳跃 / 重力 / 掉落重置
│   ├── Coin.cs                 # 碰撞检测 → 触发 Collected 事件
│   └── Main.cs                 # 计分 / 胜利判定 / 重开
└── tests/
    └── SmokeTest.cs            # 无头冒烟测试（9 项全过）
```

> **C# 重要规则**：文件名必须与类名**大小写完全一致**（`Player.cs` 里必须是 `class Player`），
> 否则 Godot 会报 "the associated class could not be found"。Unity 也是同样的要求。

## 核心知识点（按文件阅读）

### scripts/Player.cs —— 角色控制
- `CharacterBody2D`：2D 游戏中最常用的"代码控制移动"物理体
- `Velocity` + `MoveAndSlide()`：自己维护速度，引擎负责移动与碰撞
- `IsOnFloor()`：判断是否着地（决定能不能跳）
- 重力公式：`Velocity.Y += gravity * delta`
- 跳跃高度公式：`高度 ≈ 起跳速度² / (2 × 重力)`

### scripts/Coin.cs —— 触发器与信号
- `Area2D`：不阻挡物体，只检测重叠（经典的"触发器"）
- `BodyEntered` 信号 → C# 事件 `+= OnBodyEntered`：谁碰到了我
- `[Signal]` 特性 + `CollectedEventHandler` 委托：金币只负责广播事件，不关心谁在听

### scripts/Main.cs —— 游戏流程
- `GetNode<Label>("UI/ScoreLabel")`：在 `_Ready()` 里获取节点引用（对应 GDScript 的 `@onready`）
- 分组（Group）：`GetTree().GetNodesInGroup("coins")` 把所有金币放进 `coins` 组，一行代码统计总数
- `Coin.Collected += OnCoinCollected`：订阅金币事件（对应原版 tscn 里的 8 条信号连接）
- `GetTree().ReloadCurrentScene()`：重开一局

### scenes/main.tscn —— 场景组合
- 实例化（Instance）：玩家和金币都是可复用的独立场景
- 事件订阅在 C# 的 `_Ready()` 里完成，tscn 不再保存信号连接

## 运行冒烟测试（可选）

```powershell
# 需要 Godot .NET 版，且 .csproj 已编译过（dotnet build godot2D_Tutorial.csproj）
& "Godot_v4.7-stable_mono_win64_console.exe" --headless --path . --script res://tests/SmokeTest.cs
```

预期输出末尾为 `==== RESULT: ALL PASS ====`（验证移动、跳跃、重力、金币收集、计分、重开共 9 项）。

## 后续练习建议

1. 给跳跃加「土狼时间」（离地后短时间内仍可跳）
2. 添加敌人（用 `Area2D` 做碰到即死的区域）
3. 加音效：`AudioStreamPlayer` 播放跳跃 / 收集音效
4. 把金币数量、平台位置做成 `[Export]` 属性方便在编辑器调参

## 许可证

MIT License —— 随意使用、修改、分发。
