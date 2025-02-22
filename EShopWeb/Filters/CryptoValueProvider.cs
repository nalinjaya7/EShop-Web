using EShopWeb.Common;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Primitives;

namespace EShopWeb.Filters
{
    public class CryptoValueProvider : BindingSourceValueProvider
    {
        private readonly string _encryptedParameters;
        private readonly CryptoParamsProtector _protector;
        private Dictionary<string, string> _values;
        public CryptoValueProvider(BindingSource bindingSource,CryptoParamsProtector protector,string encryptedParameters) : base(bindingSource)
        {
            _encryptedParameters = encryptedParameters;
            _protector = protector;
        }

        public override bool ContainsPrefix(string prefix)
        {
           if(_values == null)
            {
                if (string.IsNullOrEmpty(_encryptedParameters))
                {
                    _values = new Dictionary<string, string>();
                }
                else
                {
                    _values = _protector.DecryptToParamDictionary(_encryptedParameters);
                }
            }
            return _values.ContainsKey(prefix.ToLower());
        }

        public override ValueProviderResult GetValue(string key)
        {
            if (_values.ContainsKey(key.ToLower()))
            {
                return new ValueProviderResult(new StringValues(_values[key.ToLower()]));
            }
            else
            {
                return ValueProviderResult.None;
            }
        }
    }
}
