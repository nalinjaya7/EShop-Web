using EShopWeb.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShopWeb.Models
{
    public class CreditNoteViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public CreditNoteViewModel(int iD, string code, int EShopUserID, decimal creditValue, decimal deductValue, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.Code = code;
            this.EshopViewUserID = EShopUserID;
            this.CreditValue = creditValue;
            this.DeductValue = deductValue;
            this._protector = cryptoParamsProtector;
        }

        [Required]
        private int ID { get; set; }
        [Required]
        [StringLength(15)]
        public string Code { get; set; }
        [Required]
        public int EshopViewUserID { get; set; }
        [Required]
        [Column(TypeName = "Money")]
        [DisplayFormat(DataFormatString = "{0:c2}")]
        public decimal CreditValue { get; set; }
        [Required]
        [Column(TypeName = "Money")]
        [DisplayFormat(DataFormatString = "{0:c2}")]
        public decimal DeductValue { get; set; }
        [NotMapped]
        public string EnID
        {
            get
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                keyValuePairs.Add("ID", this.ID.ToString());
                return this._protector.EncryptParamDictionary(keyValuePairs);
            }
        }
        [ValidateNever]
        public virtual EShopUserViewModel EshopViewUser { get; set; }
    }
}
