using Microsoft.EntityFrameworkCore;
using wwfpp.Data;

namespace wwfpp.Services
{
    public class AdminEmailServices
    {
        private readonly AppDbContext _context;
        private readonly SettingsServices _settingsServices;
        public AdminEmailServices(AppDbContext context, SettingsServices settingsServices)
        {
            _context = context;
            _settingsServices = settingsServices;
        }
        public async Task<Dictionary<string, (int? Id, string Email)>> GetAdminEmailsAsync()
        {
            var admin = await _context.tbl_employee_administrator.FirstOrDefaultAsync().ConfigureAwait(false);
            if (admin == null) { return new Dictionary<string, (int?, string)>(); }

            async Task<string> GetEmailAsync(int? empId)
            {
                return !empId.HasValue
                    ? string.Empty
                    : await _context.tbl_employee
                    .Where(e => e.emp_id == empId.Value)
                    .Select(e =>
                        (e.firstname ?? "").Trim() + " " + (e.middlename ?? "").Trim() + " " + (e.lastname ?? "").Trim()
                        + "<" + (e.e_mail ?? "").Trim() + ">")
                    .FirstOrDefaultAsync().ConfigureAwait(false) ?? string.Empty;
            }

            var result = new Dictionary<string, (int?, string)>
            {
                ["cra"] = (admin.cra, await GetEmailAsync(admin.cra).ConfigureAwait(false)),    /** COUNTRY REPRESENTATIVE ID */
                ["acr"] = (admin.acr, await GetEmailAsync(admin.acr).ConfigureAwait(false)),    /** ALT COUNTRY REPRESENTATIVE ID */
                ["doo"] = (admin.doo, await GetEmailAsync(admin.doo).ConfigureAwait(false)),    /** DIRECTOR OF OPERATION ID */
                ["faa"] = (admin.faa, await GetEmailAsync(admin.faa).ConfigureAwait(false)),    /** FINANCE ADMINISTRATOR */
                ["aca"] = (admin.aca, await GetEmailAsync(admin.aca).ConfigureAwait(false)),    /** ACCOUNT ADMINISTRATOR */
                ["hra"] = (admin.hra, await GetEmailAsync(admin.hra).ConfigureAwait(false)),    /** HR HEAD ADMINISTRATOR */
                ["ahr"] = (admin.hra, await GetEmailAsync(admin.ahr).ConfigureAwait(false)),    /** Human Resource Manager | 2025-Aug-07 onwards */
                ["rca"] = (admin.rca, await GetEmailAsync(admin.rca).ConfigureAwait(false)),    /** FRONT DESK | RECEPTIONISTs EMAIL */

                // Travel advance verifiers
                ["t_t_a_1"] = (admin.t_t_a_1, await GetEmailAsync(admin.t_t_a_1).ConfigureAwait(false)),
                ["t_t_a_2"] = (admin.t_t_a_2, await GetEmailAsync(admin.t_t_a_2).ConfigureAwait(false)),
                ["t_t_a_3"] = (admin.t_t_a_3, await GetEmailAsync(admin.t_t_a_3).ConfigureAwait(false)),
                ["t_t_a_4"] = (admin.t_t_a_4, await GetEmailAsync(admin.t_t_a_4).ConfigureAwait(false)),
                ["t_t_a_5"] = (admin.t_t_a_5, await GetEmailAsync(admin.t_t_a_5).ConfigureAwait(false)),

                // Travel advance settlement verifiers
                ["t_a_s_1"] = (admin.t_a_s_1, await GetEmailAsync(admin.t_a_s_1).ConfigureAwait(false)),
                ["t_a_s_2"] = (admin.t_a_s_2, await GetEmailAsync(admin.t_a_s_2).ConfigureAwait(false)),
                ["t_a_s_3"] = (admin.t_a_s_3, await GetEmailAsync(admin.t_a_s_3).ConfigureAwait(false)),
                ["t_a_s_4"] = (admin.t_a_s_4, await GetEmailAsync(admin.t_a_s_4).ConfigureAwait(false)),
                ["t_a_s_5"] = (admin.t_a_s_5, await GetEmailAsync(admin.t_a_s_5).ConfigureAwait(false)),
            };
            /*
             * CHECKING IF COUNTRY REPRESENTATIVE IS IN INTERNATIONAL TRAVEL
             * If in travel return "Absent" else return Present"
             */
            if (_settingsServices.GetCRAbsentStatus(admin.cra) == "Present")
            {
                result["acr"] = (0, string.Empty);  /** ALT COUNTRY REPRESENTATIVE ID */
            }
            return result;
        }

    }
}