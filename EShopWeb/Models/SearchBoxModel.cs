using EShopModels;
using EShopModels.Common;
using EShopWeb.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShopWeb.Models
{
    public class SearchBoxModel
    {
        private readonly CryptoParamsProtector _cryptoParams;
        public SearchBoxModel(int iD,string name, SearchBoxType type,CryptoParamsProtector cryptoParams)
        {
            ID = iD;
            Name = name;
            Type = type;
            _cryptoParams = cryptoParams;
        }
        [Required]
        private int ID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public SearchBoxType Type { get; set; }
        [Required]
        public List<Price> Prices { get; set; }
        [NotMapped]
        public string EnID {
            get
            {
                Dictionary<string,string> dic = new Dictionary<string,string>();
                dic.Add("ID",ID.ToString());    
                return this._cryptoParams.EncryptParamDictionary(dic);
            }
        }
    }
}
