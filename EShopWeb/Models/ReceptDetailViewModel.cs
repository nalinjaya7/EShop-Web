using EShopModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EShopWeb.Common;

namespace EShopWeb.Models
{
    public class ReceptDetailViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public ReceptDetailViewModel(int iD, int receptID, int salesOrderID, decimal settledAmount,byte[] rowVersion, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.ReceptID = receptID;
            this.SalesOrderID = salesOrderID;
            this.SettledAmount = settledAmount;
            this.RowVersion = rowVersion;
            _protector = cryptoParamsProtector;
        }

        [Required]
        private int ID { get; set; }
        [Required]
        public int ReceptID { get; set; }
        [Required]
        public int SalesOrderID { get; set; }
        [Required]
        [DataType(DataType.Currency)]
        public decimal SettledAmount { get; set; }
        [ValidateNever]
        public ReceptViewModel Recept { get; set; } 
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
    }
}
