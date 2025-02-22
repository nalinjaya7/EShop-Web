using Microsoft.AspNetCore.DataProtection;

namespace EShopWeb.Common
{
    public class CryptoParamsProtector
    {
        private readonly IDataProtector _protector;
        public CryptoParamsProtector(IDataProtectionProvider dataProtectionProvider)
        {
            _protector = dataProtectionProvider.CreateProtector(GetType().FullName);
        }

        public string EncryptParamDictionary(Dictionary<string, string> keyValues)
        {
            var paramsInSingleString = string.Join("+", keyValues.Select(p => string.Format("{0}={1}", p.Key.ToLower(), p.Value)));
            return _protector.Protect(paramsInSingleString);
        }

        public Dictionary<string, string> DecryptToParamDictionary(string encryptedParameter)
        {
            var paramsInSingleString = string.Empty;
            try
            {
                paramsInSingleString = _protector.Unprotect(encryptedParameter);
            }
            catch (Exception)
            {
                return new Dictionary<string, string>();
            }
            return paramsInSingleString.Split('+').Select(p => p.Split('=')).ToDictionary(p => p[0], p => p[1]);
        }
    }
}
