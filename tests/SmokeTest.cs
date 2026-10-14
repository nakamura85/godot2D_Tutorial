using Godot;
using System.Linq;

/// <summary>
/// 自动化冒烟测试：模拟真实操作验证游戏可运行
/// 运行方式：godot --headless --script res://tests/smoke_test.cs
/// 注意：全部使用 PhysicsFrame 等待，保证与物理引擎时序一致（headless 下
///       process 帧率不封顶，按 process 帧计数会与物理时间脱钩）
/// </summary>
public partial class SmokeTest : SceneTree
{
	private int _fails;

	private void Check(bool cond, string label)
	{
		if (cond)
		{
			GD.Print($"PASS: {label}");
		}
		else
		{
			_fails++;
			GD.Print($"FAIL: {label}");
		}
	}

	public override void _Initialize()
	{
		_ = Run();
	}

	private async System.Threading.Tasks.Task Run()
	{
		// 1. 加载主场景
		var packed = GD.Load<PackedScene>("res://scenes/main.tscn");
		Check(packed != null, "main.tscn 可加载");
		var main = packed.Instantiate<Node2D>();
		Root.AddChild(main);

		// 2. 等待物理稳定（30 个物理帧 = 0.5 秒），检查玩家落在地面上
		for (int i = 0; i < 30; i++)
		{
			await ToSignal(this, SceneTree.SignalName.PhysicsFrame);
		}
		var player = main.GetNode<CharacterBody2D>("Player");
		var coins = GetNodesInGroup("coins");
		Check(coins.Count == 8, $"场景中有 8 枚金币 (实际 {coins.Count})");
		Check(player.IsOnFloor(), $"玩家站在地面上 (y={player.GlobalPosition.Y:F1})");

		// 3. 模拟向右移动 0.5 秒（预期 Δx ≈ 320 × 0.5 = 160 px）
		Input.ActionPress("move_right");
		float x0 = player.GlobalPosition.X;
		for (int i = 0; i < 30; i++)
		{
			await ToSignal(this, SceneTree.SignalName.PhysicsFrame);
		}
		Input.ActionRelease("move_right");
		float dx = player.GlobalPosition.X - x0;
		Check(dx > 100.0f, $"向右移动生效 (Δx={dx:F1})");

		// 4. 模拟跳跃：滞空时间 ≈ 2×720/1400 ≈ 62 物理帧，等 90 帧确保落地
		float y0 = player.GlobalPosition.Y;
		Input.ActionPress("jump");
		for (int i = 0; i < 10; i++)
		{
			await ToSignal(this, SceneTree.SignalName.PhysicsFrame);
		}
		Input.ActionRelease("jump");
		float minY = player.GlobalPosition.Y;
		for (int i = 0; i < 90; i++)
		{
			await ToSignal(this, SceneTree.SignalName.PhysicsFrame);
			minY = Mathf.Min(minY, player.GlobalPosition.Y);
		}
		Check(minY < y0 - 100.0f, $"跳跃生效 (上升 {y0 - minY:F1} px)");
		Check(player.IsOnFloor(), $"跳跃后落回地面 (y={player.GlobalPosition.Y:F1})");

		// 5. 模拟收集金币：重新获取分组（移动阶段可能已真实收集了出生点旁的金币），
		//    然后把玩家传送到最右边那颗金币的位置
		var coinsNow = GetNodesInGroup("coins");
		var coin = coinsNow.Cast<Node2D>().Last();
		player.GlobalPosition = coin.GlobalPosition;
		player.Velocity = Vector2.Zero;
		for (int i = 0; i < 20; i++)
		{
			await ToSignal(this, SceneTree.SignalName.PhysicsFrame);
		}
		Check(!IsInstanceValid(coin) || coin.IsQueuedForDeletion(), "金币被收集后移除");
		var label = main.GetNode<Label>("UI/ScoreLabel");
		var mainScript = (Main)main;
		int collectedCount = mainScript.Collected;
		Check(collectedCount >= 1, $"main.cs 收到 Collected 信号 (count={collectedCount})");
		Check(label.Text.Contains(collectedCount.ToString()), $"计分 UI 更新 ({label.Text})");

		GD.Print($"==== RESULT: {(_fails == 0 ? "ALL PASS" : $"{_fails} FAILED")} ====");
		Quit(_fails > 0 ? 1 : 0);
	}
}
