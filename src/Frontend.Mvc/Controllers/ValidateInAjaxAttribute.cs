using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Microsoft.AspNetCore.Mvc
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateInAjaxAttribute : Attribute, IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Here is nothing to do.
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!(context.Controller is Controller2 controller))
                throw new InvalidOperationException();
            if (!controller.IsWindowAjax)
                context.Result = controller.BadRequest();
        }
    }
}
