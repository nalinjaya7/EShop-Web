using Microsoft.AspNetCore.Mvc.Filters;

namespace EShopWeb.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CryptoValueProviderAttribute : Attribute, IResourceFilter
    {
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            
        }

        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            context.ValueProviderFactories.Clear();
            context.ValueProviderFactories.Add(new CryptoValueProviderFactory());
        }         
    }
}
