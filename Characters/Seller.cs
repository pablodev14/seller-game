using Godot;
using System;

public partial class Seller : CharacterBody2D
{
    private AnimatedSprite2D _animator;
    private TextureRect _xIndicator;
    private Label _woodIndicator;
    public int WoodCounts { get; private set; } = 20;
    private Label _coinIndicator;
    private int _coinCounts = 0;
    private Node2D _npc;

    public override void _Ready()
    {
        _animator = GetNode<AnimatedSprite2D>("Animator");
        _animator.Play("default");

        _xIndicator = GetNode<TextureRect>("Screen/XIndicator");
        _xIndicator.Visible = false;

        _woodIndicator = GetNode<Label>("Screen/WoodIndicator/Label");
        _woodIndicator.Text = WoodCounts.ToString();

        _coinIndicator = GetNode<Label>("Screen/CoinIndicator/Label");
        _coinIndicator.Text = _coinCounts.ToString();
    }

    public override void _Process(double delta)
    {
        var move = Input.GetVector("move_left", "move_right", "move_up", "move_down");

        Velocity = move * 100;

        if (Velocity.X != 0 || Velocity.Y != 0)
        {
            _animator.Play("run");

            if (Velocity.X < 0 || Velocity.Y > 0)
            {
                _animator.FlipH = true;
            }
            else
            {
                _animator.FlipH = false;
            }
        }
        else
        {
            _animator.Play("default");
        }

        MoveAndSlide();

        if (_xIndicator.Visible && Input.IsActionJustPressed("sell"))
        {
            if (_npc is null) return;

            if (_npc is IBuyer buyer)
            {
                buyer.Buy(this);
            }

            if (_npc is IThief thief)
            {
                thief.Steal(this);
            }
        }

        if (Input.IsActionJustPressed("show_resume"))
        {
            GD.Print("Imprimir transacciones");
        }
    }

    private void _InteractWithNPC(Node2D body)
    {
        if (body == this) return;

        _xIndicator.Visible = true;

        _npc = body;
    }

    private void _ExitNPC(Node2D body)
    {
        _xIndicator.Visible = false;
        _npc = null;
    }

    public void DiscountWood()
    {
        if (WoodCounts <= 0) return;

        WoodCounts--;
        _woodIndicator.Text = WoodCounts.ToString();
    }

    public void IncreaseCoin()
    {
        _coinCounts++;
        _coinIndicator.Text = _coinCounts.ToString();
    }
}
