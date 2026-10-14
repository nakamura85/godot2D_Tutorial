using Godot;

/// <summary>
/// 金币：被玩家碰到就消失，并发出 Collected 信号
///
/// 核心知识点：
/// 1. Area2D 不参与物理碰撞，专门用来做“检测重叠”（触发器）
/// 2. BodyEntered 信号：其他物理体（CharacterBody2D 等）进入时触发
/// 3. 事件（C# event / 委托）是观察者模式：金币只负责“喊一声”，
///    主场景决定听到之后做什么（加分、判断胜利），职责分离
/// </summary>
public partial class Coin : Area2D
{
	/// <summary>收集时发出的信号，主场景会连接这个信号。</summary>
	[Signal]
	public delegate void CollectedEventHandler();

	// 记录初始高度，用于上下浮动动画
	private float _baseY;

	public override void _Ready()
	{
		_baseY = Position.Y;
		// 把 BodyEntered 信号连到自己的处理函数
		BodyEntered += OnBodyEntered;
	}

	public override void _Process(double delta)
	{
		// 简单的上下浮动动画，让金币看起来有活力
		Position = new Vector2(Position.X, _baseY + Mathf.Sin(Time.GetTicksMsec() / 200.0f) * 5.0f);
	}

	private void OnBodyEntered(Node2D body)
	{
		// 只对玩家生效（玩家在 player.tscn 里加入了 "player" 分组）
		if (body.IsInGroup("player"))
		{
			EmitSignal(SignalName.Collected);
			// 从场景中移除自己
			QueueFree();
		}
	}
}
