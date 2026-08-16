using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Data;
using wwfpp.Data;
using wwfpp.Helpers;
using wwfpp.Models;

namespace wwfpp.Services
{
    public class AdminServices
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly SessionHelper _sessionHelper;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AdminServices(
            AppDbContext context,
            IOptions<AppSettings> appSettings,
            SessionHelper sessionHelper,
            GlobalOptionServices globalOptionServices,
            IHttpContextAccessor httpContextAccessor
        )
        {
            _context = context;
            _appSettings = appSettings.Value; // unwrap IOptions<AppSettings>
            _sessionHelper = sessionHelper;
            _globalOptionServices = globalOptionServices;
            _httpContextAccessor = httpContextAccessor;
        }

        /***************************************************************************************************
        * Since : 2026-Jul-04
        ****************************************************************************************************/
        public SelectList ModuleList(int? module_id)
        {
            var Modules = _context.tbl_user_module
                .OrderByDescending(m => m.module_sort)
                .ThenByDescending(m => m.module_label)
                .Select(m => new { m.module_id, m.module_label })
                .ToList();
            return new SelectList(Modules, "module_id", "module_label", module_id);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-04
        ****************************************************************************************************/

    }
}