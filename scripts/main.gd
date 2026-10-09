extends Node2D
## 主场景：负责计分、胜利判定和重新开始
##
## 核心知识点：
## 1. @onready：在节点进入场景树后自动获取引用（比 _ready 里手动 get_node 方便）
## 2. 分组（Group）：用 "coins" 分组一次性统计所有金币
## 3. reload_current_scene()：重开一局的最简单方法

# 场景里的 UI 节点引用
@onready var score_label: Label = $UI/ScoreLabel
@onready var win_label: Label = $UI/WinLabel

var total_coins := 0
var collected := 0

func _ready() -> void:
	win_label.visible = false
	# 统计场景里所有在 "coins" 分组中的金币
	total_coins = get_tree().get_nodes_in_group("coins").size()
	_update_score_label()

# 这个函数由每颗金币的 collected 信号触发
func _on_coin_collected() -> void:
	collected += 1
	_update_score_label()
	if collected >= total_coins:
		win_label.visible = true

func _update_score_label() -> void:
	score_label.text = "金币：%d / %d" % [collected, total_coins]

func _unhandled_input(event: InputEvent) -> void:
	# 按 R 重新开始
	if event.is_action_pressed("restart"):
		get_tree().reload_current_scene()
