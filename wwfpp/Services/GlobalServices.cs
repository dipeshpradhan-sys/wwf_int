using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

//Access on Controller as " GblUtilities.
public static class GblUtilities
{
    private static IHttpContextAccessor? _httpContextAccessor;

    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor
            ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }
    public static HttpContext? CurrentHttpContext => _httpContextAccessor?.HttpContext;
    public static readonly double working_hrs_day_seven = 7; //Up to FY 2016/2017 : Nepal, Bhutan still using  '
    public static readonly double working_hrs_pay_period_seven = 154; // twenty two * seven
    /***************************************************************************************************
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static readonly List<string> Countries =
    [
        "Afghanistan","Albania","Algeria","Andorra","Angola","Antigua and Barbuda","Argentina","Armenia","Australia","Austria","Azerbaijan","Bahamas","Bahrain",
        "Bangladesh","Barbados","Belarus","Belgium","Belize","Benin","Bhutan","Bolivia","Bosnia and Herzegovina","Botswana","Brazil","Brunei","Bulgaria",
        "Burkina Faso","Burundi","Cabo Verde","Cambodia","Cameroon","Canada","Central African Republic","Chad","Chile","China","Colombia","Comoros","Costa Rica",
        "Cote d Ivoire","Croatia","Cuba","Cyprus","Czech Republic","Democratic Republic of the Congo","Denmark","Djibouti","Dominica","Dominican Republic",
        "Ecuador","Egypt","El Salvador","Equatorial Guinea","Eritrea","Estonia","Ethiopia","Fiji","Finland","France","Gabon","Gambia","Georgia","Germany","Ghana","Greece",
        "Grenada","Guatemala","Guinea","Guinea-Bissau","Guyana","Haiti","Honduras","Hungary","Iceland","India","Indonesia","Iran","Iraq","Ireland","Israel","Italy",
        "Jamaica","Japan","Jordan","Kazakhstan","Kenya","Kiribati","Kosovo","Kuwait","Kyrgyzstan","Laos","Latvia","Lebanon","Lesotho","Liberia","Libya","Liechtenstein",
        "Lithuania","Luxembourg","Macedonia","Madagascar","Malawi","Malaysia","Maldives","Mali","Malta","Marshall Islands","Mauritania","Mauritius","Mexico",
        "Micronesia","Moldova","Monaco","Mongolia","Montenegro","Morocco","Mozambique","Myanmar (Burma)","Namibia","Nauru","Nepal","Netherlands","New Zealand",
        "Nicaragua","Niger","Nigeria","North Korea","Norway","Oman","Pakistan","Palau","Palestine","Panama","Papua New Guinea","Paraguay","Peru","Philippines","Poland",
        "Portugal","Qatar","Republic of the Congo","Romania","Russia","Rwanda","Samoa","San Marino","Sao Tome and Principe","Saudi Arabia","Senegal","Serbia",
        "Seychelles","Sierra Leone","Singapore","Slovakia","Slovenia","Solomon Islands","Somalia","South Africa","South Korea","South Sudan","Spain","Sri Lanka",
        "St. Kitts and Nevis","St. Lucia","St. Vincent and the Grenadines","Sudan","Suriname","Swaziland","Sweden","Switzerland","Syria","Taiwan","Tajikistan",
        "Tanzania","Thailand","Timor-Leste","Togo","Tonga","Trinidad and Tobago","Tunisia","Turkey","Turkmenistan","Tuvalu","Uganda","Ukraine",
        "United Arab Emirates","United Kingdom","United States of America","Uruguay","Uzbekistan","Vanuatu","Vatican City","Venezuela","Vietnam",
        "Yemen","Zambia","Zimbabwe"
        /*
          csharp
          ViewBag.CountryList = new SelectList(CountryList.Countries, selectedValue);;//Pass the list to the view using SelectList:
        
          Razor View (.cshtml)
                @Html.DropDownList("Country", (SelectList)ViewBag.CountryList, "Select Country", new { @class = "combobox" })
          or 
                <select asp-for="Country" asp-items="ViewBag.CountryList" class="combobox">
                    <option value="">Select Country</option>
                </select>
         Model binding
                public string Country { get; set; }
        */
    ];
    public static SelectList GetCountries(string selvalue = "")
    {
        var options = Countries.ToDictionary(c => c, c => c);
        return BuildSelectList(options, selvalue);
    }
    /***************************************************************************************************
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    private static readonly HashSet<string> AllowedHtmlTags = ["br", "b", "i", "u"];
    private const string PassOne = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789#_@~`!$%^&*()-{}[]|/?.<>,:;+=";
    private const string PassTwo = "abc9#VWXde4rs015fgEFGHhijQRST36UYZklmnoIJKp78_tMuLvwxyzABCDq2NOP@~`!$%^&*()-{}[]|/?.<>,:;+=";
    private const string PossChr = "23456789ABDEFHKLMNOPRSTUVWXZabdefghikmnopqrstuvwxyz";/*avoid confusing characters (l 1 and i for example) for captcha */
    private static readonly string[] MonthNames = [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];
    private static readonly string[] MonthNamesLocale = [
       // Define your own month names (can be localized)
        "Baishakh", "Jestha", "Ashadh", "Shrawan", "Bhadra", "Ashwin",
        "Kartik", "Mangsir", "Poush", "Magh", "Falgun", "Chaitra"
    ];
    /***************************************************************************************************
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static string PossibleCaptchaLetters()
    {
        return PossChr;
    }
    /***************************************************************************************************
    * Add number(s) of space specified
    * Example in a controller : string spaces = GblUtilities.Spaces(10);
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static string Spaces(int parm)
    {
        string fnStr = "";
        for (int fCnt = 1; fCnt <= parm; fCnt++) { fnStr += "&nbsp;"; }
        return fnStr;
    }
    /***************************************************************************************************
    * Validate email address
    * Example in a controller : bool result = GblUtilities.ValidateEmail( email ); 
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static bool ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) { return false; }

        /** Simple regex for email validation*/
        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }
    /***************************************************************************************************
    * encrypt
    * Only to Jumble provided strings
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static string Encode(string input)
    {
        var result = new StringBuilder();
        foreach (char c in input)
        {
            int index = PassTwo.IndexOf(c, StringComparison.Ordinal);
            if (index >= 0)
            {
                _ = result.Append(PassOne[index]);
            }
        }
        return result.ToString();
    }
    /***************************************************************************************************
    * decript
    * Only to de-jumble provided strings
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static string Decode(string input)
    {
        var result = new StringBuilder();
        foreach (char c in input)
        {
            int index = PassOne.IndexOf(c, StringComparison.Ordinal);
            if (index >= 0)
            {
                _ = result.Append(PassTwo[index]);
            }
        }
        return result.ToString();
    }
    /***************************************************************************************************
    * To display valid messages on div
    * Example in a controller : string result = GblUtilities.MsgDisplay( msgst, selRows, delRows ); 
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static string MsgDisplay(string pmsgst, string SRows = "", string DRows = "")
    {
        string FnMessage = "";
        string Fn_class;
        string? Fn_msg;
        string Parm_msgst = pmsgst;

        int Selected_rows = int.TryParse(SRows, out int SelRows) ? SelRows : 0;
        int Deleted_rows = int.TryParse(DRows, out int DelRows) ? DelRows : 0;
        int ttl_sel_rows; int ttl_del_rows; int un_del_rows;

        if (Parm_msgst == "logout") { Fn_class = "success"; Fn_msg = "You are logged out successfully"; }
        else if (Parm_msgst == "sessionout") { Fn_class = "error"; Fn_msg = "Session expired. Please login again."; }
        else if (Parm_msgst == "invalidlogin") { Fn_class = "error"; Fn_msg = "Invalid login. Please try again"; }
        else if (Parm_msgst == "addsuccess") { Fn_class = "success"; Fn_msg = "Record(s) added successfully."; }
        else if (Parm_msgst is "updatesuccess" or "uploadsuccess") { Fn_class = "success"; Fn_msg = "Record(s) updated successfully."; }
        else if (Parm_msgst == "activesuccess") { Fn_class = "success"; Fn_msg = "Record(s) activated successfully."; }
        else if (Parm_msgst == "deactivesuccess") { Fn_class = "success"; Fn_msg = "Record(s) deactivated successfully."; }
        else if (Parm_msgst == "deletesuccess") { Fn_class = "success"; Fn_msg = "Record(s) deleted successfully."; }
        else if (Parm_msgst == "deletesome")
        {
            ttl_sel_rows = !string.IsNullOrWhiteSpace(Selected_rows.ToString()) ? Selected_rows : 0;
            ttl_del_rows = !string.IsNullOrWhiteSpace(Deleted_rows.ToString()) ? Deleted_rows : 0;
            un_del_rows = ttl_sel_rows - ttl_del_rows;
            Fn_class = "error";
            Fn_msg = string.Concat(Deleted_rows.ToString(), " record(s) deleted and ", un_del_rows.ToString(), " record(s) not deleted. The record(s) may be in use in another place. Please check and try again.");
        }
        else if (Parm_msgst == "deletesomepending")
        {
            ttl_sel_rows = !string.IsNullOrWhiteSpace(Selected_rows.ToString()) ? Selected_rows : 0;
            ttl_del_rows = !string.IsNullOrWhiteSpace(Deleted_rows.ToString()) ? Deleted_rows : 0;
            un_del_rows = ttl_sel_rows - ttl_del_rows;
            Fn_class = "error";
            Fn_msg = string.Concat(Deleted_rows.ToString(), " pending record(s) deleted. ", un_del_rows.ToString(), " non pending record(s), cannot delete.");
        }
        else if (Parm_msgst == "deletenone") { Fn_class = "error"; Fn_msg = "(0) record(s) deleted. The record may be in use in another place. Please check and try again."; }
        else if (Parm_msgst == "deletenonepending") { Fn_class = "error"; Fn_msg = "Non pending record(s), cannot delete."; }
        else if (Parm_msgst == "exists") { Fn_class = "error"; Fn_msg = "Record(s) already exist in another record."; }
        else if (Parm_msgst == "exist") { Fn_class = "error"; Fn_msg = "Request already exists for selected date range!"; }
        else if (Parm_msgst == "error") { Fn_class = "error"; Fn_msg = "Error in process."; }
        else if (Parm_msgst == "cancelsuccess") { Fn_class = "success"; Fn_msg = "Cancellation request has been submitted successfully."; }
        else if (Parm_msgst == "cancelerror") { Fn_class = "success"; Fn_msg = "Error while submitting cancellation request."; }
        else if (Parm_msgst == "discardsuccess") { Fn_class = "success"; Fn_msg = "Cancellation request has been discarded successfully."; }
        else if (Parm_msgst == "discarderror") { Fn_class = "error"; Fn_msg = "Error while dicarding cancellation request."; }
        else if (Parm_msgst == "hourshort") { Fn_class = "error"; Fn_msg = "Error while storing data.Used hours is greater than provided annual hours of employee."; }
        else if (Parm_msgst == "carryforwardsuccess") { Fn_class = "success"; Fn_msg = "Leave carry forwarded successfully."; }
        else if (Parm_msgst == "errorfiscalyear") { Fn_class = "success"; Fn_msg = "Mismatched in fiscal year and/or date."; }
        else if (Parm_msgst == "importsuccess") { Fn_class = "success"; Fn_msg = "CSV file imported successfully."; }
        else if (Parm_msgst == "fgtpasssuccess") { Fn_class = "success"; Fn_msg = "Your password reset link has been sent successfully. Please check your email inbox (and spam folder if needed) to continue resetting your password. Follow the instructions in the email to securely update your account access."; }
        else if (Parm_msgst == "fgtpassfail") { Fn_class = "error"; Fn_msg = "Oops! The information provided doesn’t look correct. Please re‑enter your correct details to continue with password recovery."; }
        else if (Parm_msgst == "fgtpinsuccess") { Fn_class = "success"; Fn_msg = "Your PIN reset link has been sent successfully. Please check your email inbox (and spam folder if needed) to continue resetting your PIN. Follow the instructions in the email to securely update your account access."; }
        else if (Parm_msgst == "fgtpinfail") { Fn_class = "error"; Fn_msg = "Oops! The information provided doesn’t look correct. Please re‑enter your correct details to continue with PIN recovery."; }
        else if (Parm_msgst == "importerror")
        {
            if (_httpContextAccessor?.HttpContext == null)
            {
                Fn_class = "error";
                Fn_msg = "No active HttpContext";// avoid NullReferenceException
            }
            else
            {
                Fn_class = "success";
                Fn_msg = _httpContextAccessor.HttpContext.Session.GetString("str_import_error"); /* this comes with <p> tag*/
                _httpContextAccessor.HttpContext.Session.SetString("str_import_error", "");/* making session empty*/
            }
        }
        else
        {
            Fn_class = "displaynone";
            Fn_msg = "&nbsp;";
        }

        if (!string.IsNullOrWhiteSpace(Fn_class) && !string.IsNullOrWhiteSpace(Fn_msg))
        {
            if (Parm_msgst == "importerror")
            {
                FnMessage = Fn_msg; /* this comes with <p> tag*/
            }
            else
            {
                FnMessage = @"<p id=""message"" class=""[<FN-CLASS>]"">[<FN-MSG>]</p><br/>";
                FnMessage = FnMessage.Replace("[<FN-CLASS>]", Fn_class, StringComparison.Ordinal);
                FnMessage = FnMessage.Replace("[<FN-MSG>]", Fn_msg, StringComparison.Ordinal);
            }
        }
        if (string.IsNullOrWhiteSpace(FnMessage)) { FnMessage = ""; }
        return FnMessage;
    }
    /***************************************************************************************************
    * To insert semicolon between to strings
    *  Example in a controller : string result = GblUtilities.PutSemiColon( string1, string2 ); 
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static string PutSemiColon(string pram, string parmText)
    {
        return string.IsNullOrWhiteSpace(pram) ? parmText : string.Concat(pram, ";", parmText);
    }
    /***************************************************************************************************
    * To get Unique ID
    *  Format: yyyyMMddHHmmssfff (year, month, day, hour, minute, second, milliseconds)
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static string UniqueID()
    {
        return string.Concat(DateTime.Now.ToString("yyyyMMddHHmmssfff"), RandomNumberGenerator.GetInt32(100000, 999999).ToString());
    }
    /***************************************************************************************************
    * To randomise characters
    *  
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    private static string RandomChars(string chars, int count)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < count; i++)
        {
            int index = RandomNumberGenerator.GetInt32(chars.Length);
            _ = sb.Append(chars[index]);
        }
        return sb.ToString();
    }
    /***************************************************************************************************
    * To suffle text
    *  
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    private static string Shuffle(string input)
    {
        char[] array = input.ToCharArray();
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
        return new string(array);
    }
    /***************************************************************************************************
    * To get random value
    *  
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static string GetRndUniqueText(int pSize, string? pType)
    {
        int size = (string.IsNullOrWhiteSpace(pSize.ToString()) || pSize < 12) ? 12 : pSize;
        var sb = new StringBuilder();

        string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string lower = "abcdefghijklmnopqrstuvwxyz";
        string digits = "0123456789";
        string special = "!@#$+-*&?:{}[]()?";

        if (pType == "N")
        {
            _ = sb.Append(RandomChars(digits, size));
        }
        else if (pType == "A")
        {
            _ = sb.Append(RandomChars(upper + lower, size));
        }
        else if (pType == "AN")
        {
            _ = sb.Append(RandomChars(upper, 3));
            _ = sb.Append(RandomChars(lower, size - 6));
            _ = sb.Append(RandomChars(digits, 3));
        }
        else
        {
            /** AN + SPC **/
            _ = sb.Append(RandomChars(upper, 2));
            _ = sb.Append(RandomChars(lower, size - 7));
            _ = sb.Append(RandomChars(digits, 3));
            _ = sb.Append(RandomChars(special, 2));
        }
        return Shuffle(sb.ToString());// Shuffle the characters to avoid predictable grouping
    }
    /***************************************************************************************************
    * GET ONE DECIMAL PLACE VALUE OF NUMBER AS
    * ROUND 0 if DECIMAL VALUE IS 0, 1, 2, 3 OR 4 [1.0, 1.1, 1.2, 1.3, 1.4 all set to 1.0]
    * ROUND 5 if DECIMAL VALUE IS 5, 6, 7, 8 OR 9 [1.5, 1.6, 1.7, 1.8, 1.9 all set to 1.5]
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static double SetSingleDecimalValueToZeroOrFive(double value)
    {
        double x = Math.Floor(value);/** Take the integer part*/
        double y = Math.Round(value - x, 1);/** Get the fractional part rounded to 1 decimal place */
        double z = y == 0.5 ? value : y < 0.5 ? x : x + 0.5;
        return z;
    }
    /***************************************************************************************************
    * get string part if string is too long with ...
    *  
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static string GetStringPart(string Input, int Len)
    {
        return !string.IsNullOrWhiteSpace(Input) && Input.Length > Len ? string.Concat(Input.AsSpan(0, Len), "...") : Input;
    }
    /***************************************************************************************************
    * format date with given format by adding 0 before single digit'
    *  
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static string AddLeadingZero(string parm_value)
    {
        return int.TryParse(parm_value, out int ADgt) ? ADgt < 10 ? string.Concat("0", parm_value) : parm_value : parm_value;
    }
    /***************************************************************************************************
    * 
    *  
    * Since : 2026-Jun-01
    ****************************************************************************************************/
    public static string HelpTips(string Tips)
    {
        if (string.IsNullOrWhiteSpace(Tips)) { return string.Empty; }
        string safeTips = WebUtility.HtmlEncode(Tips);

        // Whitelist replacements
        foreach (string tag in AllowedHtmlTags)
        {
            safeTips = safeTips.Replace($"&lt;{tag}&gt;", $"<{tag}>", StringComparison.Ordinal)
                       .Replace($"&lt;/{tag}&gt;", $"</{tag}>", StringComparison.Ordinal);
        }
        safeTips = safeTips.Replace("&lt;br/&gt;", "<br/>", StringComparison.Ordinal);/* Special case for self-closing <br/> */

        string FnStr = "";
        if (!string.IsNullOrWhiteSpace(Tips))
        {
            FnStr = $@"
                    <div id = ""main-box-message-2"" class=""relative"">
                        <p>
                            <a href = ""#"" id=""helpLink"">
                               <img id=""imgHelp"" src=""/images/help_icon.png"" border =""0"" />
                            </a>
                        </p>
                        <div id = ""divHelp"" class=""displaynone"">
                            <div class=""note"">{safeTips}</div>
                        </div>
                     </div>    
                     <br/>                    
                    ";
        }
        return FnStr;
    }
    /***************************************************************************************************
    * Since : 2026-Jun-14
    ****************************************************************************************************/
    public static string EscapeCSV(string field)
    {
        if (string.IsNullOrEmpty(field)) { return ""; }
        field = field.Replace(",", ";", StringComparison.Ordinal);
        bool mustQuote = field.Contains(',', StringComparison.Ordinal) || field.Contains('\"', StringComparison.Ordinal) || field.Contains('\n', StringComparison.Ordinal);
        string escaped = field.Replace("\"", "\"\"", StringComparison.Ordinal);
        return mustQuote ? $"\"{escaped}\"" : escaped;
    }
    /***************************************************************************************************
    * Since : 2026-Jun-14
    ****************************************************************************************************/
    public static string GetFileSize(string fileName)
    {
        string fileSize = "";
        var fileInfo = new FileInfo(fileName);
        if (fileName != null && File.Exists(fileName))
        {
            long sizeInBytes = fileInfo.Length;
            double sizeInKb = Math.Round(sizeInBytes / 1024.0);
            double sizeInMb = Math.Round(sizeInBytes / (1024.0 * 1024.0));
            fileSize = sizeInBytes > 1024
                ? sizeInKb > 1024 ? sizeInMb.ToString() + "MB" : sizeInKb.ToString() + "KB"
                : sizeInBytes.ToString() + "Bytes";
        }
        return fileSize;
    }
    /***************************************************************************************************
    * Since : 2026-Jun-16
    ****************************************************************************************************/
    public static string ToProperCase(string parm)
    {
        return string.IsNullOrWhiteSpace(parm) ? parm : string.Concat(char.ToUpperInvariant(parm[0]), parm[1..].ToLower(CultureInfo.CurrentCulture));
    }
    /***************************************************************************************************
    * Choose month set based on locale
    * Since : 2026-Jun-16
    ****************************************************************************************************/
    public static string[] PossibleMonths(string locale = "en")
    {
        return locale.Equals("ne", StringComparison.OrdinalIgnoreCase) ? MonthNamesLocale : MonthNames;
    }
    /***************************************************************************************************
    * Since : 2026-Jun-26
    ****************************************************************************************************/
    public static string MonthName(int month)
    {
        string[] monthNames = PossibleMonths("en");
        return month is < 1 or > 12 ? string.Empty : monthNames[month - 1];
    }
    /***************************************************************************************************
    * Since : 2026-Jun-26
    ****************************************************************************************************/
    public static string DebugModelState(ModelStateDictionary modelState)
    {
        var errors = modelState
            .Where(static kvp => kvp.Value.Errors.Count > 0)
            .Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}");
        string result = string.Join(Environment.NewLine, errors);
        System.Diagnostics.Debug.WriteLine(result);
        return result;
    }
    /***************************************************************************************************
    * 
    * Since : 2026-Jun-29
    ****************************************************************************************************/
    public static int CryptoRandomNext(int maxExclusive)
    {
        if (maxExclusive > 0)
        {
            byte[] bytes = new byte[4];
            RandomNumberGenerator.Fill(bytes);
            uint value = BitConverter.ToUInt32(bytes, 0);
            return (int)(value % maxExclusive);
        }
        else
        {
            return 0;
        }
    }
    /***************************************************************************************************
    * Since : 2026-Jun-29
    ****************************************************************************************************/
    internal static class DataTableHelper
    {
        public static (int PageSize, int Skip, string Draw, string SortColumn, string SortDir, string SearchValue)
        GetParameters(HttpRequest request)
        {
            int pageSize = 0;
            int skip = 0;
            string draw = "10";
            string sortColumn = "";
            string sortColumnDir = "asc";
            string searchValue = "";
            if (request != null)
            {
                draw = request.Form["draw"].FirstOrDefault() ?? "10";
                string start = request.Form["start"].FirstOrDefault() ?? "0";
                string length = request.Form["length"].FirstOrDefault() ?? "10";
                sortColumn = request.Form["columns[" + request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault() ?? "";
                sortColumnDir = request.Form["order[0][dir]"].FirstOrDefault() ?? "asc";
                searchValue = request.Form["search[value]"].FirstOrDefault() ?? "";

                pageSize = !string.IsNullOrWhiteSpace(length) ? Convert.ToInt32(length) : 0;
                skip = !string.IsNullOrWhiteSpace(start) ? Convert.ToInt32(start) : 0;
            }
            return (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue);
        }
    }
    /***************************************************************************************************
    * Since : 2026-Jun-30
    ****************************************************************************************************/
    public static void UploadFile(string Folder, IFormFile file, out string uStatus, out string? newFileName)
    {
        uStatus = "false";
        newFileName = "";
        string uploadsFolder = Folder;
        if (file != null)
        {
            string safeFileName = Path.GetFileName(file.FileName);
            string extension = Path.GetExtension(safeFileName).ToLower(CultureInfo.CurrentCulture);
            if (string.IsNullOrWhiteSpace(extension))
            {
                uStatus = "false";
            }
            if (!Directory.Exists(uploadsFolder))
            {
                _ = Directory.CreateDirectory(uploadsFolder);
            }
            string uniqueFileName = string.Concat(UniqueID(), extension);
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            string fullPathResolved = Path.GetFullPath(filePath);
            string baseDirectoryResolved = Path.GetFullPath(uploadsFolder + Path.DirectorySeparatorChar);
            if (!fullPathResolved.StartsWith(baseDirectoryResolved, StringComparison.OrdinalIgnoreCase))
            {
                uStatus = "false";
            }
            else
            {
                using var stream = new FileStream(filePath, FileMode.Create);
                file.CopyTo(stream);
                uStatus = "true";
                newFileName = uniqueFileName;
            }
        }
    }
    /***************************************************************************************************
    * Since : 2026-Jun-30
    ****************************************************************************************************/
    public static string DeleteFile(string Folder, string File)
    {
        string uploadsFolder = Folder;
        string UploadFile = File ?? "";
        if (!string.IsNullOrWhiteSpace(UploadFile))
        {
            string filePath = Path.Combine(uploadsFolder, UploadFile);
            string fullPathResolved = Path.GetFullPath(filePath);
            string baseDirectoryResolved = Path.GetFullPath(uploadsFolder + Path.DirectorySeparatorChar);

            if (fullPathResolved.StartsWith(baseDirectoryResolved, StringComparison.OrdinalIgnoreCase))
            {
                if (System.IO.File.Exists(fullPathResolved))
                {
                    System.IO.File.Delete(fullPathResolved);
                }
                if (System.IO.File.Exists(fullPathResolved))
                {
                    return "false";
                }
            }
        }
        return "true";
    }
    /***************************************************************************************************
    * Since : 2026-Jul-09
    ****************************************************************************************************/
    public static SelectList BuildSelectList(Dictionary<string, string> options, string selectedValue = "")
    {
        var list = options.Select(o => new { Value = o.Key, Text = o.Value }).ToList();
        return new SelectList(list, "Value", "Text", selectedValue);
    }
    /***************************************************************************************************
    * Since : 2026-Jun-14
    ****************************************************************************************************/
    public static SelectList StatusActivePassive(string Type = "", string selvalue = "")
    {
        var options = new Dictionary<string, string> { { "Y", "Yes" }, { "N", "No" } };
        if (Type == "AP")
        {
            options = new Dictionary<string, string> { { "A", "Active" }, { "P", "Passive" } };
        }
        else if (Type == "AD")
        {
            options = new Dictionary<string, string> { { "A", "Active" }, { "D", "InActive" } };
        }
        else if (Type == "YNAD")
        {
            options = new Dictionary<string, string> { { "Y", "Active" }, { "N", "InActive" } };
        }
        return BuildSelectList(options, selvalue);
    }
    /***************************************************************************************************
    * Since : 2026-Jun-19
    ****************************************************************************************************/
    public static SelectList StatusOpenLocked(string selvalue = "")
    {
        var options = new Dictionary<string, string> { { "Y", "Open" }, { "N", "Locked" } };
        return BuildSelectList(options, selvalue);
    }
    /***************************************************************************************************
    * Since : 2026-Jun-24
    ****************************************************************************************************/
    public static SelectList ApprovalStatus(string selvalue = "")
    {
        var options = new Dictionary<string, string>
        {
            { "Pending", "Pending" },
            { "Approved", "Approved" },
            { "Declined", "Declined" }
        };
        return BuildSelectList(options, selvalue);
    }
    /***************************************************************************************************
    * Since : 2026-Jul-04
    ****************************************************************************************************/
    public static SelectList GenderList(string selvalue = "")
    {
        var options = new Dictionary<string, string> { { "M", "Male" }, { "F", "Female" } };
        return BuildSelectList(options, selvalue);
    }
    /***************************************************************************************************
    * Since : 2026-Jul-04
    ****************************************************************************************************/
    public static SelectList MaritalStatusList(string selvalue = "")
    {
        var options = new Dictionary<string, string> { { "S", "Single" }, { "M", "Married" } };
        return BuildSelectList(options, selvalue);
    }
    /***************************************************************************************************
    * Since : 2026-Jul-04
    ****************************************************************************************************/
    public static SelectList EmpPayStatusList(string selvalue = "")
    {
        var options = new Dictionary<string, string> { { "Y", "Show" }, { "N", "Hide" } };
        return BuildSelectList(options, selvalue);
    }
    /***************************************************************************************************
    * Since : 2026-Jul-08
    ****************************************************************************************************/
    public static SelectList GetLeaveUnit(string selvalue = "")
    {
        var options = new Dictionary<string, string> { { "hours", "Hours" }, { "days", "Days" } };
        return BuildSelectList(options, selvalue);
    }
    /***************************************************************************************************
    * Since : 2026-Jul-10
    ****************************************************************************************************/
    public static SelectList GetPeriod(string selvalue = "0")
    {
        var options = new Dictionary<string, string> { { "1", "Period 1" }, { "2", "Period 2" } };
        return BuildSelectList(options, selvalue);
    }
    /***************************************************************************************************
    * Since : 2026-Jul-15
    ****************************************************************************************************/
    public static SelectList GetHoursMinutes(string selvalue = "")
    {
        var options = new Dictionary<string, string>();
        int increment = 5;

        for (int hour = 7; hour <= 11; hour++)
        {
            string fnStrHrs = hour.ToString("D2"); // pad with leading zero

            for (int jCnt = 0; jCnt <= 11; jCnt++)
            {
                int actVal = jCnt * increment;
                string fnStrMin = actVal.ToString("D2"); // pad with leading zero

                string value = $"{fnStrHrs}:{fnStrMin}";
                options.Add(value, value);
            }
        }
        return BuildSelectList(options, selvalue);
    }
    /***************************************************************************************************
    * Since : 2026-Jul-15
    ****************************************************************************************************/
    public static SelectList GetAMPM(string selvalue = "")
    {
        var options = new Dictionary<string, string> { { "AM", "AM" }, { "PM", "PM" } };
        return BuildSelectList(options, selvalue);
    }
    /***************************************************************************************************
    * Since : 2026-Jul-18
    ****************************************************************************************************/
    public static SelectList GetReportDisplayType(string selvalue = "1")
    {
        var options = new Dictionary<string, string> { { "1", "Preview" }, { "2", "Export" } };
        return BuildSelectList(options, selvalue);
    }
    /***************************************************************************************************
    * Since : 2026-Jul-18
    ****************************************************************************************************/
    public static SelectList GetReportType(string selvalue = "1")
    {
        var options = new Dictionary<string, string> { { "1", "Summary" }, { "2", "Detail" } };
        return BuildSelectList(options, selvalue);
    }
    /***************************************************************************************************
    * Since : 2026-Jul-24
    ****************************************************************************************************/
    public static int GetDateDiffDays(DateTime? start, DateTime? end)
    {
        if (string.IsNullOrWhiteSpace(start.ToString()) || string.IsNullOrWhiteSpace(end.ToString())) { return 0; }
        DateTime newstart = start ?? DateTime.MinValue;
        DateTime newend = end ?? DateTime.MinValue;
        return (newend - newstart).Days + 1;
    }
}

