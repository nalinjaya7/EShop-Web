using EShopModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using EShopWeb.Common;
using EShopModels.Common;

namespace EShopWeb.Models
{
    public class OrderViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public OrderViewModel(int iD, string orderNo, int EShopUserID, decimal total, DateTime salesDate, OrderStatus orderStatus, decimal discount, byte[] rowVersion,CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.OrderNo = orderNo;
            this.EshopViewUserID = EShopUserID; 
            this.Total = total;
            this.SalesDate = salesDate;
            this.OrderStatus = orderStatus;
            this.Discount = discount;
            this.RowVersion = rowVersion;
            OrderDetails = new List<OrderDetailViewModel>();
            _protector = cryptoParamsProtector;
        }

        [Required]
        private int ID { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNo { get; set; }
        [Required]
        [Display(Name = "EshopViewUser")]
        public int EshopViewUserID { get; set; }
        [Required]
        [Column(TypeName = "Money")]
        [DisplayFormat(DataFormatString = "{0:c2}")]
        public decimal Total { get; set; }
        [Required]
        public DateTime SalesDate { get; set; }
        [Required]
        [Display(Name = "Status")]
        public OrderStatus OrderStatus { get; set; }
       
        [Required]
        [Column(TypeName = "Money")]
        [DisplayFormat(DataFormatString = "{0:c2}")]
        public decimal Discount { get; set; }
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
        public string OrderStatusText
        {
            get
            {
                return OrderStatus.ToString();
            }
        }
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
        public virtual EShopUserViewModel EshopViewUser { get; set; }
        [ValidateNever]
        public virtual List<OrderDetailViewModel> OrderDetails { get; set; }
    }
}
