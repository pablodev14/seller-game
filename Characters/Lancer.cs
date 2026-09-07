using Godot;

public partial class Lancer : NPC, IBuyer
{
    [Export] private int _price = 1;

    public int Price => _price;

    public bool Buy(Seller seller)
    {
        if (!seller.TryDiscountWood())
        {
            return false;
        }

        seller.IncreaseCoin(Price);

        return true;
    }
}