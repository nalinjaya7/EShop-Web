using Microsoft.AspNetCore.Http;

namespace EShopWeb.Models
{
    public class ProductUploadModel
    {
        public ProductUploadModel(int iD, IFormFile httpPostedFileBase)
        {
            this.ID = iD;
            this.HttpPostedFileBase = httpPostedFileBase;
        }

        public int ID { get; set; }
        public IFormFile HttpPostedFileBase { get; set; }
    }    
}