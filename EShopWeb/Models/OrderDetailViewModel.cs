using EShopModels;
using EShopWeb.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShopWeb.Models
{
    public class OrderDetailViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public OrderDetailViewModel(int iD, int productID, int unitChartID, int orderID, decimal salesPrice, decimal costPrice, decimal lineAmount, decimal quantity, decimal lineDiscount,byte[] rowVersion, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.ProductID = productID;
            this.UnitChartID = unitChartID;
            this.OrderID = orderID;
            this.SalesPrice = salesPrice;
            this.CostPrice = costPrice;
            this.LineAmount = lineAmount;
            this.Quantity = quantity;
            this.LineDiscount = lineDiscount;
            this.RowVersion = rowVersion;
            _protector = cryptoParamsProtector;
        }

        [Required]
        private int ID { get; set; }
        [Required]
        [Display(Name = "Product")]
        public int ProductID { get; set; }
        [Required]
        [Display(Name = "UnitChart")]
        public int UnitChartID { get; set; }
        [Required]
        [Display(Name = "Order")]
        public int OrderID { get; set; }
        [Required]
        [Column(TypeName = "Money")]
        [DisplayFormat(DataFormatString = "{0:c2}")]
        public decimal SalesPrice { get; set; }
        [Required]
        [Column(TypeName = "Money")]
        [DisplayFormat(DataFormatString = "{0:c2}")]
        public decimal CostPrice { get; set; }
        [Required]
        [Column(TypeName = "Money")]
        [DisplayFormat(DataFormatString = "{0:c2}")]
        public decimal LineAmount { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:n2}")]
        public decimal Quantity { get; set; }
        [Required]
        [Column(TypeName = "Money")]
        [DisplayFormat(DataFormatString = "{0:c2}")]
        public decimal LineDiscount { get; set; }
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
                    return string.Empty;
                }
            }
        }
        [NotMapped]
        public string ProductItemCode
        {
            get
            {
                if (Product != null)
                {
                    return Product.ItemCode;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        [NotMapped]
        public string ProductBarCode
        {
            get
            {
                if (Product != null)
                {
                    return Product.BarCode;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        [NotMapped]
        public string UnitChartName
        {
            get
            {
                if (Product != null)
                {
                    return UnitChart.UnitChartName;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        [NotMapped]
        public decimal UnitChartQuantity
        {
            get
            {
                if (this.UnitChart != null)
                {
                    return this.UnitChart.Quantity;
                }
                else
                {
                    return 0.0m;
                }
            }
        }
        [ValidateNever]
        public virtual ProductViewModel Product { get; set; }
        [ValidateNever]
        public virtual UnitChartViewModel UnitChart { get; set; }
        [ValidateNever]
        public virtual OrderViewModel Order { get; set; }

        [NotMapped]
        public string EnID
        {
            get
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("ID", ID.ToString());
                return this._protector.EncryptParamDictionary(keyValuePairs);
            }
        }

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
    }
}
