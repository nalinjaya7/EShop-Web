using EShopModels;
using EShopModels.Common;
using EShopWeb.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShopWeb.Models
{
    public class OrderUserModel
    {
        readonly CryptoParamsProtector _protector;
        public OrderUserModel(int userID,int orderID,string userName,decimal total,DateTime orderDate, decimal discount, decimal due,string orderNo, bool isDeleted, OrderStatus orderStatus, CryptoParamsProtector cryptoParamsProtector)
        {
            this.UserID = userID;
            this.OrderID = orderID;
            this.UserName = userName;
            this.Total = total;
            this.OrderDate = orderDate;
            this.Discount = discount;
            this.Due = due;
            this.OrderNo = orderNo;
            this.IsDeleted = isDeleted;
            this.OrderStatus = orderStatus;
            _protector = cryptoParamsProtector; 
        }

        [NotMapped]
        public string EnID
        {
            get
            {
                Dictionary<string, string> ParaDictionary = new()
                {
                    { "ID", this.UserID.ToString() }
                };
                return _protector.EncryptParamDictionary(ParaDictionary);
            }
        }
        private int UserID { get; set; }
        private int OrderID { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public decimal Total { get; set; }
        [Required]
        public DateTime OrderDate { get; set; }
        [Required]
        public decimal Discount { get; set; }
        [Required]
        public decimal Due { get; set; }
        [Required]
        public string OrderNo { get; set; }
        [Required]
        public bool IsDeleted { get; set; }
        [Required]
        public OrderStatus OrderStatus { get; set; }
        [NotMapped]
        public string OrderStatusText
        {
            get
            {
                return this.OrderStatus.ToString();
            }
        }
    }
}