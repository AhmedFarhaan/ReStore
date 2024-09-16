using System.Collections.Generic;
namespace ReStore.Entities
{
    public class Basket
    {
        public int Id { get; set; }
        public string BuyerId { get; set; }
        public List<BasketItem> Items { get; set; } = new();//initialize new list of basket items(one to many relationship)
        public string PaymentIntetId { get; set; }
        public string ClientSecret {  get; set; }
        //Method for adding product to Basket
        public void AddItem(Product product, int quantity)
        {
            if (Items.All(item => item.ProductId != product.Id))
            {
                Items.Add(new BasketItem { Product =product,Quantity=quantity});
            }
            var existtingItems = Items.FirstOrDefault(item => item.ProductId == product.Id);
            if (existtingItems != null) existtingItems.Quantity += quantity;
        }

        //Method for removing product from Basket
        public void RemoveItem(int productId, int quantity)
        {
            var item = Items.FirstOrDefault(item => item.ProductId == productId);//getting item first
            if (item == null) return;//if item not exist dont do enything
            item.Quantity -= quantity;
            if (item.Quantity == 0) Items.Remove(item);
        }
    }
}
