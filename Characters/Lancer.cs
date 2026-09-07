using Godot;

public partial class Lancer : NPC, IBuyer
{
    [Export] private int _price = 1;

    public int Price => _price;

    public void Buy(Seller seller)
    {
        if (seller.WoodCounts <= 0) return;

        seller.DiscountWood();
        seller.IncreaseCoin();
    }
}
