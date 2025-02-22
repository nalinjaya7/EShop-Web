using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EShopWeb.Filters
{
    public class CryptoBindingSource
    {
        public static readonly BindingSource Crypto = new("Crypto", "BindingSource_Crypto", isGreedy: false, isFromRequest: true);
    }
}
