using System.ComponentModel.DataAnnotations.Schema;

namespace ReStore.Entities
{
    [Table("BasketItems")]
    public class BasketItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }

        //Navigation Properties
        public int ProductId { get; set; }
        public Product Product { get; set; }//(one to one relationship)

        public int BasketId { get; set; }
        public Basket Basket { get; set; }
    }
}