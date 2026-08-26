using Microsoft.EntityFrameworkCore;
using System;
using System.Text;
using System.Threading.Tasks;
using wwfpp.Data;
using wwfpp.Models;
using wwfpp.Models.Request;
using wwfpp.Services;

public class TravelApprovalService
{
    private readonly AdministrationEmailService _adminService;
    private readonly RequestServices _requestServices;
    private readonly AppDbContext _context;

    public TravelApprovalService(AdministrationEmailService adminService, RequestServices requestServices, AppDbContext context)
    {
        _adminService = adminService;
        _requestServices = requestServices;
        _context = context;
    }

    public async Task<TravelApprovalResult> GetTravelValidManagerInfoAsync(
        int empId,
        string travelType,
        int? immediateSupervisorId,
        int? lineDirectorId,
        int? crEmpId,
        int? altCrEmpId,
        int? dooEmpId,
        string crAbsentStatus,
        string lineDirectorEmail,
        string supervisorEmail)
    {
        var emails = await _adminService.GetAdministrationEmailsAsync();
        var result = new TravelApprovalResult();

        // Case 1: Submitted by CR
        if (empId == crEmpId)
        {
            result.ApproverId = dooEmpId;
            result.ToEmployeeId = dooEmpId;
            result.ApproverEmail = emails["doo"].Email;
            result.Stage = "ad";
            result.ApproverPost = "Director of Operations";
            return result;
        }

        // Case 2: National travel
        if (travelType.Equals("NATIONAL", StringComparison.OrdinalIgnoreCase))
        {
            if (immediateSupervisorId == lineDirectorId)
            {
                result.ApproverId = lineDirectorId;
                result.ToEmployeeId = lineDirectorId;
                result.ApproverEmail = lineDirectorEmail;
                result.Stage = "ad";
                result.ApproverPost = "Line Director"; //app_by_post
            }
            else
            {
                result.IntermediateApproverId = immediateSupervisorId; //i_app_by
                result.IntermediateApproverPost = "Immediate Supervisor"; //i_app_by_post
                result.ApproverId = lineDirectorId; //app_by
                result.ToEmployeeId = immediateSupervisorId;//i_app_by
                result.ApproverEmail = supervisorEmail;//str_to
                result.Stage = "rd";
                result.ApproverPost = "Line Director"; //app_by_post
            }
            return result;
        }

        // Case 3: International travel
        if (lineDirectorId == crEmpId)
        {
            if (altCrEmpId.HasValue && altCrEmpId.Value != empId)
            {
                result.ApproverId = altCrEmpId;//app_by
                result.ToEmployeeId = altCrEmpId;//toemp_id
                result.ApproverEmail = emails["acr"].Email;//str_to
                result.Stage = "ad";
                result.ApproverPost = "Alt Country Representative"; //app_by_post
            }
            else
            {
                result.ApproverId = crEmpId;//app_by
                result.ToEmployeeId = crEmpId;//toemp_id
                result.ApproverEmail = emails["cra"].Email;//str_to
                result.Stage = "ad";
                result.ApproverPost = "Country Representative";//app_by_post
            }
        }
        else
        {
            int? crPresentStatus = GetCRAbsentStatus(Convert.ToInt32(crEmpId));
            if (altCrEmpId.HasValue && altCrEmpId.Value != 0 && crPresentStatus >0)
            {
                if (altCrEmpId == lineDirectorId)
                {
                    result.ApproverId = altCrEmpId; //app_by
                    result.ToEmployeeId = altCrEmpId; //toemp_id
                    result.ApproverEmail = emails["acr"].Email; //str_to
                    result.Stage = "ad";
                    result.ApproverPost = "Alt Country Representative";//app_by_post
                }
                else if (altCrEmpId == empId)
                {
                    result.IntermediateApproverId = lineDirectorId;//i_app_by
                    result.IntermediateApproverPost = "Line Director";//i_app_by_post
                    result.ApproverId = crEmpId;//app_by
                    result.ToEmployeeId = lineDirectorId;//toemp_id
                    result.ApproverEmail = lineDirectorEmail;//str_to
                    result.Stage = "rd";
                    result.ApproverPost = "Country Representative";//app_by_post
                }
                else
                {
                    result.IntermediateApproverId = lineDirectorId;//i_app_by
                    result.IntermediateApproverPost = "Line Director";//i_app_by_post
                    result.ApproverId = altCrEmpId;//app_by
                    result.ToEmployeeId = lineDirectorId;//toemp_id
                    result.ApproverEmail = lineDirectorEmail;//str_to
                    result.Stage = "rd";
                    result.ApproverPost = "Alt Country Representative"; //app_by_post
                }
            }
            else
            {
                result.IntermediateApproverId = lineDirectorId;//i_app_by
                result.IntermediateApproverPost = "Line Director";//i_app_by_post
                result.ApproverId = crEmpId;//app_by
                result.ToEmployeeId = lineDirectorId;//toemp_id
                result.ApproverEmail = lineDirectorEmail;//str_to
                result.Stage = "rd";
                result.ApproverPost = "Country Representative"; //app_by_post
            }
        }

        return result;
    }

    public async Task<string> GetTravelEmailHtmlContent(int empTravelId)

