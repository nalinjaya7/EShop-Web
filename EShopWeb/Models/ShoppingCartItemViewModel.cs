using EShopModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using EShopWeb.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EShopWeb.Models
{
    public class ShoppingCartItemViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public ShoppingCartItemViewModel(int iD, int shoppingCartID, int productID, int unitChartID, decimal unitPrice, decimal lineDiscount, decimal quantity, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.ShoppingCartID = shoppingCartID;
            this.ProductID = productID;
            this.UnitChartID = unitChartID;
            this.UnitPrice = unitPrice;
            this.LineDiscount = lineDiscount;
            this.Quantity = quantity;
            _protector = cryptoParamsProtector;
        }
        [ValidateNever]
        public bool? editrow { get; set; } = false;
        [Required]
        private int ID { get; set; }
        [Required]
        public int ShoppingCartID { get; set; }
        [Required]
        public int UnitChartID { get; set; }
        [Required]
        public int ProductID { get; set; }
        [Required]
        public decimal UnitPrice { get; set; }
        [Required]
        public decimal LineDiscount { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [NotMapped]
        public string EnID
        {
            get
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("ID", ID.ToString());
                return _protector.EncryptParamDictionary(keyValuePairs);
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
        public string UnitChatName
        {
            get
            {
                if(this.UnitChart != null)
                {
                    return this.UnitChart.UnitChartName;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        public string ProductName
        {
            get
            {
                if(this.Product != null)
                {
                    return this.Product.Name;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        [ValidateNever]
        public ProductViewModel Product { get; set; }
        [ValidateNever]
        public UnitChartViewModel UnitChart { get; set; }
        [ValidateNever]
        public ShoppingCartViewModel ShoppingCart { get; set; }
        
        
    }
}
