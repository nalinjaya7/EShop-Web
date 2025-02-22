using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EShopWeb.Filters
{
    [AttributeUsage(AttributeTargets.Class)]
    public class FromCryptoAttribute : Attribute, IBindingSourceMetadata
    {
        public BindingSource BindingSource => CryptoBindingSource.Crypto;
    }
}
