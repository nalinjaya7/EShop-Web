namespace EShopWeb.Models
{
    public class ShoppingItemQtyUpdate
    {        
        public int Id { get; set; }
        public int ProductId { get; set; } = 0;
        public int UnitChartID {  get; set; }
        public decimal OldQuantity { get; set; }
        public decimal NewQuantity { get; set; } 

    }
}
