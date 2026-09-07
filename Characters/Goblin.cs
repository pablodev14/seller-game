public partial class Goblin : NPC, IThief
{
    public bool Steal(Seller seller)
    {
        return seller.TryDiscountWood();
    }
}