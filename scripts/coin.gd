extends Area2D
## 金币：被玩家碰到就消失，并发出 collected 信号
##
## 核心知识点：
## 1. Area2D 不参与物理碰撞，专门用来做"检测重叠"（触发器）
## 2. body_entered 信号：其他物理体（CharacterBody2D 等）进入时触发
## 3. signal（信号）是 Godot 的观察者模式：金币只负责"喊一声"，
##    主场景决定听到之后做什么（加分、判断胜利），职责分离

# 收集时发出的信号，主场景会连接这个信号
signal collected

# 记录初始高度，用于上下浮动动画
var _base_y: float

func _ready() -> void:
	_base_y = position.y
	# 把 body_entered 信号连到自己的处理函数
	body_entered.connect(_on_body_entered)

func _process(_delta: float) -> void:
	# 简单的上下浮动动画，让金币看起来有活力
	position.y = _base_y + sin(Time.get_ticks_msec() / 200.0) * 5.0

func _on_body_entered(body: Node2D) -> void:
	# 只对玩家生效（玩家在 player.tscn 里加入了 "player" 分组）
	if body.is_in_group("player"):
		collected.emit()
		# 从场景中移除自己
		queue_free()
