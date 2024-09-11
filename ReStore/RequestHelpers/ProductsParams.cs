namespace ReStore.RequestHelpers
{
    public class ProductsParams :PaginationParams
    {
        public string OrderBy { get; set; }
        public string SearchTerm { get; set; }
        public string Brands { get; set; }
        public string Types { get; set; }
    }
}
