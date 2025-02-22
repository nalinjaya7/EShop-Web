using EShopModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EShopWeb.Common;
using EShopModels.Common;

namespace EShopWeb.Models
{
    public class PaymentViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public PaymentViewModel(int iD, PaymentType paymentType, int EShopUserID, string code, decimal amount, decimal settleAmount, DateTime? chequeDate, DateTime? bankDate, string chequeNumber, string cardNumber, string bankName, string branchName,byte[] rowVersion, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.PaymentType = paymentType;
            this.EshopViewUserID = EShopUserID;
            this.Code = code;
            this.Amount = amount;
            this.SettleAmount = settleAmount;
            this.ChequeDate = chequeDate;
            this.BankDate = bankDate;
            this.ChequeNumber = chequeNumber;
            this.CardNumber = cardNumber;
            this.BankName = bankName;
            this.BranchName = branchName;
            this.RowVersion = rowVersion;
            _protector = cryptoParamsProtector;
        }

        [Required]
        private int ID { get; set; }

        [Required]
        [StringLength(15)]
        public string Code { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "PaymentType")]
        public PaymentType PaymentType { get; set; }

        [Required(ErrorMessage = "{0} is required")]
        [Display(Name = "EshopViewUser")]
        public int EshopViewUserID { get; set; }

        [Required]
        [Column(TypeName = "Money")]
        [DisplayFormat(DataFormatString = "{0:c2}")]
        public decimal Amount { get; set; }

        [Column(TypeName = "Money")]
        [DisplayFormat(DataFormatString = "{0:c2}")]
        public decimal? SettleAmount { get; set; }

        [Display(Name = "Cheque Date")]
        [DataType(DataType.Date)]
        public DateTime? ChequeDate { get; set; }

        [Display(Name = "Bank Date")]
        [DataType(DataType.Date)]
        public DateTime? BankDate { get; set; }

        [Display(Name = "Cheque Number")]
        [StringLength(22)]
        public string? ChequeNumber { get; set; }

        [Display(Name = "Card Number")]
        [StringLength(16)]
        public string? CardNumber { get; set; }

        [Display(Name = "Bank Name")]
        [StringLength(150)]
        public string? BankName { get; set; }

        [Display(Name = "Branch Name")]
        [StringLength(150)]
        public string? BranchName { get; set; }

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

        [NotMapped]
        public string PaymentTypeText
        {
            get
            {
                return this.PaymentType.ToString();
            }
        }
        [ValidateNever]
        public virtual EShopUserViewModel EShopViewUser { get; set; } 

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
