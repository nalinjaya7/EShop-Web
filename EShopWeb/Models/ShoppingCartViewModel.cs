using EShopModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using EShopWeb.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace EShopWeb.Models
{
    public class ShoppingCartViewModel
    {
        private readonly CryptoParamsProtector? _protector;
        public ShoppingCartViewModel(int iD,int userID, decimal grossAmount, decimal discountAmount, decimal taxAmount, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.UserID = userID;
            this.GrossAmount = grossAmount;
            this.DiscountAmount = discountAmount;
            this.TaxAmount = taxAmount;
            _protector = cryptoParamsProtector;
            Items = new List<ShoppingCartItemViewModel>();
        }

        [Required]
        private int ID { get; set; }
        [Required]
        public int UserID { get; set; }
        [Required]
        public decimal GrossAmount { get; set; }
        [Required]
        public decimal DiscountAmount { get; set; }
        [Required]
        public decimal TaxAmount { get; set; }
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
        [ValidateNever]
        public EShopUser EShopUser { get; set; }
        [ValidateNever]
        public List<ShoppingCartItemViewModel> Items { get; set; }
        
    }
}
