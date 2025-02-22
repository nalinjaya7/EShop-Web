using EShopModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EShopWeb.Common;

namespace EShopWeb.Models
{
    public class ProductCategoryViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public ProductCategoryViewModel(int iD, string name,byte[] rowVersion, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.Name = name; 
            this.RowVersion = rowVersion;   
            _protector = cryptoParamsProtector;
        }
        [Required]
        private int ID { get; set; }
        [Required]
        [Display(Name = "Category Name")]
        [StringLength(100)]
        public string Name { get; set; }

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
        [ValidateNever]
        public virtual List<ProductSubCategoryViewModel> ProductSubCategories { get; set; }
    }
}
