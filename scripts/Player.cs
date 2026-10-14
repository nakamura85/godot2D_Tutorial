using Godot;

/// <summary>
/// 玩家控制器：左右移动 + 跳跃 + 重力 + 掉落重置
///
/// 核心知识点：
/// 1. CharacterBody2D 是 2D 游戏中最常用的“自己控制移动”的物理体
/// 2. Velocity（速度）由我们自己维护，MoveAndSlide() 负责移动并处理碰撞
/// 3. IsOnFloor() 判断是否站在地面上（必须先 MoveAndSlide 才准确）
/// </summary>
public partial class Player : CharacterBody2D
{
	// 移动速度（像素/秒）
	private const float Speed = 320.0f;

	// 起跳速度（负数表示向上，因为 Godot 2D 的 Y 轴向下）
	// 跳跃高度 ≈ v² / (2×重力) = 720² / (2×1400) ≈ 185 像素
	private const float JumpVelocity = -720.0f;

	// 从 project.godot 里读取默认重力（在 项目设置 -> 物理 -> 2D 里配置）
	private float _gravity = (float)ProjectSettings.GetSetting("physics/2d/default_gravity").AsDouble();

	// 出生点，掉坑后回到这里
	private Vector2 _startPosition;

	public override void _Ready()
	{
		_startPosition = GlobalPosition;
	}

	public override void _PhysicsProcess(double delta)
	{
		// ---- 重力：不在地面上时持续加速下落 ----
		if (!IsOnFloor())
		{
			Velocity = new Vector2(Velocity.X, Velocity.Y + _gravity * (float)delta);
		}

		// ---- 跳跃：只有在地面上才能跳 ----
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			Velocity = new Vector2(Velocity.X, JumpVelocity);
		}

		// ---- 左右移动：GetAxis 返回 -1.0 ~ 1.0 ----
		float direction = Input.GetAxis("move_left", "move_right");
		if (direction != 0.0f)
		{
			Velocity = new Vector2(direction * Speed, Velocity.Y);
		}
		else
		{
			// 没按键时平滑减速停下
			Velocity = new Vector2(Mathf.MoveToward(Velocity.X, 0.0f, Speed), Velocity.Y);
		}

		// ---- 执行移动 + 碰撞 ----
		MoveAndSlide();

		// ---- 掉出地图：回到出生点 ----
		if (GlobalPosition.Y > 1200.0f)
		{
			GlobalPosition = _startPosition;
			Velocity = Vector2.Zero;
		}
	}
}
