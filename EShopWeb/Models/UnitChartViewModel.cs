using EShopModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EShopWeb.Common;

namespace EShopWeb.Models
{
    public class UnitChartViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public UnitChartViewModel(int iD, int unitTypeID, int productID, decimal quantity, string unitChartName,byte[] rowVersion, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.UnitTypeID = unitTypeID;
            this.ProductID = productID;
            this.Quantity = quantity;
            this.UnitChartName = unitChartName;
            this.RowVersion = rowVersion;
            _protector = cryptoParamsProtector;
        }

        [Required]
        private int ID { get; set; }
        [Required]
        [Display(Name = "Unit Type")]
        public virtual int UnitTypeID { get; set; }
        [Required]
        [Display(Name = "Product")]
        public virtual int ProductID { get; set; }
        [Required]
        public decimal Quantity { get; set; }
        [Display(Name = "Unit Name")]
        public string UnitTypeName
        {
            get
            {
                return (UnitType == null) ? "" : UnitType.Name;
            }
        }

        [Display(Name = "Unit Chart Name")]
        [StringLength(100)]
        public string UnitChartName { get; set; }
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
        [ValidateNever]
        public virtual UnitTypeViewModel UnitType { get; set; }
        [ValidateNever]
        public virtual ProductViewModel Product { get; set; }
    }
}
