namespace ShoppingCartService.Models.Dto
{
    public class ItemFormat
    {
        public ItemUnit Unit { get; set; }

        public ItemPrice Price { get; set; }

        public ItemFormat(ItemUnit unit, ItemPrice price)
        {
            Unit = unit;
            Price = price;
        }
    }
}