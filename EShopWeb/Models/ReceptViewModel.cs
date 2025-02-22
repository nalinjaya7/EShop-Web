using EShopModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EShopWeb.Common;

namespace EShopWeb.Models
{
    public class ReceptViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public ReceptViewModel(int iD, int paymentID, int EShopUserID, string code, decimal settledAmount,byte[] rowVersion, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.PaymentID = paymentID;
            this.EShopUserID = EShopUserID;
            this.Code = code;
            this.SettledAmount = settledAmount;
            this.RowVersion = rowVersion;
            _protector = cryptoParamsProtector;
        }
        [Required]
        private int ID { get; set; }
        [Required]
        [StringLength(15)]
        public string Code { get; set; }
        [Required]
        public int PaymentID { get; set; }
        [Required]
        public int EShopUserID { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public decimal SettledAmount { get; set; }
        [ValidateNever]
        public List<ReceptDetailViewModel> ReceptDetails { get; set; }
        [ValidateNever]
        public PaymentViewModel Payment { get; set; }
        [ValidateNever]
        public EShopUserViewModel EShopUser { get; set; }

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
