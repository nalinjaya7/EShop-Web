using EShopWeb.Common;
using System.ComponentModel.DataAnnotations;

namespace EShopWeb.Models
{
    public class BatchViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public BatchViewModel(string code)
        {
            this.Code = code;
        }

        [Required]
        [StringLength(15)]
        public string Code { get; set; }


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
    }
}
