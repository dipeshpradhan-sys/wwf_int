using Microsoft.AspNetCore.Mvc;
namespace wwfpp.Controllers
{
    public class SessionController : Controller
    {
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetSession(string key, string value)
        {
            HttpContext.Session.SetString(key, value);
            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult GetSession(string key)
        {
            string? value = HttpContext.Session.GetString(key);
            return Json(new { value });
        }
    }

}
