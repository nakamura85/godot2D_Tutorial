using Godot;

/// <summary>
/// 主场景：负责计分、胜利判定和重新开始
///
/// 核心知识点：
/// 1. GetNode&lt;Label&gt;()：在 _Ready 里获取场景中的节点引用
///    （对应 GDScript 的 @onready）
/// 2. 分组（Group）：用 "coins" 分组一次性统计所有金币
/// 3. ReloadCurrentScene()：重开一局的最简单方法
/// </summary>
public partial class Main : Node2D
{
	// 场景里的 UI 节点引用
	private Label _scoreLabel;
	private Label _winLabel;

	private int _totalCoins;
	public int Collected { get; private set; }

	public override void _Ready()
	{
		_scoreLabel = GetNode<Label>("UI/ScoreLabel");
		_winLabel = GetNode<Label>("UI/WinLabel");

		_winLabel.Visible = false;
		// 统计场景里所有在 "coins" 分组中的金币，并把每颗金币的 Collected
		// 事件连到自己的处理函数（对应原版 tscn 里的 8 条信号连接）
		var coins = GetTree().GetNodesInGroup("coins");
		_totalCoins = coins.Count;
		foreach (var coin in coins)
		{
			((Coin)coin).Collected += OnCoinCollected;
		}
		UpdateScoreLabel();
	}

	// 这个函数由每颗金币的 Collected 事件触发
	private void OnCoinCollected()
	{
		Collected++;
		UpdateScoreLabel();
		if (Collected >= _totalCoins)
		{
			_winLabel.Visible = true;
		}
	}

	private void UpdateScoreLabel()
	{
		_scoreLabel.Text = $"金币：{Collected} / {_totalCoins}";
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		// 按 R 重新开始
		if (@event.IsActionPressed("restart"))
		{
			GetTree().ReloadCurrentScene();
		}
	}
}
