using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CafeWeb.Static
{
    public class RedirectOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            // Проверяем, что это не POST запрос и не редирект
            if (httpContext.Request.Method != "POST" &&
                !httpContext.Response.Headers.ContainsKey("Location"))
            {
                // Проверяем наличие специального маркера в TempData
                var tempData = context.Controller as Controller;
                if (tempData?.TempData["RedirectOnly"] == null)
                {
                    context.Result = new RedirectToActionResult("Index", "Cafe", null);
                    return;
                }

                // Очищаем после использования
                tempData.TempData.Remove("RedirectOnly");
            }

            base.OnActionExecuting(context);
        }
    }
}
