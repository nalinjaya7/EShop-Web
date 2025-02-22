using EShopModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EShopWeb.Common;
using EShopModels.Common;

namespace EShopWeb.Models
{
    public class ProductPromotionDetailViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public ProductPromotionDetailViewModel(int iD, int productPromotionID, int productID, PromotionApplyType applyType, decimal applyAmount, decimal discountAmount,
DiscountType discountType, int? discountProductID, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.ProductPromotionID = productPromotionID;
            this.ProductID = productID;
            this.ApplyType = applyType;
            this.ApplyAmount = applyAmount;
            this.DiscountAmount = discountAmount;
            this.DiscountType = discountType;
            this.DiscountProductID = discountProductID;
            _protector = cryptoParamsProtector;
        }

        [Required]
        private int ID { get; set; }
        [Required]
        public int ProductPromotionID { get; set; }
        [Required]
        public int ProductID { get; set; }
        [Required]
        [Display(Name = "Product ItemCode")]
        public string ProductItemCode { get; set; }
        [Required]
        [Display(Name = "Product BarCode")]
        public string ProductBarCode { get; set; }
        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }
        [Required]
        [Display(Name = "Apply Type")]
        public PromotionApplyType ApplyType { get; set; }
        [Required]
        [Display(Name = "Apply Amount")]
        public decimal ApplyAmount { get; set; }
        [Required]
        [Display(Name = "Discount Amount")]
        public decimal DiscountAmount { get; set; }
        [Required]
        [Display(Name = "Discount Type")]
        public DiscountType DiscountType { get; set; }
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
        [Display(Name = "Discount Product")]
        public int? DiscountProductID { get; set; }
        [ValidateNever]
        public ProductPromotionViewModel ProductPromotion { get; set; }
        [ValidateNever]
        public Product Product { get; set; }
        [ValidateNever]
        public Product? DiscountProduct { get; set; }
    }
}
