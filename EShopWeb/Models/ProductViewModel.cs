using EShopModels;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Drawing;
using System.Text;
using System.Drawing.Imaging;
using EShopWeb.Common;

namespace EShopWeb.Models
{
    public class ProductViewModel
    {
        private readonly CryptoParamsProtector _protector;
        public ProductViewModel(int iD, int? productSubCategoryID, int productCategoryID, string name, string barCode, string itemCode, int reOrderLevel, int? taxGroupID, bool taxInclude, decimal? taxRate, byte[]? ImageData, byte[] rowVersion, CryptoParamsProtector cryptoParamsProtector)
        {
            this.ID = iD;
            this.ProductSubCategoryID = productSubCategoryID;
            this.ProductCategoryID = productCategoryID;
            this.Name = name;
            this.BarCode = barCode;
            this.ItemCode = itemCode;
            this.ReOrderLevel = reOrderLevel;
            this.TaxGroupID = taxGroupID;
            this.TaxInclude = taxInclude;
            this.TaxRate = taxRate;
            this.ProductImage = ImageData;
            this.RowVersion = rowVersion;
            this._protector = cryptoParamsProtector;
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
        [Display(Name = "Product SubCategory")]
        public int? ProductSubCategoryID { get; set; }
        [Required]
        [Display(Name = "Product Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Select Product Category")]
        public int ProductCategoryID { get; set; }
        [Required]
        [StringLength(200)]
        [Display(Name = "Product Name")]
        public string Name { get; set; }
        [Required]
        [StringLength(20)]
        public string BarCode { get; set; }
        [Required]
        [StringLength(20)]
        public string ItemCode { get; set; }
        [Required]
        [Display(Name = "Re-Order Level")]
        public int ReOrderLevel { get; set; }
        [Required]
        [Display(Name = "Tax Include")]
        public bool TaxInclude { get; set; }
        [Display(Name = "Tax Rate")]
        public decimal? TaxRate { get; set; }
        [Display(Name = "Tax Group")]
        public int? TaxGroupID { get; set; }
 
        [NotMapped]
        public decimal SellingPrice
        {
            get
            {
                if (Inventories != null && Inventories.Count > 0)
                {
                    if (Inventories.Where(g => g.IsBaseUnitInventory).SingleOrDefault() != null)
                    {
                        return Inventories.Where(g => g.IsBaseUnitInventory).SingleOrDefault().SellingPrice;
                    }
                    else
                    {
                        return Inventories.FirstOrDefault().SellingPrice;
                    }
                }
                else
                {
                    return 0.0m;
                }
            }
        }

        [NotMapped]
        public bool IsInventoryAvailable
        {
            get
            {
                if(Inventories == null || Inventories.Count == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [NotMapped]
        public string EnID
        {
            get
            {
                Dictionary<string, string> ParaDictionary = new()
                {
                    { "ID", ID.ToString() }
                };
                return _protector.EncryptParamDictionary(ParaDictionary);
            }
        }

        [Display(Name = "Product Image")]
        [MaxLength(4000,ErrorMessage= "Image must be a string or array type with a maximum length of '4000'.")]
        public byte[]? ProductImage{get;set;}

        [Display(Name = "Product Image")]
        public string Image
        {
            get
            {
                if (ProductImage == null)
                {
                    byte[] imararrr = null;
                    using (var resource = new MemoryStream())
                    {
                        Bitmap bitmap = new(150, 150, System.Drawing.Imaging.PixelFormat.Format64bppArgb);
                        Graphics graphics = Graphics.FromImage(bitmap);
                        graphics.Clear(Color.FromArgb(230, 230, 230));
                        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                        graphics.DrawString("No Image", new Font("Arial", 15, FontStyle.Regular), new SolidBrush(Color.FromArgb(0,0,0)),
                            new PointF(25F,60F));
                        graphics.Flush();
                        graphics.Dispose();
                        bitmap.Save(resource, ImageFormat.Jpeg);
                        bitmap.Dispose();
                        imararrr = resource.ToArray();
                    }
                    return "data:image/png;base64," + Convert.ToBase64String(imararrr);
                }
                else
                {
                    return "data:image/png;base64," + Convert.ToBase64String(ProductImage);
                }
            }
        }

        public virtual TaxGroupViewModel? TaxGroup { get; set; }
        public virtual ProductSubCategoryViewModel? ProductSubCategory { get; set; }
        [ValidateNever]
        public virtual ProductCategoryViewModel ProductCategory { get; set; }
        [ValidateNever]
        public virtual List<InventoryViewModel> Inventories { get; set; }
        [ValidateNever]
        public virtual List<UnitChartViewModel> UnitCharts { get; set; } 
        [ValidateNever]
        public virtual List<ProductPromotionDetailViewModel> ProductPromotionDetails { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {           
            if (UnitCharts == null || UnitCharts.Count == 0)
            {
                yield return new ValidationResult("Please add some UnitCharts");
            }
            if (ProductCategoryID == 0)
            {
                yield return new ValidationResult("Please select product category");
            }
            if (ProductSubCategoryID == 0)
            {
                yield return new ValidationResult("Please select product sub category");
            }
            validationContext.Items["UnitCharts"] = UnitCharts;
        }
    }
}
