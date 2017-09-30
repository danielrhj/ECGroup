using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace ECGroup
{
    public class WebApiDisposeFilter : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            base.OnActionExecuted(actionExecutedContext);
            GC.Collect();
        }
    }
}