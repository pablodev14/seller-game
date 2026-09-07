using Godot;
using System;
using System.Collections.Generic;

public partial class Seller : CharacterBody2D
{
    private const int InitialWoodCounts = 20;
    private const float MovementSpeed = 100f;

    private AnimatedSprite2D _animator;
    private TextureRect _xIndicator;
    private Label _woodIndicator;
    public int WoodCounts { get; private set; } = InitialWoodCounts;
    private Label _coinIndicator;
    private int _coinCounts = 0;
    private Node2D _npc;

    private readonly List<Transaction> _transactions = new();

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

        Velocity = move * MovementSpeed;

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

        HandleTransactionInput();
        HandleHistoryInput();
    }

    private void HandleTransactionInput()
    {
        if (!_xIndicator.Visible || !Input.IsActionJustPressed("sell"))
        {
            return;
        }

        if (_npc is null)
        {
            return;
        }

        if (_npc is not NPC npc)
        {
            return;
        }

        if (npc is IBuyer buyer)
        {
            if (buyer.Buy(this))
            {
                RegisterSale(npc);
            }

            return;
        }

        if (npc is IThief thief && thief.Steal(this))
        {
            RegisterTheft(npc);
        }
    }

    private void HandleHistoryInput()
    {
        if (Input.IsActionJustPressed("show_resume"))
        {
            PrintTransactionHistory();
        }
    }

    private void RegisterSale(NPC npc)
    {
        Transaction transaction = _transactions.Find(item => item.NPC == npc);

        if (transaction is null)
        {
            transaction = new Sale(npc);
            _transactions.Add(transaction);
        }

        transaction.IncreaseTotal();
    }

    private void RegisterTheft(NPC npc)
    {
        Transaction transaction = _transactions.Find(item => item.NPC == npc);

        if (transaction is null)
        {
            transaction = new Theft(npc);
            _transactions.Add(transaction);
        }

        transaction.IncreaseTotal();
    }

    private void PrintTransactionHistory()
    {
        if (_transactions.Count == 0)
        {
            GD.Print("No hay transacciones registradas.");
            return;
        }

        foreach (Transaction transaction in _transactions)
        {
            GD.Print(
                $"{transaction.NPCName} | " +
                $"{transaction.TransactionType} | " +
                $"Total: {transaction.TotalCounts}"
            );
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

    public bool TryDiscountWood()
    {
        if (WoodCounts <= 0)
        {
            return false;
        }

        WoodCounts--;
        _woodIndicator.Text = WoodCounts.ToString();

        return true;
    }

    public void IncreaseCoin(int amount)
    {
        _coinCounts += amount;
        _coinIndicator.Text = _coinCounts.ToString();
    }
}