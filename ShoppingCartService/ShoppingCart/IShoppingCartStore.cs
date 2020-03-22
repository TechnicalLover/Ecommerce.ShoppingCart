namespace ShoppingCartService.ShoppingCart
{
    using System.Threading.Tasks;
    using ShoppingCartService.Models.Dto;

    public interface IShoppingCartStore
    {
        Task<ShoppingCart> Get(int userId);

        Task Save(ShoppingCart shoppingCart);
    }
}