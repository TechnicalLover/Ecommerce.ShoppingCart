namespace ShoppingCartService.Models.Dto
{
    using System.Collections.Generic;
    using System.Linq;
    using ShoppingCartService.EventFeed;

    public class ShoppingCart
    {
        private HashSet<Item> items = new HashSet<Item>();

        public int UserId { get; set; }
        public IEnumerable<Item> Items { get { return items; } }

        public ShoppingCart(int userId, HashSet<Item> items)
        {
            this.items = items;
            UserId = userId;
        }

        public void AddItems(IEnumerable<Item> items, IEventStore eventStore)
        {
            foreach (var item in items)
            {
                if (this.items.Add(item))
                {
                    eventStore.Raise("ItemsAddedToCartEvent", new { UserId, item });
                }
            }
        }

        public void RemoveItems(int[] productCodes, IEventStore eventStore)
        {
            IEnumerable<Item> itemToRemove = this.items.Where(i => productCodes.Contains(i.ProductCode));
            foreach (var item in itemToRemove)
            {
                if (this.items.Remove(item))
                {
                    eventStore.Raise("ItemsRemovedFromCartEvent", new { UserId, item });
                }
            }
        }
    }
}