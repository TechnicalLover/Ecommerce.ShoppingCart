using System.Linq;

namespace ShoppingCartService.Services.Response
{
    public class ProductCatalogResponse
    {
        public int Id { get; set; }

        public string ProductName { get; set; }

        public int DivisionCode { get; set; }

        public string DivisionName { get; set; }

        public int CategoryCode { get; set; }

        public string CategoryName { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public ProductFormatResponse CaseFormat { get; set; }

        public ProductFormatResponse BundleFormat { get; set; }

        public ProductFormatResponse UpcFormat { get; set; }

        public ProductFormatResponse GetFormat(int unitCode)
        {
            if (BundleFormat.Unit.Id == unitCode)
            {
                return BundleFormat;
            }

            if (UpcFormat.Unit.Id == unitCode)
            {
                return UpcFormat;
            }

            // if unitCode is not valid, return CaseFormat by default
            return CaseFormat;
        }
    }
}