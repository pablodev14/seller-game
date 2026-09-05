using Godot;
using System;

public partial class Lancer : StaticBody2D
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

    public void Buy(Seller seller)
    {
        if (seller.WoodCounts <= 0) return;

        seller.DiscountWood();
        seller.IncreaseCoin();
    }
}
