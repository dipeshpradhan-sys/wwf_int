using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using wwfpp.Data;

public class AdministrationEmailService
{
    private readonly AppDbContext _context;

    public AdministrationEmailService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, (int? Id, string Email)>> GetAdministrationEmailsAsync()
    {
        var admin = await _context.tbl_employee_administrator.FirstOrDefaultAsync();
        if (admin == null) return new Dictionary<string, (int?, string)>();

        async Task<string> GetEmailAsync(int? empId)
        {
            if (!empId.HasValue) return string.Empty;
            return await _context.tbl_employee.Where(e => e.emp_id == empId.Value).Select(e =>(e.firstname ?? "").Trim() + " " +(e.middlename ?? "").Trim() + " " +(e.lastname ?? "").Trim() + "<" +(e.e_mail ?? "").Trim() + ">").FirstOrDefaultAsync();
        }

        var result = new Dictionary<string, (int?, string)>
        {
            ["cra"] = (admin.cra, await GetEmailAsync(admin.cra)),
            ["doo"] = (admin.doo, await GetEmailAsync(admin.doo)),
            ["faa"] = (admin.faa, await GetEmailAsync(admin.faa)),
            ["aca"] = (admin.aca, await GetEmailAsync(admin.aca)),
            ["hra"] = (admin.hra, await GetEmailAsync(admin.hra)),
            ["rca"] = (admin.rca, await GetEmailAsync(admin.rca)),
            ["acr"] = (admin.acr, await GetEmailAsync(admin.acr)),

            // Travel verifiers
            ["t_t_a_1"] = (admin.t_t_a_1, await GetEmailAsync(admin.t_t_a_1)),
            ["t_t_a_2"] = (admin.t_t_a_2, await GetEmailAsync(admin.t_t_a_2)),
            ["t_t_a_3"] = (admin.t_t_a_3, await GetEmailAsync(admin.t_t_a_3)),
            ["t_t_a_4"] = (admin.t_t_a_4, await GetEmailAsync(admin.t_t_a_4)),
            ["t_t_a_5"] = (admin.t_t_a_5, await GetEmailAsync(admin.t_t_a_5)),

            // Travel advance settlement verifiers
            ["t_a_s_1"] = (admin.t_a_s_1, await GetEmailAsync(admin.t_a_s_1)),
            ["t_a_s_2"] = (admin.t_a_s_2, await GetEmailAsync(admin.t_a_s_2)),
            ["t_a_s_3"] = (admin.t_a_s_3, await GetEmailAsync(admin.t_a_s_3)),
            ["t_a_s_4"] = (admin.t_a_s_4, await GetEmailAsync(admin.t_a_s_4)),
            ["t_a_s_5"] = (admin.t_a_s_5, await GetEmailAsync(admin.t_a_s_5)),
        };
        return result;
        //return result;
    }

}