using Microsoft.AspNetCore.Mvc;

namespace wwfpp.Helpers
{
    public class SessionHelper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionHelper(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        private ISession Session => _httpContextAccessor.HttpContext.Session;

        // String helpers
        public void SetString(string key, string value) => Session.SetString(key, value);
        public string GetString(string key) => Session.GetString(key);

        // Int32 helpers
        public void SetInt32(string key, int value) => Session.SetInt32(key, value);
        public int? GetInt32(string key) => Session.GetInt32(key);

        // Remove
        public void Remove(string key) => Session.Remove(key);

    }

}
