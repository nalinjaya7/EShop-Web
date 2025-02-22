namespace EShopWeb.Models
{
    public class AddToCartModel
    {
        public AddToCartModel(int unitChartID, decimal quantity,int inventoryID)
        {
            this.UnitChartID = unitChartID; 
            this.Quantity = quantity;
            this.InventoryID = inventoryID;
        }
        public int InventoryID { get;set;}
        public int UnitChartID { get; set; }
        public int ProductID { get; set; } 
        public decimal Quantity { get; set; }
        public ProductViewModel ProductViewModel { get; set; }
        public UnitChartViewModel UnitChartViewModel { get; set; }
    }
}