    {

        var travelMain = await _context.tbl_employee_travel_main
        .Where(s => s.emp_travel_id == empTravelId)
        .FirstOrDefaultAsync();
        string? trip_purpose = travelMain.trip_purpose;
        string? denomination = travelMain.denomination;
        string? remarks = travelMain.remarks;
        string? travel_type = travelMain.travel_type;
        string? destinations = travelMain.destinations;
        DateTime? submit_date = travelMain.submit_date;
        DateTime? date_from = travelMain.date_from;
        DateTime? date_to = travelMain.date_to;


        string Normalize(string? input, string lblNa)
        {
            if (string.IsNullOrWhiteSpace(input))
                return lblNa;
            return input.Replace("\r", "<br/>").Replace("\n", "<br/>");
        }

        trip_purpose = Normalize(trip_purpose, "N/A");
        denomination = Normalize(denomination, "N/A");
        remarks = Normalize(remarks, "N/A");

        decimal t_amount_1 = 0, t_amount_2 = 0, t_amount_3 = 0, t_amount_4 = 0, t_amount_5 = 0, t_amount_6 = 0;
        string show_total_1 = "", show_total_2 = "", show_total_3 = "", show_total_4 = "", show_total_5 = "", show_total_6 = "";

        var strParticulars = new StringBuilder();

        // --- Sub records (expenses) ---
        var subs = await _context.tbl_employee_travel_sub
            .Where(s => s.emp_travel_id == empTravelId)
            .ToListAsync();

        foreach (var sub in subs)
        {
            var parName = await _context.tbl_travel_particulars
                .Where(p => p.par_id == sub.par_id)
                .Select(p => p.particular)
                .FirstOrDefaultAsync();

            var curName = await _context.tbl_currency
                .Where(c => c.cur_id == sub.cur_id)
                .Select(c => c.cur_abbr)
                .FirstOrDefaultAsync();

            var nos = sub.nos ?? 0;
            var rate = sub.rate ?? 0;
            var amount = (decimal)nos * (decimal)rate;

            switch (sub.cur_id)
            {
                case 1: t_amount_1 += amount; break;
                case 2: t_amount_2 += amount; break;
                case 3: t_amount_3 += amount; break;
                case 4: t_amount_4 += amount; break;
                case 5: t_amount_5 += amount; break;
                case 6: t_amount_6 += amount; break;
            }

            strParticulars.AppendLine($@"
        <tr>
            <td align='left'>{parName}</td>
            <td align='left'>{sub.detail ?? ""}</td>
            <td align='right'>{sub.unit ?? ""}</td>
            <td align='right'>{nos}</td>
            <td align='right'>{rate:F2}</td>
            <td align='center'>{curName}</td>
            <td align='right'>{amount:F2}</td>
        </tr>");
        }

        var LocalCurrency = _requestServices.GetApplicationSetting("op_currency_symbol");
        if (t_amount_1 > 0) show_total_1 = $"{LocalCurrency} : {t_amount_1:F2} | ";
        if (t_amount_2 > 0) show_total_2 = $"IC : {t_amount_2:F2} | ";
        if (t_amount_3 > 0) show_total_3 = $"USD : {t_amount_3:F2} | ";
        if (t_amount_4 > 0) show_total_4 = $"Euro : {t_amount_4:F2} | ";
        if (t_amount_5 > 0) show_total_5 = $"Pound : {t_amount_5:F2} | ";
        if (t_amount_6 > 0) show_total_6 = $"CHF : {t_amount_6:F2} | ";

        // --- Fund sources (up to 4 slots) ---
        var strFundSources = new StringBuilder();
        for (int sn = 1; sn <= 4; sn++)
        {
            var fundId = await _context.tbl_employee_travel_codes
                .Where(c => c.emp_travel_id == empTravelId && c.sn == sn)
                .Select(c => c.fund_id)
                .FirstOrDefaultAsync();

            if (fundId > 0)
            {
                var fundName = await _context.tbl_fund_source
                    .Where(f => f.fund_id == fundId)
                    .Select(f => f.fund_source)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(fundName))
                    strFundSources.AppendLine($"{fundName}<br/>");
            }
        }

        // --- Final HTML ---
        var strParticularsDetail = $@"
    <b>Travel Type : </b>{travel_type}
    <br/><b>Purpose of Trip : </b>{trip_purpose}
    <br/><b>Destination/s : </b>{destinations}
    <br/><b>Submit Date : </b>{submit_date}
    <br/><b>Start Date : </b>{date_from}
    <br/><b>End Date : </b>{date_to}
    <br/><br/><div>
    <table border='0' bgcolor='#cccccc'>
        <tr>
            <td align='left' bgcolor='#eeeeee'><b>Particulars</b></td>
            <td align='left' bgcolor='#eeeeee'><b>Details</b></td>
            <td align='right' bgcolor='#eeeeee'><b>Unit</b></td>
            <td align='right' bgcolor='#eeeeee'><b>Nos.</b></td>
            <td align='right' bgcolor='#eeeeee'><b>Rate</b></td>
            <td align='center' bgcolor='#eeeeee'><b>Currency</b></td>
            <td align='right' bgcolor='#eeeeee'><b>Amount</b></td>
        </tr>
        {strParticulars}
        <tr bgcolor='#eeeeee'>
            <td align='left' colspan='7'>Total: {show_total_1}{show_total_2}{show_total_3}{show_total_4}{show_total_5}{show_total_6}</td>
        </tr>
    </table>
    </div>

    <br/><br/><b>Fund Source: </b><br/>{strFundSources}
    <br/><b>Currency Denomination: </b><br/>{denomination}
    <br/><br/><b>Remarks: </b><br/>{remarks}";

        return strParticularsDetail;
    }
    public int GetCRAbsentStatus(int cr_emp_id)
    {
        DateTime curDate = DateTime.Today;

        int result = _context.tbl_employee_travel_main
            .Where(e => e.emp_id == cr_emp_id
                && e.date_from <= curDate
                && e.date_to >= curDate)
            .Select(e => (int?)e.emp_id)   // cast to int? explicitly
            .FirstOrDefault() ?? 0;        // fallback if null

        return result;
    }
}