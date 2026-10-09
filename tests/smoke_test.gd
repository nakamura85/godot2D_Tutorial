extends SceneTree
## 自动化冒烟测试：模拟真实操作验证游戏可运行
## 运行方式：godot --headless --script res://tests/smoke_test.gd
## 注意：全部使用 physics_frame 等待，保证与物理引擎时序一致（headless 下
##       process 帧率不封顶，按 process 帧计数会与物理时间脱钩）

var _fails := 0

func _check(cond: bool, label: String) -> void:
	if cond:
		print("PASS: ", label)
	else:
		_fails += 1
		print("FAIL: ", label)

func _initialize() -> void:
	_run()

func _run() -> void:
	# 1. 加载主场景
	var packed: PackedScene = load("res://scenes/main.tscn")
	_check(packed != null, "main.tscn 可加载")
	var main := packed.instantiate()
	root.add_child(main)

	# 2. 等待物理稳定（30 个物理帧 = 0.5 秒），检查玩家落在地面上
	for i in range(30):
		await physics_frame
	var player: CharacterBody2D = main.get_node("Player")
	var coins := get_nodes_in_group("coins")
	_check(coins.size() == 8, "场景中有 8 枚金币 (实际 %d)" % coins.size())
	_check(player.is_on_floor(), "玩家站在地面上 (y=%.1f)" % player.global_position.y)

	# 3. 模拟向右移动 0.5 秒（预期 Δx ≈ 320 × 0.5 = 160 px）
	Input.action_press("move_right")
	var x0 := player.global_position.x
	for i in range(30):
		await physics_frame
	Input.action_release("move_right")
	var dx := player.global_position.x - x0
	_check(dx > 100.0, "向右移动生效 (Δx=%.1f)" % dx)

	# 4. 模拟跳跃：滞空时间 ≈ 2×720/1400 ≈ 62 物理帧，等 90 帧确保落地
	var y0 := player.global_position.y
	Input.action_press("jump")
	for i in range(10):
		await physics_frame
	Input.action_release("jump")
	var min_y := player.global_position.y
	for i in range(90):
		await physics_frame
		min_y = minf(min_y, player.global_position.y)
	_check(min_y < y0 - 100.0, "跳跃生效 (上升 %.1f px)" % (y0 - min_y))
	_check(player.is_on_floor(), "跳跃后落回地面 (y=%.1f)" % player.global_position.y)

	# 5. 模拟收集金币：重新获取分组（移动阶段可能已真实收集了出生点旁的金币），
	#    然后把玩家传送到最右边那颗金币的位置
	var coins_now := get_nodes_in_group("coins")
	var coin: Area2D = coins_now.back()
	player.global_position = coin.global_position
	player.velocity = Vector2.ZERO
	for i in range(20):
		await physics_frame
	_check(not is_instance_valid(coin) or coin.is_queued_for_deletion(), "金币被收集后移除")
	var label: Label = main.get_node("UI/ScoreLabel")
	var collected_count: int = main.collected
	_check(collected_count >= 1, "main.gd 收到 collected 信号 (count=%d)" % collected_count)
	_check(label.text.contains(str(collected_count)), "计分 UI 更新 (%s)" % label.text)

	print("==== RESULT: %s ====" % ("ALL PASS" if _fails == 0 else "%d FAILED" % _fails))
	quit(1 if _fails > 0 else 0)
