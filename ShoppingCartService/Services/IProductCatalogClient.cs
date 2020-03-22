
namespace ShoppingCartService.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using ShoppingCartService.Models.Dto;
    using ShoppingCartService.Models.MetaModels;

    public interface IProductCatalogClient
    {
        Task<IEnumerable<Item>> GetShoppingCartItems(AddItem[] addItems);
    }
}