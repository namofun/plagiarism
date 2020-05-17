using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Mvc
{
    public class ShowWindowResult : ViewResult
    {
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (!ViewData.ContainsKey("HandleKey2"))
                throw new InvalidOperationException();
            return base.ExecuteResultAsync(context);
        }
    }
}
