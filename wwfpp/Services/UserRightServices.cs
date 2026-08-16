using System.Data;
using System.Text;
using wwfpp.Data;
namespace wwfpp.Services
{
    public class UserRightsServices(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor
    )
    {
        private readonly AppDbContext _context = context;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        // Change this to your actual image path / URL helper
        private const string ImagePath = "/images/";

        // -- Helpers to read session ------------------------------------------
        private string User_id => _httpContextAccessor.HttpContext?.Session.GetString("user_id") ?? "";
        private string User_level => _httpContextAccessor.HttpContext?.Session.GetString("user_level") ?? "";
        private bool IsSuperAdmin => User_id == "1" && User_level == "1";
        /**
         * -- Main public method -----------------------------------------------
         * <summary>
         * Generates an HTML string for the user-rights permission grid.
         * </summary>
         * <param name="parmLevelUserId">
         *   The level_id (when parmLevelOrUser == "level_id")
         *   or user_id  (when parmLevelOrUser == "user_id").
         *   Pass "1" to grant super-admin (all rights).
         * </param>
         * <param name="parmLevelOrUser">"level_id" or "user_id"</param>
         */
        public string GetUserRights(string parmLevelUserId, string parmLevelOrUser)
        {
            /** -- Resolve which permission tables to query ------------------*/
            bool isLevelBased = parmLevelOrUser == "level_id";
            /** -- Load all active modules -----------------------------------**/
            var modules = _context.tbl_user_module.Where(m => m.module_status == "A").OrderBy(m => m.module_sort).ToList();
            var sb = new StringBuilder();
            var arrModuleIds = new List<int>();
            int moCnt = 0;
            foreach (var module in modules)
            {
                moCnt++;
                arrModuleIds.Add(module.module_id);
                /** -- Module checkbox ---------------------------------------**/
                string moduleDetail = IsSuperAdmin ? $"{module.module_name} / {module.module_label} [ {module.module_code} ]" : module.module_label ?? "";
                string chkInputModule;
                string moduleClass;

                if (parmLevelUserId == "1")
                {
                    /** Super-admin: always ticked, always visible*/
                    chkInputModule = $"<img src=\"{ImagePath}right.png\" border=\"0\" />";
                    moduleClass = "block";
                }
                else
                {
                    bool moduleGranted = HasModulePermission(isLevelBased, parmLevelUserId, module.module_id);
                    moduleClass = moduleGranted ? "block" : "displaynone";
                    string mdlCheck = moduleGranted ? "checked" : "";
                    chkInputModule = $"<input type=\"checkbox\" id=\"mchk{module.module_id}\" " +
                                     $"name=\"mchk{module.module_id}\" value=\"Y\" {mdlCheck} " +
                                     $"data-action=\"showHide\" data-chk=\"mchk{module.module_id}\" data-target=\"m{module.module_id}\" />";
                }
                /** -- Module header row -------------------------------------**/
                _ = sb.AppendLine($@"
                    <div class=""divTable w-100"">
                      <div class=""divTableRow bg-sub-head"">
                        <div class=""divTableCell title left w-5"">&nbsp;{chkInputModule}</div>
                        <div class=""divTableCell title left w-55"">&nbsp;{moCnt}. {moduleDetail}</div>
                        <div class=""divTableCell title left"">&nbsp;</div>
                        <div class=""divTableCell title left"">&nbsp;</div>
                        <div class=""divTableCell title left"">&nbsp;</div>
                        <div class=""divTableCell title left"">&nbsp;</div>
                      </div>
                    </div>");
                /** -- Load menus for this module ----------------------------**/
                var menus = _context.tbl_user_menu
                    .Where(m => m.module_id == module.module_id && m.menu_status == "A")
                    .OrderBy(m => m.menu_sort).ToList();
                int menuCount = menus.Count;
                if (menuCount == 0) { continue; }
                /** -- Select-all checkboxes row -----------------------------**/
                string chkModV, chkModA, chkModE, chkModD;
                /** Super-admin: images only, no checkboxes**/
                if (parmLevelUserId == "1")
                {
                    chkModV = chkModA = chkModE = chkModD = "";
                }
                else
                {
                    var (vCnt, aCnt, eCnt, dCnt) = GetMenuPermissionCounts(isLevelBased, parmLevelUserId, module.module_id, menuCount);
                    string vAll = vCnt == menuCount ? "checked" : "";
                    string aAll = aCnt == menuCount ? "checked" : "";
                    string eAll = eCnt == menuCount ? "checked" : "";
                    string dAll = dCnt == menuCount ? "checked" : "";

                    chkModV = $"<input type=\"checkbox\" id=\"mvchk_all{module.module_id}\" name=\"mvchk_all{module.module_id}\" value=\"1\" " +
                              $"data-action=\"checkAll\" data-hidden=\"h{module.module_id}\" data-prefix=\"mvchk{module.module_id}-\" data-allid=\"mvchk_all{module.module_id}\" {vAll} >";
                    chkModA = $"<input type=\"checkbox\" id=\"machk_all{module.module_id}\" name=\"machk_all{module.module_id}\" value=\"1\" " +
                              $"data-action=\"checkAll\" data-hidden=\"h{module.module_id}\" data-prefix=\"machk{module.module_id}-\" data-allid=\"machk_all{module.module_id}\" {aAll} >";
                    chkModE = $"<input type=\"checkbox\" id=\"mechk_all{module.module_id}\" name=\"mechk_all{module.module_id}\" value=\"1\" " +
                              $"data-action=\"checkAll\" data-hidden=\"h{module.module_id}\" data-prefix=\"mechk{module.module_id}-\" data-allid=\"mechk_all{module.module_id}\" {eAll} >";
                    chkModD = $"<input type=\"checkbox\" id=\"mdchk_all{module.module_id}\" name=\"mdchk_all{module.module_id}\" value=\"1\" " +
                              $"data-action=\"checkAll\" data-hidden=\"h{module.module_id}\" data-prefix=\"mdchk{module.module_id}-\" data-allid=\"mdchk_all{module.module_id}\" {dAll} >";
                }

                _ = sb.AppendLine($@"<div id=""m{module.module_id}"" class=""{moduleClass}"">");
                _ = sb.AppendLine($@"
                    <div class=""divTable w-100"">
                      <div class=""divTableRow bg-sub-txt"">
                        <div class=""divTableCell title left w-5"">&nbsp;<input type=""hidden"" name=""h{module.module_id}"" id=""h{module.module_id}"" value=""{menuCount}""></div>
                        <div class=""divTableCell title left w-55"">&nbsp;<i>Select/Deselect&nbsp;</i>&rarr;</div>
                        <div class=""divTableCell title left w-10"">&nbsp;{chkModV}</div>
                        <div class=""divTableCell title left w-10"">&nbsp;{chkModA}</div>
                        <div class=""divTableCell title left w-10"">&nbsp;{chkModE}</div>
                        <div class=""divTableCell title left w-10"">&nbsp;{chkModD}</div>
                      </div>
                    </div>");
                /**  -- Individual menu rows ----------------------------------**/
                int meCnt = 0;
                foreach (var menu in menus)
                {
                    meCnt++;
                    string? menuDetail = IsSuperAdmin
                        ? $"{menu.menu_name} / {menu.menu_label} [ {menu.menu_code} ]"
                        : menu.menu_label;
                    string chkMenuV, chkMenuA, chkMenuE, chkMenuD;
                    /** Super-admin: show tick images**/
                    if (parmLevelUserId == "1")
                    {
                        chkMenuV = $"<img src=\"{ImagePath}right.png\" border=\"0\" />";
                        chkMenuA = chkMenuV;
                        chkMenuE = chkMenuV;
                        chkMenuD = chkMenuV;
                    }
                    else
                    {
                        var (vChk, aChk, eChk, dChk) = GetSingleMenuPermission(
                            isLevelBased, parmLevelUserId, menu.menu_id);

                        chkMenuV = BuildMenuCheckbox("mvchk", module.module_id, meCnt, vChk, "mvchk_all");
                        chkMenuA = BuildMenuCheckbox("machk", module.module_id, meCnt, aChk, "machk_all");
                        chkMenuE = BuildMenuCheckbox("mechk", module.module_id, meCnt, eChk, "mechk_all");
                        chkMenuD = BuildMenuCheckbox("mdchk", module.module_id, meCnt, dChk, "mdchk_all");
                    }

                    _ = sb.AppendLine($@"
                        <div class=""divTable w-100"">
                          <div class=""divTableRow bg-txt"">
                            <div class=""divTableCell normal left w-5"" height=""40"">&nbsp;{meCnt}.
                              <input type=""hidden"" id=""h_menu_id_{module.module_id}_{meCnt}"" name=""h_menu_id_{module.module_id}_{meCnt}"" value=""{menu.menu_id}"">
                            </div>
                            <div class=""divTableCell normal left w-55"">&nbsp;{menuDetail}</div>
                            <div class=""divTableCell normal left w-10"">&nbsp;{chkMenuV}</div>
                            <div class=""divTableCell normal left w-10"">&nbsp;{chkMenuA}</div>
                            <div class=""divTableCell normal left w-10"">&nbsp;{chkMenuE}</div>
                            <div class=""divTableCell normal left w-10"">&nbsp;{chkMenuD}</div>
                          </div>
                        </div>");
                }
                _ = sb.AppendLine("</div>"); /** close <div id="module_id"> */
            } /** foreach module**/
            /** Hidden field with comma-separated module ids (used by JS)**/
            _ = sb.AppendLine($"<input type=\"hidden\" name=\"h_arr_module\" id=\"h_arr_module\" value=\"{string.Join(",", arrModuleIds)}\">");
            return sb.ToString();
        }
        /**  --- Private helpers --------------------------------------------------*/
        /** <summary>Returns true if the level/user has been granted access to a module.</summary>**/
        private bool HasModulePermission(bool isLevelBased, string id, int moduleId)
        {
            if (string.IsNullOrEmpty(id)) { return false; }
            if (isLevelBased)
            {
                return _context.tbl_user_level_module.Any(x => x.level_id == id && x.module_id == moduleId);
            }
            else
            {
                int intId = int.Parse(id);
                return _context.tbl_user_user_module.Any(x => x.user_id == intId && x.module_id == moduleId);
            }
        }
        /** <summary>
         * Returns counts of V/A/E/D permissions for all menus inside a module
         * — used for the select-all row checkboxes.
         * </summary>
         */
        private (int vCnt, int aCnt, int eCnt, int dCnt) GetMenuPermissionCounts(bool isLevelBased, string id, int moduleId, int menuCount)
        {
            if (string.IsNullOrEmpty(id)) { return (0, 0, 0, 0); }
            /** Get menu ids that belong to this module and are active **/
            var menuIds = _context.tbl_user_menu
                .Where(m => m.module_id == moduleId && m.menu_status == "A")
                .Select(m => m.menu_id).ToList();
            if (isLevelBased)
            {
                var perms = _context.tbl_user_level_menu
                    .Where(x => x.level_id == id && menuIds.Contains(x.menu_id)).ToList();
                return (
                    perms.Count(x => x.is_vw == "Y"),
                    perms.Count(x => x.is_ad == "Y"),
                    perms.Count(x => x.is_ed == "Y"),
                    perms.Count(x => x.is_de == "Y")
                );
            }
            else
            {
                int intId = int.Parse(id);
                var perms = _context.tbl_user_user_menu
                    .Where(x => x.user_id == intId && menuIds.Contains(x.menu_id ?? "")).ToList();
                return (
                    perms.Count(x => x.is_vw == "Y"),
                    perms.Count(x => x.is_ad == "Y"),
                    perms.Count(x => x.is_ed == "Y"),
                    perms.Count(x => x.is_de == "Y")
                );
            }
        }
        /**
         * <summary>
         * Returns V/A/E/D checked state ("checked" or "") for a single menu row.
         * </summary>
        */
        private (string vChk, string aChk, string eChk, string dChk) GetSingleMenuPermission(
            bool isLevelBased, string id, string menuId)
        {
            if (string.IsNullOrEmpty(id)) { return ("", "", "", ""); }
            string vChk, aChk, eChk, dChk;
            if (isLevelBased)
            {
                var perm = _context.tbl_user_level_menu.FirstOrDefault(x => x.level_id == id && x.menu_id == menuId);
                vChk = perm?.is_vw == "Y" ? "checked" : "";
                aChk = perm?.is_ad == "Y" ? "checked" : "";
                eChk = perm?.is_ed == "Y" ? "checked" : "";
                dChk = perm?.is_de == "Y" ? "checked" : "";
            }
            else
            {
                int intId = int.Parse(id);
                var perm = _context.tbl_user_user_menu.FirstOrDefault(x => x.user_id == intId && x.menu_id == menuId);
                vChk = perm?.is_vw == "Y" ? "checked" : "";
                aChk = perm?.is_ad == "Y" ? "checked" : "";
                eChk = perm?.is_ed == "Y" ? "checked" : "";
                dChk = perm?.is_de == "Y" ? "checked" : "";
            }
            return (vChk, aChk, eChk, dChk);
        }
        /** <summary>Builds a single menu-level permission checkbox.</summary>*/
        private static string BuildMenuCheckbox(string prefix, int moduleId, int menuIndex, string checkedAttr, string allPrefix)
        {
            string id = $"{prefix}{moduleId}-{menuIndex}";
            string allId = $"{allPrefix}{moduleId}";
            return $"<input type=\"checkbox\" id=\"{id}\" name=\"{id}\" value=\"Y\" {checkedAttr} " +
                   $"data-action=\"checkOne\" data-hidden=\"h{moduleId}\" data-prefix=\"{prefix}{moduleId}-\" data-allid=\"{allId}\" >";
        }
        public void SaveModulesAndMenus(IDictionary<string, string> formValues, string _id, string[] arrModule, string parm_level_or_user)
        {
            /**parm_level_or_user : Level || User */
            foreach (var moduleItem in arrModule)
            {
                if (formValues.TryGetValue(string.Concat("mchk", moduleItem), out var mchk) && mchk == "Y")
                {
                    if (parm_level_or_user == "User")
                    {
                        var UserModule = new tbl_user_user_module
                        {
                            Id = GblUtilities.UniqueID(),
                            user_id = Convert.ToInt32(_id),
                            module_id = int.Parse(moduleItem)
                        };
                        _ = _context.tbl_user_user_module.Add(UserModule);
                    }
                    else
                    {
                        var levelModule = new tbl_user_level_module
                        {
                            Id = GblUtilities.UniqueID(),
                            level_id = _id,
                            module_id = int.Parse(moduleItem)
                        };
                        _ = _context.tbl_user_level_module.Add(levelModule);
                    }

                    int hMenuCnt = int.Parse(formValues["h" + moduleItem]);

                    for (int iCnt = 1; iCnt <= hMenuCnt; iCnt++)
                    {
                        string menu_id = formValues[$"h_menu_id_{moduleItem}_{iCnt}"];

                        string mvchk = formValues.ContainsKey($"mvchk{moduleItem}-{iCnt}") ? formValues[$"mvchk{moduleItem}-{iCnt}"] : "N";
                        string machk = formValues.ContainsKey($"machk{moduleItem}-{iCnt}") ? formValues[$"machk{moduleItem}-{iCnt}"] : "N";
                        string mechk = formValues.ContainsKey($"mechk{moduleItem}-{iCnt}") ? formValues[$"mechk{moduleItem}-{iCnt}"] : "N";
                        string mdchk = formValues.ContainsKey($"mdchk{moduleItem}-{iCnt}") ? formValues[$"mdchk{moduleItem}-{iCnt}"] : "N";

                        mvchk = string.IsNullOrWhiteSpace(mvchk) ? "N" : mvchk;
                        machk = string.IsNullOrWhiteSpace(machk) ? "N" : machk;
                        mechk = string.IsNullOrWhiteSpace(mechk) ? "N" : mechk;
                        mdchk = string.IsNullOrWhiteSpace(mdchk) ? "N" : mdchk;

                        if (!(mvchk == "N" && machk == "N" && mechk == "N" && mdchk == "N"))
                        {
                            if (parm_level_or_user == "User")
                            {
                                var UserMenu = new tbl_user_user_menu
                                {
                                    Id = GblUtilities.UniqueID(),
                                    user_id = Convert.ToInt32(_id),
                                    menu_id = menu_id,
                                    is_vw = mvchk,
                                    is_ad = machk,
                                    is_ed = mechk,
                                    is_de = mdchk
                                };
                                _ = _context.tbl_user_user_menu.Add(UserMenu);
                            }
                            else
                            {
                                var levelMenu = new tbl_user_level_menu
                                {
                                    Id = GblUtilities.UniqueID(),
                                    level_id = _id,
                                    menu_id = menu_id,
                                    is_vw = mvchk,
                                    is_ad = machk,
                                    is_ed = mechk,
                                    is_de = mdchk
                                };
                                _ = _context.tbl_user_level_menu.Add(levelMenu);
                            }
                        }
                    }
                }
            }
            _ = _context.SaveChanges();
        }

    }
}
