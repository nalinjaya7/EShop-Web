using EShopWeb.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShopWeb.Models
{
    public class ContactViewModel
    {
        private readonly CryptoParamsProtector _protector;
      
        public ContactViewModel(int iD, int eshopViewUserID, string email, string phone, string address,byte[] rowVersion, CryptoParamsProtector cryptoParamsProtector)
        {this.ID = iD;
            _protector = cryptoParamsProtector; 
            this.EshopViewUserID = eshopViewUserID;
            this.Email = email;
            this.Phone = phone;
            this.Address = address;
            this.RowVersion = rowVersion;
        }
        [Required]
        private int ID { get; set; }
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
        [Key]
        [Required]
        [ForeignKey("EshopViewUser")]
        public int EshopViewUserID { get; set; }
        [Required]
        [StringLength(256)]
        [DataType(DataType.EmailAddress, ErrorMessage = "Invalide email address")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Phone is required")]
        [StringLength(300, ErrorMessage = "Maximum length 300 characters")]
        public string Phone { get; set; }
        [Required]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }
        [ValidateNever]
        public virtual EShopUserViewModel EshopViewUser { get; set; }
        [NotMapped]
        public string EnID
        {
            get
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
                {
                    { "ID", this.ID.ToString() }
                };
                return this._protector.EncryptParamDictionary(keyValuePairs);
            }
        }
    }
}
