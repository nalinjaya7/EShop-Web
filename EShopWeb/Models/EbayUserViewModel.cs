using EShopModels;
using EShopModels.Common;
using EShopWeb.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShopWeb.Models
{
    public class EShopUserViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public EShopUserViewModel(int iD, string userName, string firstName, string lastName, string address, string email, bool isActive, Guid activationCode, Roles roleName,byte[] rowVersion, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.UserName = userName;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Address = address;
            this.Email = email;
            this.IsActive = isActive;
            this.ActivationCode = activationCode; 
            this.RoleName = roleName;
            this.RowVersion = rowVersion;
            _protector = cryptoParamsProtector;
        }

        [Required]
        private int ID { get; set; }
        [Required]
        [StringLength(20)]
        [Display(Name = "User Name")]
        public string UserName { get; set; }
        [Required]
        [StringLength(200)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [StringLength(200)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string Address { get; set; }
        [Required]
        [StringLength(250)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }
        [NotMapped]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public bool IsActive { get; set; }
        public Guid ActivationCode { get; set; }
 
        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        [NotMapped]
        public string EnID
        {
            get
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>
                {
                    { "ID", ID.ToString() }
                };
                return this._protector.EncryptParamDictionary(keyValuePairs);
            }
        }
        public virtual Roles RoleName { get; set; }
       
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
        public virtual ContactViewModel Contact { get; set; } 
        [ValidateNever]
        public virtual List<CreditNoteViewModel> CreditNotes { get; set; }
    }
}
