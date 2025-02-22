using EShopModels;
using EShopModels.Common;
using EShopWeb.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EShopWeb.Models
{
    public class ProductPromotionViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public ProductPromotionViewModel(int iD, string code, string name, DateTime startDate, DateTime endDate, PromotionApplyType applyType, DayType dayType, DiscountType discountType,
    decimal applyAmount, decimal discountAmount, bool isEntireDay, TimeSpan startTime, TimeSpan endTime, string recurrnceArray, int? discountProductID,byte[] rowVersion, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.Code = code;
            this.Name = name;
            this.StartDate = startDate;
            this.EndDate = endDate;
            this.ApplyType = applyType;
            this.DayType = dayType;
            this.DiscountType = discountType;
            this.ApplyAmount = applyAmount;
            this.DiscountAmount = discountAmount;
            this.IsEntireDay = isEntireDay;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.RecurrnceArray = recurrnceArray;
            this.DiscountProductID = discountProductID;
            this.RowVersion = rowVersion; 
            ProductPromotionDetails = new List<ProductPromotionDetailViewModel>();
            _protector = cryptoParamsProtector;
        }
        [Required]
        private int ID { get; set; }
        [Required]
        [StringLength(15)]
        public string Code { get; set; }
        [Required]
        [StringLength(600)]
        public string Name { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
        [Display(Name = "Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:d}")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }
        [Required]
        [Display(Name = "Apply Type")]
        public PromotionApplyType ApplyType { get; set; }
        [Required]
        [Display(Name = "Day Type")]
        public DayType DayType { get; set; }
        [Required]
        [Display(Name = "Discount Type")]
        public DiscountType DiscountType { get; set; }
        [Required]
        [Display(Name = "Apply Amount")]
        [Range(minimum: 1.0, maximum: 100000.0)]
        public decimal ApplyAmount { get; set; }
        [Required]
        [Display(Name = "Discount Amount")]
        [Range(minimum: 1.0, maximum: 100000.0)]
        public decimal DiscountAmount { get; set; }
        [Required]
        [Display(Name = "Is EntireDay")]
        public bool IsEntireDay { get; set; }
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        [JsonConverter(typeof(TimeSpanToStringConverter))]
        public TimeSpan StartTime { get; set; }
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        [JsonConverter(typeof(TimeSpanToStringConverter))]
        public TimeSpan EndTime { get; set; }
        public string RecurrnceArray { get; set; }
        [Display(Name = "Discount Product")]
        public Nullable<int> DiscountProductID { get; set; }
        public bool IsExpired { get; set; }

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
        public ProductViewModel DiscountProduct { get; set; } 
        [ValidateNever]
        public List<ProductPromotionDetailViewModel> ProductPromotionDetails { get; set; }
    }
}
