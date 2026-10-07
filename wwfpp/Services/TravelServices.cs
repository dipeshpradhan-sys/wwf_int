using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuestPDF.Infrastructure;
using System.Data;
using System.Text;
using wwfpp.Data;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Models.Personnel;

namespace wwfpp.Services
{
    public class TravelServices
    {
        private readonly AppDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly GlobalOptionServices _globalOptionServices;
        private readonly SettingsServices _settingsServices;
        private readonly GeneralServices _generalServices;
        private readonly EmployeeServices _employeeServices;
        private readonly RequestServices _requestServices;
        private readonly AdminEmailServices _adminEmailServices;
        public TravelServices(
                AppDbContext context,
                IOptions<AppSettings> appSettings,
                GlobalOptionServices globalOptionServices,
                SettingsServices settingsServices,
                GeneralServices generalServices,
                EmployeeServices employeeServices,
                RequestServices requestServices,
                AdminEmailServices adminEmailServices
        )
        {
            _context = context;
            _appSettings = appSettings.Value; // unwrap IOptions<AppSettings>
            _globalOptionServices = globalOptionServices;
            _settingsServices = settingsServices;
            _generalServices = generalServices;
            _employeeServices = employeeServices;
            _requestServices = requestServices;
            _adminEmailServices = adminEmailServices;
        }
        /***************************************************************************************************
        * Since : 2026-Sep-19
        * Contribution: IsCurrentYearPendingTravel
        ****************************************************************************************************/
        public string IsCurrentYearPendingTravel(int empId, DateTime startDate, DateTime endDate)
        {
            /**
            '  Status     im|Line         im|Line            im|Line              im|Line   
            '1.Pending:   ''|Pending, 	null|Pending, 	Pending|Pending, 	Approved|Pending
            '2.Approved:  ''|Approved,	null|Approved, 	Approved|Approved
            '3.Declined:  ''|Declined,	null|Declined, 	(Declined|Pending),	Approved|Declined
            '4.Cancelled: ''|Cancelled,   null|Cancelled, Approved|Cancelled
            'There is pending travels only if: case 1 exists
            */
            if (empId == 0) { return "Y"; }
            /*
            //use this if we need to check pending between current fiscal year 
            bool exists = _context.tbl_employee_travel_main.Any(z =>
                z.emp_id == empId &&
                (z.i_app_status == null || z.i_app_status != "Declined") &&
                z.app_status == "Pending" &&
                z.date_from >= startDate &&
                z.date_from <= endDate
            );
            */
            var StartDate = new DateTime(2020, 7, 1);
            bool exists = _context.tbl_employee_travel_main.Any(z =>
                z.emp_id == empId &&
                (z.i_app_status == null || z.i_app_status != "Declined") &&
                z.app_status == "Pending" &&
                z.date_from >= StartDate
            );

            return exists ? "Y" : "N";
        }
        /***************************************************************************************************
        * Since : 2026-Sep-20
        * Contribution: MapCurrency
        ****************************************************************************************************/
        public string MapCurrency(byte? curId)
        {
            return !curId.HasValue
            ? ""
            : curId.Value switch
            {
                1 => "NRS",
                2 => "IC",
                3 => "USD",
                4 => "Euro",
                5 => "Pound",
                6 => "CHF",
                _ => ""
            };
        }
        /***************************************************************************************************
        * Since : 2026-Sep-19
        ****************************************************************************************************/
        public SelectList GetCurrencyType(byte? curId)
        {
            var CurrencyList = _context.tbl_currency
                .OrderBy(f => f.cur_abbr)
                .Select(f => new
                {
                    f.cur_id,
                    DisplayName = f.cur_abbr
                })
                .ToList();
            return new SelectList(CurrencyList, "cur_id", "DisplayName", curId);
        }
        /***************************************************************************************************
        * Since : 2026-Sep-19
        ****************************************************************************************************/
        public SelectList GetTravelType(string selvalue = "")
        {
            var options = new Dictionary<string, string>
            {
                { "National", "National" },
                { "International", "International" }
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Sep-24
        * CHECK IF LEAVE IS ALREADY SUBMITTED FOR THAT DAY
        * ** N = TRAVEL AVAILABLE FOR THAT DAY | Y = TRAVEL ALREADY TAKEN ON THAT DAY ***
        * Not allow cases: i_app_status|app_status : NULL|Pending, ''|Pending, Pending|Pending, Approved|Pending, Approved|Approved
        * Allow cases : Declined|Pending, Approved|Declined, Approved|Cancelled
        ****************************************************************************************************/
        public async Task<string> ChkTravelRequestedDayAsync(int empId, DateTime startDate, DateTime endDate, int? empTravelId = null)
        {
            bool exists = await _context.tbl_employee_travel_main
                .Where(t => t.emp_id == empId
                    && (
                        (startDate >= t.date_from && startDate <= t.date_to) ||
                        (endDate >= t.date_from && endDate <= t.date_to) ||
                        (t.date_from >= startDate && t.date_from <= endDate) ||
                        (t.date_to >= startDate && t.date_to <= endDate)
                    )
                    && (t.app_status == "Pending" || t.app_status == "Approved")
                    && (string.IsNullOrEmpty(t.i_app_status)
                        || t.i_app_status == "Pending"
                        || t.i_app_status == "Approved"))
                .Where(t => empTravelId == null || t.emp_travel_id == empTravelId)
                .AnyAsync().ConfigureAwait(false);

            return exists ? "Y" : "N";
        }
        /***************************************************************************************************
        * Since : 2026-Sep-24
        * Get and assign Imme. supervisor, Line director, CR or Alt CR for Emailing
        ****************************************************************************************************/
        public async Task<TravelApprovalViewModel> GetTravelValidManagerInfoAsync(int empId, string TravelType)
        {
            /** Immediate Supervisor **/
            var (iToEmpId, IByPost) = await _requestServices.ResolveApproverAsync(empId).ConfigureAwait(false);
            int imId = iToEmpId;
            string iByPost = IByPost;
            string? imEmail = _employeeServices.GetEmployeeNameEmail(imId);

            /** Line Director **/
            var (toEmpId, ByPost) = await _requestServices.ResolveApproverAsync(empId, true).ConfigureAwait(false);
            int lnId = toEmpId;
            string byPost = ByPost;
            string? lnEmail = _employeeServices.GetEmployeeNameEmail(lnId);
            var result = new TravelApprovalViewModel();
            var admins = await _adminEmailServices.GetAdminEmailsAsync().ConfigureAwait(false);
            int? craId = admins["cra"].Id;
            int? acrId = admins["acr"].Id;
            int? dooId = admins["doo"].Id;
            string? craEmail = admins["cra"].Email;
            string? acrEmail = admins["acr"].Email;
            string? dooEmail = admins["doo"].Email;

            string IAppStatus = "";
            int IAppBy = 0;
            string IAppByPost = "";

            string AppStatus = "";
            int AppBy = 0;
            string AppByPost = "";

            int ToEmpId = 0;
            string StrTo = "";
            string St = "";

            // Case 1: Submitted by CR
            if (empId == craId)
            {
                AppStatus = "Pending";
                AppBy = dooId ?? 0;
                ToEmpId = AppBy;
                StrTo = dooEmail;
                St = "ad";
                AppByPost = "Head of Operations";
            }
            else
            {
                /** CR HAS NOT SUBMITTED THE TRAVEL **/
                // Case 2: '** CHECK IF THE TRAVEL IS NATIONAL OR INTERNATIONAL **'
                if (TravelType.Equals("NATIONAL", StringComparison.OrdinalIgnoreCase))
                {
                    /** TRAVEL IS NATIONAL **/

                    /** CHECK IF IMME. SUPERVISOR & LINE DIRECTOR SAME **/
                    if (imId == lnId)
                    {
                        AppStatus = "Pending";
                        AppBy = lnId;
                        ToEmpId = AppBy;
                        StrTo = lnEmail ?? "";
                        St = "ad";
                        AppByPost = byPost ?? "";
                    }
                    else
                    {
                        /** **************SAME: NO : SEND TO IMMEDIATE SUPERVISOR**/
                        IAppStatus = "Pending";
                        IAppBy = imId;
                        IAppByPost = iByPost ?? "";

                        AppStatus = "Pending";
                        AppBy = lnId;
                        AppByPost = byPost ?? "";

                        ToEmpId = IAppBy;
                        StrTo = imEmail ?? "";
                        St = "rd";
                    }
                }
                else
                {
                    /** TRAVEL IS INTERNATIONAL (OTHERS) **/
                    /** CHECK IF IMME. SUPERVISOR & CR SAME **/

                    if (lnId == craId)
                    {
                        /**SAME: YES : DIRECTLY SEND TO CR**/

                        /**************** CHECK IF A.I. CR DEFINED **/
                        if (acrId.HasValue && acrId.Value != 0 && acrId.Value != empId)
                        {
                            /** **************ALT CR DEFINED : YES**' AND AGAIN IF AICR IS EMPLOYEE HIM / HER SELF THEN(EMP_ID = AICR)*/
                            AppStatus = "Pending";
                            AppBy = acrId ?? 0;
                            ToEmpId = AppBy;
                            StrTo = acrEmail;
                            St = "ad";
                            AppByPost = "A.I. Country Representative";
                        }
                        else
                        {
                            /** **************ALT CR DEFINED : NO**/
                            AppStatus = "Pending";
                            AppBy = Convert.ToInt32(craId);
                            ToEmpId = AppBy;
                            StrTo = craEmail;
                            St = "ad";
                            AppByPost = "Country Representative";
                        }
                    }
                    else
                    {
                        /** **************SAME: NO : SEND TO LINE DIRECTOR FOR RECOMMENDATION - PREVIOUSLY IT WAS IMMEDIATE SUPERVISOR**/
                        /** CHECK IF A.I. CR DEFINED **/
                        if (acrId.HasValue && acrId.Value != 0) /** CR IS ABSENT */
                        {
                            if (acrId == lnId)
                            {
                                AppStatus = "Pending";
                                AppBy = Convert.ToInt32(acrId);
                                ToEmpId = AppBy;
                                StrTo = acrEmail ?? "";
                                St = "ad";
                                AppByPost = "A.I. Country Representative";
                            }
                            else if (acrId == empId)
                            {
                                IAppStatus = "Pending";
                                IAppBy = lnId;
                                IAppByPost = byPost ?? "";

                                AppStatus = "Pending";
                                AppBy = craId ?? 0;
                                AppByPost = "Country Representative";

                                ToEmpId = IAppBy;
                                StrTo = lnEmail ?? "";
                                St = "rd";
                            }
                            else
                            {
                                IAppStatus = "Pending";
                                IAppBy = lnId;
                                IAppByPost = byPost ?? "";

                                AppStatus = "Pending";
                                AppBy = acrId ?? 0;
                                AppByPost = "A. I. Country Representative";

                                ToEmpId = IAppBy;
                                StrTo = lnEmail ?? "";
                                St = "rd";
                            }
                        }
                        else
                        {
                            /**CR IS PRESENT */
                            IAppStatus = "Pending";
                            IAppBy = lnId;
                            IAppByPost = byPost ?? "";

                            AppStatus = "Pending";
                            AppBy = craId ?? 0;
                            AppByPost = "Country Representative";

                            ToEmpId = IAppBy;
                            StrTo = lnEmail ?? "";
                            St = "rd";
                        }
                    }
                } //end  "NATIONAL"
            }
            result.IAppStatus = IAppStatus;
            result.IAppBy = IAppBy;
            result.IAppByPost = IAppByPost;

            result.AppStatus = AppStatus;
            result.AppBy = AppBy;
            result.AppByPost = AppByPost;

            result.ToEmpId = ToEmpId;
            result.StrTo = StrTo;
            result.St = St;

            return result;
        }
        /***************************************************************************************************
        * Since : 2026-Sep-24
        * Get and assign  Line director, CR or Alt CR for Emailing
        ****************************************************************************************************/
        public async Task<(int AppBy, string StrTo)> GetTravelValidManagerFinalInfoAsync(int empId, string TravelType)
        {
            /** Line Director **/
            var (toEmpId, ByPost) = await _requestServices.ResolveApproverAsync(empId, true).ConfigureAwait(false);
            int lnId = toEmpId;
            string byPost = ByPost;
            string? lnEmail = _employeeServices.GetEmployeeNameEmail(lnId);

            var admins = await _adminEmailServices.GetAdminEmailsAsync().ConfigureAwait(false);
            int? craId = admins["cra"].Id;
            int? acrId = admins["acr"].Id;
            int? dooId = admins["doo"].Id;

            string? craEmail = admins["cra"].Email;
            string? acrEmail = admins["acr"].Email;
            string? dooEmail = admins["doo"].Email;

            int AppBy;
            string StrTo;

            // Case 1: ** CHECK IF SUBMITTED BY CR **'	'** CR : YES **
            if (empId == craId)
            {
                AppBy = dooId ?? 0;
                StrTo = dooEmail;
            }
            else
            {
                // Case 2: '** TRAVEL IS NATIONAL **'
                if (TravelType.Equals("NATIONAL", StringComparison.OrdinalIgnoreCase))
                {
                    AppBy = lnId;
                    StrTo = lnEmail ?? "";
                }
                else
                {
                    /** TRAVEL IS INTERNATIONAL (OTHERS) **/
                    if (acrId.HasValue)
                    {
                        /** ******** CR IS ABSENT ********/
                        AppBy = acrId ?? 0;
                        StrTo = acrEmail;
                    }
                    else
                    {
                        /** ******* ALT CR DEFINED : NO *********/
                        AppBy = craId ?? 0;
                        StrTo = craEmail;
                    }
                } //end  "NATIONAL"
            }
            return (AppBy, StrTo);
        }
        /***************************************************************************************************
        * Since : 2026-Sep-24
        * SECTION : Travel   
        * Making HTML of Travel information
        ****************************************************************************************************/
        public async Task<string> GetTravelDetailHtmlAsync(int empTravelId)
        {
            string strParticularsDetail = "";
            var travelMain = await _context.tbl_employee_travel_main.Where(s => s.emp_travel_id == empTravelId).FirstOrDefaultAsync().ConfigureAwait(false);
            if (travelMain == null) { return ""; }

            string travel_type = travelMain.travel_type ?? "";
            string trip_purpose = travelMain.trip_purpose ?? string.Empty;
            string destinations = travelMain.destinations ?? "";
            var submit_date = Convert.ToDateTime(travelMain.submit_date);
            var date_from = Convert.ToDateTime(travelMain.date_from);
            var date_to = Convert.ToDateTime(travelMain.date_to);
            string denomination = travelMain.denomination ?? "";
            string remarks = travelMain.remarks ?? "";

            string str_message_remarks = "";

            if (!string.IsNullOrWhiteSpace(travelMain.rec_remarks))
            {
                str_message_remarks += $"<br/><b>Recommender Remarks: </b>{travelMain.rec_remarks}";
            }
            if (!string.IsNullOrWhiteSpace(travelMain.app_remarks))
            {
                str_message_remarks += $"<br/><b>Approved/Declined Remarks: </b>{travelMain.app_remarks}";
            }

            static string Normalize(string? input, string lblNa)
            {
                return string.IsNullOrWhiteSpace(input)
                    ? lblNa
                    : input
                    .Replace("\r", "<br/>", StringComparison.OrdinalIgnoreCase)
                    .Replace("\n", "<br/>", StringComparison.OrdinalIgnoreCase);
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
                .ToListAsync().ConfigureAwait(false);

            foreach (var sub in subs)
            {
                string parName = await _context.tbl_travel_particulars
                    .Where(p => p.par_id == sub.par_id)
                    .Select(p => p.particular)
                    .FirstOrDefaultAsync().ConfigureAwait(false) ?? "";

                string curName = await _context.tbl_currency
                    .Where(c => c.cur_id == sub.cur_id)
                    .Select(c => c.cur_abbr)
                    .FirstOrDefaultAsync().ConfigureAwait(false) ?? "";

                double nos = sub.nos ?? 0;
                decimal rate = sub.rate ?? 0;
                decimal amount = (decimal)nos * rate;

                switch (sub.cur_id)
                {
                    case 1: t_amount_1 += amount; break;
                    case 2: t_amount_2 += amount; break;
                    case 3: t_amount_3 += amount; break;
                    case 4: t_amount_4 += amount; break;
                    case 5: t_amount_5 += amount; break;
                    case 6: t_amount_6 += amount; break;
                    default: break;
                }

                _ = strParticulars.AppendLine($@"
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

            string op_currency_symbol = _globalOptionServices.OptionServices["op_currency_symbol"];
            if (t_amount_1 > 0) { show_total_1 = $"{op_currency_symbol} : {t_amount_1:F2} | "; }
            if (t_amount_2 > 0) { show_total_2 = $"IC : {t_amount_2:F2} | "; }
            if (t_amount_3 > 0) { show_total_3 = $"USD : {t_amount_3:F2} | "; }
            if (t_amount_4 > 0) { show_total_4 = $"Euro : {t_amount_4:F2} | "; }
            if (t_amount_5 > 0) { show_total_5 = $"Pound : {t_amount_5:F2} | "; }
            if (t_amount_6 > 0) { show_total_6 = $"CHF : {t_amount_6:F2} | "; }

            // --- Fund sources (up to 4 slots) ---
            var strFundSources = new StringBuilder();
            for (int sn = 1; sn <= 4; sn++)
            {
                int fundId = await _context.tbl_employee_travel_codes
                    .Where(c => c.emp_travel_id == empTravelId && c.sn == sn)
                    .Select(c => c.fund_id)
                    .FirstOrDefaultAsync().ConfigureAwait(false) ?? 0;

                if (fundId > 0)
                {
                    string fundName = await _context.tbl_fund_source
                        .Where(f => f.fund_id == fundId)
                        .Select(f => f.fund_source)
                        .FirstOrDefaultAsync().ConfigureAwait(false) ?? "";

                    if (!string.IsNullOrEmpty(fundName)) { _ = strFundSources.AppendLine($"{fundName}<br/>"); }
                }
            }

            // --- Final HTML ---
            strParticularsDetail = $@"
            <b>Travel Type : </b>{travel_type}
            <br/><b>Purpose of Trip : </b>{trip_purpose}
            <br/><b>Destination/s : </b>{destinations}
            <br/><b>Submit Date : </b>{submit_date.ToString(_appSettings.DATE_FORMAT)}
            <br/><b>Start Date : </b>{date_from.ToString(_appSettings.DATE_FORMAT)}
            <br/><b>End Date : </b>{date_to.ToString(_appSettings.DATE_FORMAT)}
            {str_message_remarks}
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
        /***************************************************************************************************
        * Since : 2026-Sep-24
        * SECTION : Travel
        * Making Travel Detail for Emailing
        * Parm: parm_show_app_dec : A to show approve decline text
        * Parm: parm_mode : I = Insert, U = Update
        * Parm: parm_st   : ad = approve/decline, rd = recomend/decline
        ****************************************************************************************************/
        public async Task<string> TravelApproveDeclineEmailAsync(string ShowAppDecLink, string Mode, string St, int empId, int EmpTravelId, int toEmpID)
        {
            string StrMessageArea;
            string st_ad_a = "a";
            string st_ad_d = "d";
            string st_ad_ar = LangEmail.TXT_APPROVE;
            string st_ad_dd = LangEmail.TXT_DECLINE;

            StrMessageArea = LangEmail.EMAIL_EMPLOYEE_TRAVEL_SAVE_MESSAGE;
            if (Mode == "U")
            {
                StrMessageArea = LangEmail.EMAIL_EMPLOYEE_TRAVEL_UPDATE_MESSAGE;
            }
            else if (Mode == "R")
            {
                StrMessageArea = LangEmail.EMAIL_EMPLOYEE_TRAVEL_RECOMMENDED_MESSAGE;
            }
            else if (Mode == "N")
            {
                StrMessageArea = LangEmail.EMAIL_EMPLOYEE_TRAVEL_RE_NOTIFY_MESSAGE;
            }
            if (St == "rd")
            {
                st_ad_a = "rr";
                st_ad_d = "nr";
                st_ad_ar = LangEmail.TXT_RECOMMEND;
            }

            string StrParticularsDetail = await GetTravelDetailHtmlAsync(EmpTravelId).ConfigureAwait(false);
            _ = StrMessageArea.Replace("<[STR-PARTICULARS-DETAIL]>", StrParticularsDetail, StringComparison.OrdinalIgnoreCase);

            int toID = await _requestServices.GetUserIdFromEmployeeIdAsync(toEmpID).ConfigureAwait(false);

            string EmailLinkApprove = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={GblUtilities.Encode(empId.ToString())}&app_id={GblUtilities.Encode(EmpTravelId.ToString())}&toid={GblUtilities.Encode(toID.ToString())}&toemp_id={GblUtilities.Encode(toEmpID.ToString())}&st={st_ad_a}&via=eml&cat=travel'>{st_ad_ar}</a> | ";
            string EmailLinkDecline = $"<a href='{_appSettings.BaseUrl}Home/Index?emp_id={GblUtilities.Encode(empId.ToString())}&app_id={GblUtilities.Encode(EmpTravelId.ToString())}&toid={GblUtilities.Encode(toID.ToString())}&toemp_id={GblUtilities.Encode(toEmpID.ToString())}&st={st_ad_d}&via=eml&cat=travel'>{st_ad_dd}</a> | ";

            string ApproveDecline = LangEmail.EMAIL_APPROVE_DECLINE
                .Replace("<APPROVE-LINK]>", EmailLinkApprove, StringComparison.Ordinal)
                .Replace("<DECLINE-LINK]>", EmailLinkDecline, StringComparison.Ordinal);

            string travelApproveDeclineEmail = StrMessageArea;
            if (ShowAppDecLink == "A") { _ = travelApproveDeclineEmail + ApproveDecline; }

            return travelApproveDeclineEmail;
        }
        /***************************************************************************************************
        * Since : 2026-Sep-24
        * SECTION : Travel
        * get travel status detail information from database
        * Parm: EmpTravelId : travel id
        ****************************************************************************************************/
        public async Task<TravelStatusBothViewModel> GetTravelStatusBothAsync(int EmpTravelId)
        {
            var result = new TravelStatusBothViewModel();

            var travel = await _context.tbl_employee_travel_main
                .Where(t => t.emp_travel_id == EmpTravelId)
                .Select(t => new
                {
                    t.i_app_status,
                    t.i_app_by,
                    t.i_app_by_post,
                    t.app_status,
                    t.app_by,
                    t.app_by_post
                }).FirstOrDefaultAsync().ConfigureAwait(false);
            if (travel != null)
            {
                result.IAppStatus = travel.i_app_status;
                result.IAppBy = travel.i_app_by;
                result.IAppByPost = travel.i_app_by_post;
                result.AppStatus = travel.app_status;
                result.AppBy = travel.app_by;
                result.AppByPost = travel.app_by_post;
            }
            return result;
        }
        /***************************************************************************************************
        * Since : 2026-Sep-27
        ****************************************************************************************************/
        public bool IsTravelAlreadySettled(int empTravelId)
        {
            if (empTravelId < 1) { return true; } //Treat like settled

            bool exists = _context.tbl_employee_travel_settlement_main
                .Any(s => s.emp_travel_id == empTravelId);

            return exists;
        }
        /***************************************************************************************************
        * Since : 2026-Sep-27
        ****************************************************************************************************/
        #region Getting TRAVEL VIEWMODEL 
        public async Task<TravelViewModel> GetTravelDetailAsync(int EmpTravelId)
        {
            if (EmpTravelId <= 0) { return null; }

            var smt = (from main in _context.tbl_employee_travel_main
                       join emp in _context.tbl_employee
                       on main.emp_id equals emp.emp_id
                       where main.emp_travel_id == EmpTravelId
                       select new
                       {
                           main.emp_travel_id,
                           main.emp_id,
                           main.trip_purpose,
                           main.destinations,
                           main.date_from,
                           main.date_to,
                           main.submit_date,
                           main.app_status,
                           main.app_by,
                           main.app_date,
                           main.denomination,
                           main.remarks,
                           main.travel_type,
                           main.i_app_status,
                           main.i_app_by,
                           main.i_app_date,
                           main.i_app_by_post,
                           main.app_by_post,
                           main.rec_remarks,
                           main.app_remarks,
                           main.can_submit_date,
                           main.can_desc,
                           main.can_by,
                           main.can_date,
                           main.can_remarks,
                           employee = $"{emp.firstname} {emp.middlename} {emp.lastname} ({emp.emp_code})",
                           emp.emp_status
                       }).FirstOrDefault();

            if (smt == null) { return null; }

            DateTime TravelFromDate = Convert.ToDateTime(smt.date_from);
            string FiscalYear = _settingsServices.GetFiscalYearByDate(TravelFromDate);
            string fiscal_year_abb = _settingsServices.GetFiscalYearValue(FiscalYear, "fiscal_year_abb") ?? "";
            string startFiscalDate = _settingsServices.GetFiscalYearValue(FiscalYear, "date_from") ?? "";
            string endFiscaDate = _settingsServices.GetFiscalYearValue(FiscalYear, "date_to") ?? "";
            DateTime start_fiscal_date = DateTime.TryParse(startFiscalDate, out DateTime DS) ? DS : DateTime.MinValue;
            DateTime end_fiscal_date = DateTime.TryParse(endFiscaDate, out DateTime DE) ? DE : DateTime.MinValue;

            TravelViewModel model;
            model = new TravelViewModel
            {
                EmpTravelId = smt.emp_travel_id,
                TravelType = smt.travel_type,
                TripPurpose = smt.trip_purpose,
                Destinations = smt.destinations,
                DateFrom = smt.date_from,
                DateTo = smt.date_to,
                SubmitDate = smt.submit_date,
                Denomination = smt.denomination,
                Remarks = smt.remarks,
                EmpId = smt.emp_id,
                Employee = smt.employee,
                EmpStatus = smt.emp_status,
                IAppStatus = smt.i_app_status,
                IAppBy = smt.i_app_by,
                IAppByName = (smt.i_app_by is not null and > 0) ? _employeeServices.GetEmployeeName((int)smt.i_app_by) : "",
                IAppDate = smt.i_app_date,
                IAppByPost = smt.i_app_by_post,
                IAppRemarks = smt.rec_remarks,
                AppStatus = smt.app_status,
                AppBy = smt.app_by,
                AppByName = (smt.app_by is not null and > 0) ? _employeeServices.GetEmployeeName((int)smt.app_by) : "",
                AppDate = smt.app_date,
                AppByPost = smt.app_by_post,
                AppRemarks = smt.app_remarks,
                CanSubmitDate = smt.can_submit_date,
                CanDesc = smt.can_desc,
                CanBy = smt.can_by,
                CanByName = (smt.can_by is not null and > 0) ? _employeeServices.GetEmployeeName((int)smt.can_by) : "",
                CanDate = smt.can_date,
                CanRemarks = smt.can_remarks,
                FiscalYear = FiscalYear,
                FiscalYearAbb = fiscal_year_abb,
                StartFiscalDate = start_fiscal_date,
                EndFiscalDate = end_fiscal_date
            };

            model.EmployeeSignature = await _requestServices.GetSignatureAsync(model.EmpId).ConfigureAwait(false);
            model.RecommendedSignature = await _requestServices.GetSignatureAsync(model.IAppBy).ConfigureAwait(false);
            model.ApprovedSignature = await _requestServices.GetSignatureAsync(model.AppBy).ConfigureAwait(false);

            // Travel details
            model.TravelExpenses = await GetTravelExpensesDetailsAsync(EmpTravelId).ConfigureAwait(false);

            // Fund sources
            model.TravelFundSources = await GetTravelFundSourcesAsync(EmpTravelId).ConfigureAwait(false);

            // Totals
            CalculateTotals(model);

            return model;
        }
        /****************************************************************************************************/
        private async Task<List<TravelExpenseViewModel>> GetTravelExpensesDetailsAsync(int EmpTravelId)
        {
            return await (
                from detail in _context.tbl_employee_travel_sub.AsNoTracking()
                join particular in _context.tbl_travel_particulars.AsNoTracking()
                    on detail.par_id equals particular.par_id
                    into particularGroup
                from particular in particularGroup.DefaultIfEmpty()
                join currency in _context.tbl_currency.AsNoTracking()
                    on detail.cur_id equals currency.cur_id
                    into currencyGroup
                from currency in currencyGroup.DefaultIfEmpty()
                where detail.emp_travel_id == EmpTravelId

                select new TravelExpenseViewModel
                {
                    ParId = detail.par_id,
                    Particular = particular != null ? particular.particular : "",
                    Detail = detail.detail ?? "",
                    Unit = detail.unit ?? "",
                    CurId = detail.cur_id ?? 0,
                    Currency = currency != null ? currency.cur_abbr : "",
                    Nos = detail.nos ?? 0,
                    Rate = detail.rate ?? 0,
                    Amount = Convert.ToDecimal(Convert.ToDouble(detail.nos)) * (detail.rate ?? 0)
                }
            ).ToListAsync().ConfigureAwait(false);
        }
        /****************************************************************************************************/
        private async Task<List<TravelFundSourceViewModel>> GetTravelFundSourcesAsync(int EmpTravelId)
        {
            return await (
                from code in _context.tbl_employee_travel_codes.AsNoTracking()
                join fund in _context.tbl_fund_source.AsNoTracking()
                    on code.fund_id equals fund.fund_id
                    into fundGroup
                from fund in fundGroup.DefaultIfEmpty()
                where code.emp_travel_id == EmpTravelId
                orderby code.sn
                select new TravelFundSourceViewModel
                {
                    Sn = code.sn,
                    FundId = code.fund_id,
                    FundName = fund != null ? fund.fund_source ?? "" : ""
                }
            ).ToListAsync().ConfigureAwait(false);
        }
        /****************************************************************************************************/
        private static void CalculateTotals(TravelViewModel model)
        {
            model.TotalNRS = model.TravelExpenses.Where(z => z.CurId == 1).Sum(z => z.Amount ?? 0);
            model.TotalIC = model.TravelExpenses.Where(z => z.CurId == 2).Sum(z => z.Amount ?? 0);
            model.TotalUSD = model.TravelExpenses.Where(z => z.CurId == 3).Sum(z => z.Amount ?? 0);
            model.TotalEuro = model.TravelExpenses.Where(z => z.CurId == 4).Sum(z => z.Amount ?? 0);
            model.TotalPound = model.TravelExpenses.Where(z => z.CurId == 5).Sum(z => z.Amount ?? 0);
            model.TotalCHF = model.TravelExpenses.Where(z => z.CurId == 6).Sum(z => z.Amount ?? 0);
        }
        #endregion
        /***************************************************************************************************
        * Since : 2026-Sep-28
        ****************************************************************************************************/
        public SelectList GetTravelStatus(string selvalue = "")
        {
            var options = new Dictionary<string, string>
            {
                { "U", "Unsettled" },
                { "T", "Saved" },
                { "R", "Submitted" },
                { "S", "Settled" },
                { "N", "SNR [Pending]" },
                { "M", "SNR [Verified]" }
            };
            return GblUtilities.BuildSelectList(options, selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Sep-27
        ****************************************************************************************************/
        #region Getting TRAVEL SETTLEMENT VIEWMODEL 
        public async Task<TravelSettlementViewModel> GetTravelSettlementDetailAsync(int EmpTravelId)
        {
            if (EmpTravelId <= 0) { return null; }

            TravelSettlementViewModel model;
            model = new TravelSettlementViewModel();

            var Travel = await GetTravelDetailAsync(EmpTravelId);
            model.Travel = Travel;

            string TravelType = Travel.TravelType ?? "";

            model.TravelSettlement = await GetTravelSettlementMainAsync(EmpTravelId, TravelType);

            return model;
        }
        /****************************************************************************************************
        * Since : 2026-Oct-01
        ****************************************************************************************************/
        private async Task<TravelSettlementMainViewModel> GetTravelSettlementMainAsync(int EmpTravelId, string TravelType)
        {
            var smt = (from main in _context.tbl_employee_travel_settlement_main
                       where main.emp_travel_id == EmpTravelId
                       select new
                       {
                           main.trav_set_id,
                           main.emp_travel_id,
                           main.emp_id,
                           main.submit_date,
                           main.travel_date,
                           main.return_date,
                           main.usd_rate,
                           main.adv_cash_less,
                           main.charge_per_or_amt,
                           main.charge_fund_id_1,
                           main.charge_fund_id_2,
                           main.charge_fund_id_3,
                           main.charge_fund_id_4,
                           main.charge_fund_per_1,
                           main.charge_fund_per_2,
                           main.charge_fund_per_3,
                           main.charge_fund_per_4,
                           main.charge_fund_amt_1,
                           main.charge_fund_amt_2,
                           main.charge_fund_amt_3,
                           main.charge_fund_amt_4,
                           main.remarks,
                           main.app_status,
                           main.app_by,
                           main.app_date,
                           main.is_for_set
                       }).FirstOrDefault();

            if (smt == null) { return null; }

            TravelSettlementMainViewModel model;
            model = new TravelSettlementMainViewModel
            {
                TravSetId = smt.trav_set_id,
                EmpTravelId = smt.emp_travel_id,
                EmpId = smt.emp_id,
                SubmitDate = smt.submit_date,
                TravelDate = smt.travel_date,
                ReturnDate = smt.return_date,
                USDRate = smt.usd_rate,
                AdvCashLess = smt.adv_cash_less,
                ChargePerOrAmt = smt.charge_per_or_amt,
                ChargeFundId1 = smt.charge_fund_id_1,
                ChargeFundName1 = (smt.charge_fund_id_1 is not null and > 0) ? _generalServices.GetFundSourceName((int)smt.charge_fund_id_1) : "",
                ChargeFundId2 = smt.charge_fund_id_2,
                ChargeFundName2 = (smt.charge_fund_id_2 is not null and > 0) ? _generalServices.GetFundSourceName((int)smt.charge_fund_id_2) : "",
                ChargeFundId3 = smt.charge_fund_id_3,
                ChargeFundName3 = (smt.charge_fund_id_3 is not null and > 0) ? _generalServices.GetFundSourceName((int)smt.charge_fund_id_3) : "",
                ChargeFundId4 = smt.charge_fund_id_4,
                ChargeFundName4 = (smt.charge_fund_id_4 is not null and > 0) ? _generalServices.GetFundSourceName((int)smt.charge_fund_id_4) : "",
                ChargeFundPer1 = smt.charge_fund_per_1,
                ChargeFundPer2 = smt.charge_fund_per_2,
                ChargeFundPer3 = smt.charge_fund_per_3,
                ChargeFundPer4 = smt.charge_fund_per_4,
                ChargeFundAmt1 = smt.charge_fund_amt_1,
                ChargeFundAmt2 = smt.charge_fund_amt_2,
                ChargeFundAmt3 = smt.charge_fund_amt_3,
                ChargeFundAmt4 = smt.charge_fund_amt_4,
                TChargeFundPer = (smt.charge_fund_per_1 ?? 0) +
                                 (smt.charge_fund_per_2 ?? 0) +
                                 (smt.charge_fund_per_3 ?? 0) +
                                 (smt.charge_fund_per_4 ?? 0),
                TChargeFundAmt = (smt.charge_fund_amt_1 ?? 0) +
                                 (smt.charge_fund_amt_2 ?? 0) +
                                 (smt.charge_fund_amt_3 ?? 0) +
                                 (smt.charge_fund_amt_4 ?? 0),
                Remarks = smt.remarks,
                AppStatus = smt.app_status,
                AppBy = smt.app_by,
                AppByName = (smt.app_by is not null and > 0) ? _employeeServices.GetEmployeeName((int)smt.app_by) : "",
                AppDate = smt.app_date,
                IsForSet = smt.is_for_set,
                SettlementType =
                    smt.app_status == "T" ? "T" :
                    smt.app_status == "P" && smt.is_for_set == "Y" ? "R" :
                    smt.app_status == "A" && smt.is_for_set == "Y" ? "S" :
                    smt.app_status == "P" && smt.is_for_set == "N" ? "N" :
                    smt.app_status == "A" && smt.is_for_set == "N" ? "M" :
                    "U",
                SettlementTypeDetail =
                        smt.app_status == "T" ? "Saved" :
                        smt.app_status == "P" && smt.is_for_set == "Y" ? "Submitted" :
                        smt.app_status == "A" && smt.is_for_set == "Y" ? "Selttled" :
                        smt.app_status == "P" && smt.is_for_set == "N" ? "SNR [Pending]" :
                        smt.app_status == "A" && smt.is_for_set == "N" ? "SNR [Verified]" :
                        "Unsettled"
            };
            model.EmployeeSignature = await _requestServices.GetSignatureAsync(model.EmpId).ConfigureAwait(false);
            model.ApprovedSignature = await _requestServices.GetSignatureAsync(model.AppBy).ConfigureAwait(false);

            // Sub details
            var SetExpenses = await GetTravelSettlementSubAsync(smt.trav_set_id).ConfigureAwait(false);
            model.SetExpenses = SetExpenses;
            // Calculate total bill amount
            decimal TotalBill = SetExpenses.Sum(z => z.NatBillAmount ?? 0);
            decimal TotalVAT = SetExpenses.Sum(z => z.NatVAT ?? 0);
            decimal TotalTDS = SetExpenses.Sum(z => z.NatTDS ?? 0);
            decimal TotalCash = SetExpenses.Sum(z => z.NatAmount ?? 0);
            if (TravelType == "International") { TotalCash = SetExpenses.Sum(z => z.IntUSDAmount ?? 0); }
            decimal NetCash = Convert.ToDecimal(model.AdvCashLess) - TotalCash;

            model.TotalBill = TotalBill;
            model.TotalVAT = TotalVAT;
            model.TotalTDS = TotalTDS;
            model.TotalCash = TotalCash;
            model.NetCash = NetCash;

            // Docs
            model.SetDocs = await GetTravelSettlementDocAsync(smt.trav_set_id).ConfigureAwait(false);

            return model;
        }
        /****************************************************************************************************
        * Since : 2026-Oct-01
        ****************************************************************************************************/
        private async Task<List<TravelSettlementSubViewModel>> GetTravelSettlementSubAsync(string? TravSetId)
        {
            if (string.IsNullOrWhiteSpace(TravSetId)) { return null; }
            return await (
                from sub in _context.tbl_employee_travel_settlement_sub.AsNoTracking()
                where sub.trav_set_id == TravSetId
                select new TravelSettlementSubViewModel
                {
                    TravSetId = sub.trav_set_id,
                    Sn = sub.sn,
                    BillDate = sub.bill_date,
                    Location = sub.location,
                    Description = sub.description,
                    RefField = sub.RefField,
                    IntCurName = sub.int_cur_name,
                    IntRate = sub.int_rate ?? 0,
                    IntAmount = Convert.ToDecimal(sub.int_amount),
                    IntUSDAmount = Convert.ToDecimal(sub.int_usd_amount),
                    NatBillAmount = Convert.ToDecimal(sub.nat_bill_amount),
                    NatVAT = Convert.ToDecimal(sub.nat_VAT),
                    NatTDS = Convert.ToDecimal(sub.nat_TDS),
                    NatAmount = Convert.ToDecimal(sub.nat_amount),
                }
            ).ToListAsync().ConfigureAwait(false);
        }
        /***************************************************************************************************
        * Since : 2026-Oct-01
        ****************************************************************************************************/
        private async Task<List<TravelSettlementSubDocViewModel>> GetTravelSettlementDocAsync(string? TravSetId)
        {
            if (string.IsNullOrWhiteSpace(TravSetId)) { return null; }
            return await (
                from doc in _context.tbl_employee_travel_settlement_sub_doc.AsNoTracking()
                where doc.trav_set_id == TravSetId
                orderby doc.submit_date
                select new TravelSettlementSubDocViewModel
                {
                    TravSetDocId = doc.trav_set_doc_id,
                    DocName = doc.doc_name,
                    SubmitDate = doc.submit_date,
                    TravSetId = doc.trav_set_id
                }
            ).ToListAsync().ConfigureAwait(false);
        }
        #endregion
        /****************************************************************************************************/
        /****************************************************************************************************/
        /****************************************************************************************************/
        /****************************************************************************************************/
        /****************************************************************************************************/
        /****************************************************************************************************/
        /****************************************************************************************************/
        /****************************************************************************************************/
        /***************************************************************************************************
        * Since : 2026-Sep-27
        ****************************************************************************************************/
    }
}
