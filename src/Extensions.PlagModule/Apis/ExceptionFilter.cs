using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Net.Http;

namespace SatelliteSite.PlagModule
{
    internal class CustomedExceptionFilter : ExceptionFilterAttribute
    {
        private readonly bool _process400;

        public CustomedExceptionFilter(bool process400 = true)
        {
            _process400 = process400;
        }

        public override void OnException(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case ArgumentOutOfRangeException _ when _process400:
                    context.Result = new BadRequestResult();
                    context.ExceptionHandled = true;
                    break;

                case HttpRequestException _:
                    context.Result = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
                    context.ExceptionHandled = true;
                    break;
            }
        }
    }
}
