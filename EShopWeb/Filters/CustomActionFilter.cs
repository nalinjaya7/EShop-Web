using Microsoft.AspNetCore.Mvc.Filters;

namespace EShopWeb.Filters
{
    public class CustomActionFilter : ActionFilterAttribute
    {
        private readonly ILogger _logger;
        public CustomActionFilter(ILogger<CustomActionFilter> logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {            
           // _logger.LogWarning("Controller Name : "+context.Controller.GetType().Name+", Action Name : "+context.ActionDescriptor.DisplayName + " Inside OnActionExecuted method");
            base.OnActionExecuted(context);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
           // _logger.LogWarning("Controller Name : " + context.Controller.GetType().Name + ", Action Name : " + context.ActionDescriptor.DisplayName + " Inside OnActionExecuting method");
            base.OnActionExecuting(context);
        }

        public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //_logger.LogWarning("Controller Name : " + context.Controller.GetType().Name + ", Action Name : " + context.ActionDescriptor.DisplayName + " Inside OnActionExecutionAsync method");
            return base.OnActionExecutionAsync(context, next);
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
           // _logger.LogWarning("Controller Name : " + context.Controller.GetType().Name + ", Action Name : " + context.ActionDescriptor.DisplayName + " Inside OnResultExecuted method");
            base.OnResultExecuted(context);
        }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            //_logger.LogWarning("Controller Name : " + context.Controller.GetType().Name + ", Action Name : " + context.ActionDescriptor.DisplayName + " Inside OnResultExecuting method");
            base.OnResultExecuting(context);
        }

        public override Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
           // _logger.LogWarning("Controller Name : " + context.Controller.GetType().Name + ", Action Name : " + context.ActionDescriptor.DisplayName + " Inside OnResultExecutionAsync method");
            return base.OnResultExecutionAsync(context, next);
        }

        public override bool IsDefaultAttribute()
        {
            return base.IsDefaultAttribute();
        }
    }
}
