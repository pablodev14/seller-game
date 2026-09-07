using Godot;

public partial class NPC : StaticBody2D
{
    protected AnimatedSprite2D _animator;

    public override void _Ready()
    {
        _animator = GetNode<AnimatedSprite2D>("Animator");
        _animator.Play("default");
    }

    public override void _Process(double delta)
    {
    }
}
