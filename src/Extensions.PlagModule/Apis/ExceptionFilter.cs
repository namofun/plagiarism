using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace SatelliteSite.PlagModule.Apis
{
    public class CustomedExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case ArgumentOutOfRangeException _:
                    context.Result = new BadRequestResult();
                    context.ExceptionHandled = true;
                    break;
            }
        }
    }
}
