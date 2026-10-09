extends CharacterBody2D
## 玩家控制器：左右移动 + 跳跃 + 重力 + 掉落重置
##
## 核心知识点：
## 1. CharacterBody2D 是 2D 游戏中最常用的"自己控制移动"的物理体
## 2. velocity（速度）由我们自己维护，move_and_slide() 负责移动并处理碰撞
## 3. is_on_floor() 判断是否站在地面上（必须先 move_and_slide 才准确）

# 移动速度（像素/秒）
const SPEED := 320.0
# 起跳速度（负数表示向上，因为 Godot 2D 的 Y 轴向下）
# 跳跃高度 ≈ v² / (2×重力) = 720² / (2×1400) ≈ 185 像素
const JUMP_VELOCITY := -720.0
# 从 project.godot 里读取默认重力（在 项目设置 -> 物理 -> 2D 里配置）
var gravity: float = ProjectSettings.get_setting("physics/2d/default_gravity")

# 出生点，掉坑后回到这里
var _start_position: Vector2

func _ready() -> void:
	_start_position = global_position

func _physics_process(delta: float) -> void:
	# ---- 重力：不在地面上时持续加速下落 ----
	if not is_on_floor():
		velocity.y += gravity * delta

	# ---- 跳跃：只有在地面上才能跳 ----
	if Input.is_action_just_pressed("jump") and is_on_floor():
		velocity.y = JUMP_VELOCITY

	# ---- 左右移动：get_axis 返回 -1.0 ~ 1.0 ----
	var direction := Input.get_axis("move_left", "move_right")
	if direction != 0.0:
		velocity.x = direction * SPEED
	else:
		# 没按键时平滑减速停下
		velocity.x = move_toward(velocity.x, 0.0, SPEED)

	# ---- 执行移动 + 碰撞 ----
	move_and_slide()

	# ---- 掉出地图：回到出生点 ----
	if global_position.y > 1200.0:
		global_position = _start_position
		velocity = Vector2.ZERO
