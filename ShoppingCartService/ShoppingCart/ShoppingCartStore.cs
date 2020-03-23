namespace ShoppingCartService.ShoppingCart
{
    using System;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using Dapper;
    using ShoppingCartService.Models.Configurations;
    using ShoppingCartService.Models.Dto;

    public class ShoppingCartStore : IShoppingCartStore
    {
        private readonly string _connectionString;

        public ShoppingCartStore(ShoppingCartStoreConfig config)
        {
            _connectionString = config.ConnectionString;
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new ArgumentException(nameof(config.ConnectionString));
        }

        public async Task<ShoppingCart> Get(int userId)
        {
            string readShoppingCartSql = @"SELECT * FROM ShoppingCart WHERE ShoppingCart.UserId = @UserId";
            string addShoppingCartSql = @"INSERT INTO ShoppingCart (UserId) VALUES (@UserId)";
            string readItemsSql = @"SELECT * FROM ShoppingCartItems 
                WHERE ShoppingCartItems.ShoppingCartId = @ShoppingCartId";

            using (var conn = new SqlConnection(_connectionString))
            using (var tx = conn.BeginTransaction())
            {
                var shoppingCart = (await conn.QueryAsync<ShoppingCart>(
                    readShoppingCartSql,
                    new { UserId = userId },
                    tx).ConfigureAwait(false))
                    .SingleOrDefault();
                if (shoppingCart == null)
                {
                    await conn.ExecuteAsync(
                        addShoppingCartSql,
                        new { UserId = shoppingCart.UserId },
                        tx).ConfigureAwait(false);
                    shoppingCart = (await conn.QueryAsync<ShoppingCart>(
                        readShoppingCartSql,
                        new { UserId = userId },
                        tx).ConfigureAwait(false))
                        .Single();
                    return shoppingCart;
                }

                var items = await conn.QueryAsync<Item>(
                    readItemsSql,
                    new { ShoppingCartId = shoppingCart.Id },
                    tx).ConfigureAwait(false);

                await tx.CommitAsync();
                return new ShoppingCart(shoppingCart.Id, userId, items.ToHashSet());
            }
        }

        public async Task Save(ShoppingCart shoppingCart)
        {
            string deleteAllItemsForShoppingCartSql = @"DELETE item FROM ShoppingCartItems item
                INNER JOIN ShoppingCart cart ON item.ShoppingCartId = cart.ID
                AND cart.UserId=@UserId";
            string addAllForItemsShoppingCartSql = @"INSERT INTO ShoppingCartItems 
                (ShoppingCartId, ProductCode, ProductName, 
                UnitCode, UnitName, Amount, Currency, Quantity, Description)
                VALUES 
                (@ShoppingCartId, @ProductCode, @ProductName, 
                @UnitCode, @UnitName, @Amount, @Currency, @Quantity, @Description)";

            // ! this code is not for production
            using (var conn = new SqlConnection(_connectionString))
            using (var tx = conn.BeginTransaction())
            {
                await conn.ExecuteAsync(
                    deleteAllItemsForShoppingCartSql,
                    new { UserId = shoppingCart.UserId },
                    tx).ConfigureAwait(false);

                await conn.ExecuteAsync(
                    addAllForItemsShoppingCartSql,
                    shoppingCart.Items.Select(item =>
                        new
                        {
                            ShoppingCartId = shoppingCart.Id,
                            ProductCode = item.ProductCode,
                            ProductName = item.ProductName,
                            UnitCode = item.UnitCode,
                            UnitName = item.UnitName,
                            Amount = item.Amount,
                            Currency = item.Currency,
                            Quantity = item.Quantity,
                            Description = item.Description
                        }),
                    tx).ConfigureAwait(false);

                await tx.CommitAsync();
            }
        }
    }
}