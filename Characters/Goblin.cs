using Godot;
using System;

public partial class Goblin : StaticBody2D
{
    private AnimatedSprite2D _animator;
    public override void _Ready()
	{
        _animator = GetNode<AnimatedSprite2D>("Animator");
        _animator.Play("default");
    }

	public override void _Process(double delta)
	{
	}
}
