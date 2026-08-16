using Microsoft.AspNetCore.Mvc.Filters;
namespace wwfpp.Services
{
    public class NoCacheFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var response = context.HttpContext.Response;
            response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
            response.Headers["Pragma"] = "no-cache";
            response.Headers["Expires"] = "0";
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // nothing needed here
        }
    }


}
