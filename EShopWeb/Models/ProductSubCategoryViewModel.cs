using EShopModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EShopWeb.Common;

namespace EShopWeb.Models
{
    public class ProductSubCategoryViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public ProductSubCategoryViewModel(int iD, int productCategoryID, string name,byte[] rowVersion, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.ProductCategoryID = productCategoryID;
            this.Name = name;           
            this.RowVersion = rowVersion;
            _protector = cryptoParamsProtector;
        }

        [Required]
        private int ID { get; set; }
        [Required]
        public int ProductCategoryID { get; set; }
        [Required]
        [Display(Name = "Sub Category Name")]
        [StringLength(100)]
        public string Name { get; set; }
        [NotMapped]
        public string EnID
        {
            get
            {
                Dictionary<string, string> keyValuePairs = new()
                {
                    { "ID", ID.ToString() }
                };
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
        [ValidateNever]
        public virtual ProductCategoryViewModel ProductCategory { get; set; }
        [ValidateNever]
        public virtual List<Product> Products { get; set; }
    }
}
