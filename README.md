# Godot 2D 平台跳跃教程

一个使用 **Godot 4.7** 制作的入门级 2D 平台跳跃（Platformer）游戏。
项目不依赖任何外部素材（角色、平台、金币全部用基础节点绘制），克隆即可运行。

## 游戏内容

- 左右移动 + 跳跃（含土狼时间外的简单重力手感调校）
- 6 块悬浮平台 + 1 处深坑（掉下去会回到出生点）
- 8 枚金币：收集全部即胜利
- 实时计分 UI + 胜利提示 + 按 R 重新开始

## 如何运行

1. 从 [godotengine.org](https://godotengine.org/download) 下载 **Godot 4.7 或更高版本**
2. 打开 Godot → 点击「导入」→ 选择本项目的 `project.godot`
3. 点击右上角 ▶ 运行（或按 F5）

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
├── project.godot        # 项目配置：主场景、输入映射、重力参数
├── icon.svg             # 项目图标
├── scenes/
│   ├── main.tscn        # 主场景：关卡布局、金币摆放、UI、信号连接
│   ├── player.tscn      # 玩家：CharacterBody2D + 外观 + 碰撞体 + 相机
│   └── coin.tscn        # 金币：Area2D 触发器 + 浮动动画
└── scripts/
    ├── player.gd        # 移动 / 跳跃 / 重力 / 掉落重置
    ├── coin.gd          # 碰撞检测 → 发出 collected 信号
    └── main.gd          # 计分 / 胜利判定 / 重开
```

## 核心知识点（按文件阅读）

### scripts/player.gd —— 角色控制
- `CharacterBody2D`：2D 游戏中最常用的"代码控制移动"物理体
- `velocity` + `move_and_slide()`：自己维护速度，引擎负责移动与碰撞
- `is_on_floor()`：判断是否着地（决定能不能跳）
- 重力公式：`velocity.y += gravity * delta`
- 跳跃高度公式：`高度 ≈ 起跳速度² / (2 × 重力)`

### scripts/coin.gd —— 触发器与信号
- `Area2D`：不阻挡物体，只检测重叠（经典的"触发器"）
- `body_entered` 信号：谁碰到了我
- `signal collected`：金币只负责广播事件，不关心谁在听

### scripts/main.gd —— 游戏流程
- `@onready`：进入场景树后自动获取节点引用
- 分组（Group）：把所有金币放进 `coins` 组，一行代码统计总数
- `get_tree().reload_current_scene()`：重开一局

### scenes/main.tscn —— 场景组合
- 实例化（Instance）：玩家和金币都是可复用的独立场景
- 信号连接：8 颗金币的 `collected` 信号全部连到主场景的计分函数

## 后续练习建议

1. 给跳跃加「土狼时间」（离地后短时间内仍可跳）
2. 添加敌人（用 `Area2D` 做碰到即死的区域）
3. 加音效：`AudioStreamPlayer` 播放跳跃 / 收集音效
4. 把金币数量、平台位置做成 `@export` 变量方便调参

## 许可证

MIT License —— 随意使用、修改、分发。
