public abstract class Transaction
{
    public NPC NPC { get; }

    public string NPCName { get; }

    public int TotalCounts { get; private set; }

    protected Transaction(NPC npc)
    {
        NPC = npc;
        NPCName = npc.Name;
    }

    public void IncreaseTotal()
    {
        TotalCounts++;
    }

    public abstract string TransactionType { get; }
}

public sealed class Sale : Transaction
{
    public Sale(NPC npc) : base(npc)
    {
    }

    public override string TransactionType => "Venta";
}

public sealed class Theft : Transaction
{
    public Theft(NPC npc) : base(npc)
    {
    }

    public override string TransactionType => "Robo";
}