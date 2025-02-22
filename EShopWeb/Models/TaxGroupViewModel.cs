using EShopModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using EShopWeb.Common;

namespace EShopWeb.Models
{
    public class TaxGroupViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public TaxGroupViewModel(int iD, string taxCode, string description, decimal taxRate,byte[] rowVersion, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.TaxCode = taxCode;
            this.Description = description;
            this.TaxRate = taxRate;
            this.RowVersion = rowVersion;
            _protector = cryptoParamsProtector;
        }

        [Required]
        private int ID { get; set; }
        [Required]
        [StringLength(maximumLength: 20)]
        [Display(Name = "Tax Code")]
        public string TaxCode { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        [StringLength(2000)]
        public string Description { get; set; }
        [Required]
        [Range(0.01, 100.00)]
        public decimal TaxRate { get; set; }

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
