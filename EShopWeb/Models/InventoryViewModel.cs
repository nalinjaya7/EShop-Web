using EShopModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Xunit.Sdk;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EShopWeb.Common;

namespace EShopWeb.Models
{
    public class InventoryViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public InventoryViewModel(int iD, string code, int productID, int unitChartID, decimal quantity, int batchID, decimal sellingPrice, decimal purchasePrice, decimal reservedQuantity, byte[] RowVersion, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD; 
            this.Code = code;
            this.ProductID = productID;
            this.UnitChartID = unitChartID;
            this.Quantity = quantity;
            this.BatchID = batchID;
            this.SellingPrice = sellingPrice;
            this.PurchasePrice = purchasePrice;
            this.ReservedQuantity = reservedQuantity;
            this.RowVersion = RowVersion;
            _protector = cryptoParamsProtector;
        }

        [Required]
        private int ID { get; set; }
        [Required]
        [StringLength(15)]
        public string Code { get; set; }
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Product")]
        public int ProductID { get; set; }
        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "Unit Type")]
        [Range(1, int.MaxValue, ErrorMessage = "UnitType Not Selected")]
        public int UnitChartID { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal Quantity { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal SellingPrice { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public decimal PurchasePrice { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        [Display(Name = "Reserved Quantity")]
        public decimal ReservedQuantity { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime CreatedDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime ModifiedDate { get; set; }
        [Required]
        public bool IsDeleted { get; set; }
        [Timestamp]
        public byte[] RowVersion { get; set; }
        [Required]
        public int BatchID { get; set; }
 
        [NotMapped]
        public string ProductName
        {
            get
            {
                if (Product != null)
                {
                    return Product.Name;
                }
                else
                {
                    return "";
                }
            }
        }
        [NotMapped]
        public string UnitChartName
        {
            get
            {
                if (UnitChart != null)
                {
                    return UnitChart.UnitChartName;
                }
                else
                {
                    return "";
                }
            }
        }
        [ValidateNever]
        public virtual Product Product { get; set; }
        [ValidateNever]
        public virtual UnitChartViewModel UnitChart { get; set; }
        [ValidateNever]
        public virtual BatchViewModel Batch { get; set; }

        [NotMapped]
        public string EnID
        {
            get
            {
                Dictionary<string, string> ParaDictionary = new();
                ParaDictionary.Add("ID", this.ID.ToString());
                return _protector.EncryptParamDictionary(ParaDictionary);
            }
        }

        [NotMapped]
        public bool IsBaseUnitInventory
        {
            get
            {
                if (UnitChart != null && UnitChart.UnitType.IsBaseUnit)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
