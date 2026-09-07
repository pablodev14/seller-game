public interface IBuyer
{
    int Price { get; }
    bool Buy(Seller seller);
}
