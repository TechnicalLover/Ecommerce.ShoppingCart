namespace ShoppingCartService.Models.Dto
{
    public class Item
    {
        public int ProductCode { get; }

        public string ProductName { get; }

        public int UnitCode { get; }

        public string UnitName { get; }

        public string Currency { get; }

        public decimal Amount { get; }

        public int Quantity { get; private set; }

        public string Description { get; }

        public Item(int productCode, string productName, int unitCode, string unitName, string currency, decimal amount, int quantity, string description)
        {
            ProductCode = productCode;
            ProductName = productName;
            UnitCode = unitCode;
            UnitName = unitName;
            Currency = currency;
            Amount = amount;
            Quantity = quantity;
            Description = description;
        }

        public void SetQuantity(int quantity)
        {
            Quantity = quantity;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var that = obj as Item;
            return this.ProductCode.Equals(that.ProductCode)
                && this.UnitCode.Equals(that.UnitCode);
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return this.ProductCode.GetHashCode() + this.UnitCode.GetHashCode();
        }
    }
}