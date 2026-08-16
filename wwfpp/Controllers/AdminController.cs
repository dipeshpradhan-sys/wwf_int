using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Dynamic.Core;
using wwfpp.Data;
using wwfpp.EmailServices;
using wwfpp.Models;
using wwfpp.Models.Admin;
using wwfpp.Services;
using static GblUtilities;

namespace wwfpp.Controllers
{
    public class AdminController(
        AppDbContext context,
        EmailService emailService,
        AccountServices accountServices,
        AdminServices adminServices
        ) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly EmailService _emailService = emailService;
        private readonly AccountServices _accountServices = accountServices;
        private readonly AdminServices _adminServices = adminServices;

        #region APPLICATION SETTINGS 
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult ApplicationSettings()
        {
            #region FOR PERMISSION
            string PageId = "1";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_pp_options
                orderby a.option_id ascending
                select new ApplicationSettingsViewModel
                {
                    option_id = a.option_id,
                    option_name = a.option_name ?? "",
                    option_value = a.option_value ?? "",
                    autoload = a.autoload,
                    option_note = a.option_note
                }).AsNoTracking().ToList();
            ViewBag.autoload = StatusActivePassive("YN");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Admin/ApplicationSettings", "ADD|DEL", PageId, Records.Count);
            return PartialView("Admin/_ApplicationSettings", Records);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplicationSettingsList()
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var query = _context.tbl_pp_options
                .OrderByDescending(a => a.option_id)
                .Select(a => new ApplicationSettingsViewModel
                {
                    option_id = a.option_id,
                    option_name = a.option_name ?? "",
                    option_value = a.option_value ?? "",
                    autoload = a.autoload,
                    option_note = a.option_note
                });

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(
                    a => a.option_name.Contains(searchValue) ||
                    a.option_value.Contains(searchValue)
                );
            }
            var data = query.ToList();

            int totalRecord = data.Count;
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };

            return new JsonResult(jsonData);

        }
        public IActionResult ApplicationSettingsAddEdit(int? id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "1";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.AutoLoad = StatusActivePassive("YN");
            ViewBag.mode = mode;
            ApplicationSettingsViewModel model;
            //this is to load blank form while doing add process
            model = new ApplicationSettingsViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Admin/_ApplicationSettingsAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id.ToString()) || id < 1)
                {
                    //return error
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = _context.tbl_pp_options
                        .FirstOrDefault(h => h.option_id == Convert.ToInt32(id));

                    if (smt == null)
                    {
                        //return error message;
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        model = new ApplicationSettingsViewModel
                        {
                            option_id = Convert.ToInt32(smt.option_id),
                            option_name = smt.option_name ?? "",
                            option_value = smt.option_value ?? "",
                            autoload = smt.autoload,
                            option_note = smt.option_note
                        };
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        return PartialView("Admin/_ApplicationSettingsAddEdit", model);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ApplicationSettingsSave(ApplicationSettingsViewModel model)
        {
            /**
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("option_id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }

            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("1", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string option_name = model.option_name;
            string option_value = model.option_value;
            string autoload = model.autoload ?? "n";
            string option_note = model.option_note ?? "";

            if (mode == "add")
            {
                /** check if the data is exits on another record */
                var isData = _context.tbl_pp_options.FirstOrDefault(u => u.option_name == option_name);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }

                /** get maximum option id **/
                int option_id = (_context.tbl_pp_options.Any()
                                ? _context.tbl_pp_options.Max(o => o.option_id)
                                : 0) + 1;
                var DataSave = new tbl_pp_options
                {
                    option_id = option_id,
                    option_name = option_name,
                    option_value = option_value,
                    autoload = autoload,
                    option_note = option_note
                };
                _ = _context.tbl_pp_options.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id = option_id });
            }
            else if (mode == "edit")
            {
                int option_id = model.option_id;
                var isData = _context.tbl_pp_options.FirstOrDefault(u =>
                        u.option_name == option_name &&
                        u.option_id != option_id
                        );
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                var DataUpdate = _context.tbl_pp_options.FirstOrDefault(h => h.option_id == option_id);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }

                DataUpdate.option_name = option_name;
                DataUpdate.option_value = option_value;
                DataUpdate.autoload = autoload;
                DataUpdate.option_note = option_note;

                _ = _context.tbl_pp_options.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.option_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplicationSettingsDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("1", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            // Validate input
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }

            var recordsToDelete = await _context.tbl_pp_options.Where(r => request.SelectedIds.Contains(r.option_id.ToString())).ToListAsync().ConfigureAwait(false);
            if (recordsToDelete.Count == 0)
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }

            _context.tbl_pp_options.RemoveRange(recordsToDelete);
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);

            return Ok(new
            {
                status = "success",
                deletedCount = recordsToDelete.Count,
                message = Lang.msg_delete_success
            });
        }

        #endregion


        #region MODULES
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult Modules()
        {
            #region FOR PERMISSION
            string PageId = "3";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_user_module
                orderby a.module_id ascending
                select new ModulesViewModel
                {
                    module_id = a.module_id,
                    module_code = a.module_code,
                    module_name = a.module_name,
                    module_label = a.module_label,
                    module_folder = a.module_folder,
                    module_sort = a.module_sort,
                    module_status = a.module_status,
                }).AsNoTracking().ToList();

            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Admin/Modules", "ADD|DEL", PageId, Records.Count);
            return PartialView("Admin/_Modules", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModulesList()
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var query = _context.tbl_user_module
                .OrderByDescending(a => a.module_id)
                .Select(a => new ModulesViewModel
                {
                    module_id = a.module_id,
                    module_code = a.module_code,
                    module_name = a.module_name,
                    module_label = a.module_label,
                    module_folder = a.module_folder,
                    module_sort = a.module_sort,
                    module_status = a.module_status,
                });

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a => (a.module_code != null && a.module_code.Contains(searchValue)) ||
                    (a.module_name != null && a.module_name.Contains(searchValue)) ||
                    (a.module_label != null && a.module_label.Contains(searchValue)) ||
                    (a.module_folder != null && a.module_folder.Contains(searchValue))
                );
            }
            var data = query.ToList();

            int totalRecord = data.Count;
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };

            return new JsonResult(jsonData);

        }
        public IActionResult ModulesAddEdit(int? id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "3";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.ModuleStatus = StatusActivePassive("AP");
            ViewBag.mode = mode;
            ModulesViewModel model;
            /** this is to load blank form while doing add process */
            model = new ModulesViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Admin/_ModulesAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (string.IsNullOrEmpty(id.ToString()) || id < 1)
                {
                    /** return error*/
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = _context.tbl_user_module.FirstOrDefault(h => h.module_id == Convert.ToInt32(id));
                    if (smt == null)
                    {
                        /** return error message;*/
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        model = new ModulesViewModel
                        {
                            module_id = Convert.ToInt32(smt.module_id),
                            module_code = smt.module_code,
                            module_name = smt.module_name,
                            module_label = smt.module_label,
                            module_folder = smt.module_folder,
                            module_sort = smt.module_sort,
                            module_status = smt.module_status,
                        };
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        return PartialView("Admin/_ModulesAddEdit", model);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ModulesSave(ModulesViewModel model)
        {
            /*
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("module_id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }

            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("3", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string? module_code = model.module_code;
            string? module_name = model.module_name;
            string? module_label = model.module_label;
            string? module_folder = model.module_folder;
            int module_sort = Convert.ToInt32(model.module_sort);
            string? module_status = model.module_status;

            if (mode == "add")
            {
                var isData = _context.tbl_user_module.FirstOrDefault(u => u.module_code == module_code);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }

                int module_id = (_context.tbl_user_module.Any()
                                ? _context.tbl_user_module.Max(o => o.module_id)
                                : 0) + 1;
                var DataSave = new tbl_user_module
                {
                    module_id = module_id,
                    module_code = module_code,
                    module_name = module_name,
                    module_label = module_label,
                    module_folder = module_folder,
                    module_sort = module_sort,
                    module_status = module_status
                };
                _ = _context.tbl_user_module.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id = module_id });
            }
            else if (mode == "edit")
            {
                int module_id = model.module_id;
                /**check if the data is exits on another record**/
                var isData = _context.tbl_user_module
                        .FirstOrDefault(u => u.module_code == module_code &&
                        u.module_id != module_id
                        );
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }

                var DataUpdate = _context.tbl_user_module.FirstOrDefault(h => h.module_id == module_id);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }

                DataUpdate.module_code = module_code;
                DataUpdate.module_name = module_name;
                DataUpdate.module_label = module_label;
                DataUpdate.module_folder = module_folder;
                DataUpdate.module_sort = module_sort;
                DataUpdate.module_status = module_status;

                _ = _context.tbl_user_module.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.module_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ModulesDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("3", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            /** Validate input */
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            bool recordsExist = await _context.tbl_user_menu.AnyAsync(r => request.SelectedIds.Contains(r.module_id.ToString() ?? "")).ConfigureAwait(false)
            || await _context.tbl_user_level_module.AnyAsync(r => request.SelectedIds.Contains(r.module_id.ToString() ?? "")).ConfigureAwait(false)
            || await _context.tbl_user_user_module.AnyAsync(r => request.SelectedIds.Contains(r.module_id.ToString())).ConfigureAwait(false);
            if (recordsExist)
            {
                /** FK record exists | Canot delete */
                return BadRequest(new { success = false, message = Lang.msg_delete_fail });
            }
            var recordsToDelete = await _context.tbl_user_module.Where(r => request.SelectedIds.Contains(r.module_id.ToString())).ToListAsync().ConfigureAwait(false);
            if (recordsToDelete.Count < 1)
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }
            _context.tbl_user_module.RemoveRange(recordsToDelete);
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);

            return Ok(new
            {
                status = "success",
                deletedCount = recordsToDelete.Count,
                message = Lang.msg_delete_success
            });
        }
        #endregion


        #region MENUS

        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult Menus()
        {
            #region FOR PERMISSION
            string PageId = "2";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from menu in _context.tbl_user_menu
                join module in _context.tbl_user_module
                on menu.module_id equals module.module_id
                select new MenusViewModel
                {
                    // all fields from tbl_user_menu
                    menu_id = menu.menu_id,
                    menu_code = menu.menu_code,
                    menu_name = menu.menu_name,
                    menu_label = menu.menu_label,
                    menu_page = menu.menu_page,
                    menu_sort = Convert.ToInt32(menu.menu_sort),
                    menu_status = menu.menu_status,
                    module_id = Convert.ToInt32(menu.module_id),
                    module_label = module.module_label ?? ""
                }).AsNoTracking().ToList();
            ViewBag.ModuleFilter = _adminServices.ModuleList(0);
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Admin/Menus", "ADD|DEL", PageId, Records.Count);
            return PartialView("Admin/_Menus", Records);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MenusList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string ModuleFilter = request.FilterValue;/*Dropdwon Filter*/
            var query = from menu in _context.tbl_user_menu
                        join module in _context.tbl_user_module
                        on menu.module_id equals module.module_id
                        orderby menu.menu_id descending
                        select new MenusViewModel
                        {
                            menu_id = menu.menu_id,
                            menu_code = menu.menu_code,
                            menu_name = menu.menu_name,
                            menu_label = menu.menu_label,
                            menu_page = menu.menu_page,
                            menu_sort = Convert.ToInt32(menu.menu_sort),
                            menu_status = menu.menu_status,
                            module_id = Convert.ToInt32(module.module_id),
                            module_label = module.module_label ?? ""
                        };
            if (!string.IsNullOrEmpty(ModuleFilter))
            {
                query = query.Where(menu => menu.module_id == Convert.ToInt32(ModuleFilter));/*filter*/
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    a.menu_code.Contains(searchValue) ||
                    (a.menu_name != null && a.menu_name.Contains(searchValue)) ||
                    (a.menu_label != null && a.menu_label.Contains(searchValue)) ||
                    (a.menu_page != null && a.menu_page.Contains(searchValue)) ||
                    (a.module_label != null && a.module_label.Contains(searchValue))
                );
            }


            var data = query.ToList();

            int totalRecord = data.Count;
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };

            return new JsonResult(jsonData);

        }
        public IActionResult MenusAddEdit(string id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "2";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.mode = mode;
            MenusViewModel model;
            ViewBag.MenuStatus = StatusActivePassive("AP");
            ViewBag.ModuleList = new SelectList(
                _context.tbl_user_module.ToList(),  /** source data */
                "module_id",                        /** value field */
                "module_label",                     /** text field */
                "0"                                 /** selected value */
            );

            /** this is to load blank form while doing add process */
            model = new MenusViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("Admin/_MenusAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    /**return error */
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var query = (from menu in _context.tbl_user_menu
                                 join module in _context.tbl_user_module
                                 on menu.module_id equals module.module_id
                                 where menu.menu_id == id
                                 select new MenusViewModel
                                 {
                                     menu_id = menu.menu_id,
                                     menu_code = menu.menu_code,
                                     menu_name = menu.menu_name,
                                     menu_label = menu.menu_label,
                                     menu_page = menu.menu_page,
                                     menu_sort = Convert.ToInt32(menu.menu_sort),
                                     menu_status = menu.menu_status,
                                     module_id = Convert.ToInt32(menu.module_id),
                                     module_label = module.module_label ?? ""
                                 }).FirstOrDefault();
                    if (query == null)
                    {
                        /**return error message;*/
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        return PartialView("Admin/_MenusAddEdit", query);
                    }
                }
            }
            else
            {
                /** return error message */
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult MenusSave(MenusViewModel model)
        {
            /**
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("menu_id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }

            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("2", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            string menu_code = model.menu_code;
            string menu_name = model.menu_name ?? "";
            string menu_label = model.menu_label ?? "";
            string menu_page = model.menu_page ?? "";
            int menu_sort = Convert.ToInt32(model.menu_sort);
            string menu_status = model.menu_status ?? "";
            int module_id = Convert.ToInt32(model.module_id);

            if (string.Equals(mode, "add", StringComparison.OrdinalIgnoreCase))
            {
                var isData = _context.tbl_user_menu.FirstOrDefault(u => u.menu_code == menu_code);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }

                string menu_id = UniqueID();
                var DataSave = new tbl_user_menu
                {
                    menu_id = menu_id,
                    menu_code = menu_code,
                    menu_name = menu_name,
                    menu_label = menu_label,
                    menu_page = menu_page,
                    menu_sort = menu_sort,
                    menu_status = menu_status,
                    module_id = module_id
                };
                _ = _context.tbl_user_menu.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id = menu_id });
            }
            else if (string.Equals(mode, "edit", StringComparison.OrdinalIgnoreCase))
            {
                string menu_id = model.menu_id;
                var isData = _context.tbl_user_menu.FirstOrDefault(u => u.menu_code == menu_code && u.menu_id != menu_id);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                var DataUpdate = _context.tbl_user_menu.FirstOrDefault(h => h.menu_id == menu_id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }

                DataUpdate.menu_code = menu_code;
                DataUpdate.menu_name = menu_name;
                DataUpdate.menu_label = menu_label;
                DataUpdate.menu_page = menu_page;
                DataUpdate.menu_sort = menu_sort;
                DataUpdate.menu_status = menu_status;
                DataUpdate.module_id = module_id;

                _ = _context.tbl_user_menu.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.menu_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MenusDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("2", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            bool recordsExist = await _context.tbl_user_level_menu.AnyAsync(r => request.SelectedIds.Contains(r.menu_id)).ConfigureAwait(false)
            || await _context.tbl_user_user_menu.AnyAsync(r => request.SelectedIds.Contains(r.menu_id ?? "")).ConfigureAwait(false);
            if (recordsExist)
            {
                /** FK record exists | Cannot delete */
                return BadRequest(new { success = false, message = Lang.msg_delete_fail });
            }
            /** matching records    */
            var recordsToDelete = await _context.tbl_user_menu.Where(r => request.SelectedIds.Contains(r.menu_id.ToString())).ToListAsync().ConfigureAwait(false);
            if (recordsToDelete.Count < 1)
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }
            _context.tbl_user_menu.RemoveRange(recordsToDelete);
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);

            return Ok(new
            {
                status = "success",
                deletedCount = recordsToDelete.Count,
                message = Lang.msg_delete_success
            });

        }
        #endregion


        #region EMAIL LOG
        // Renders the standard HTML page layout
        [HttpGet]
        public async Task<IActionResult> EmailLog()
        {
            #region FOR PERMISSION
            string PageId = "7";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_email_list
                orderby a.id descending
                select new EmailLogViewModel
                {
                    id = a.id,
                    from_add = a.from_add ?? "",
                    to_add = a.to_add ?? "",
                    subject = a.subject ?? "",
                    e_message = a.e_message ?? "",
                    submit_date = a.submit_date,
                    status = a.status ?? "",
                    sent_date = a.sent_date,
                    category = a.category ?? "",
                    cc_add = a.cc_add ?? ""
                }).AsNoTracking().ToList();
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Admin/EmailLog", "SET-AS-SENT|DEL", PageId, Records.Count);
            return PartialView("Admin/_EmailLog", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailLogList()
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var query = from a in _context.tbl_email_list
                        orderby a.id descending
                        select new EmailLogViewModel
                        {
                            id = a.id,
                            from_add = a.from_add ?? "",
                            to_add = a.to_add ?? "",
                            subject = a.subject ?? "",
                            e_message = a.e_message ?? "",
                            submit_date = a.submit_date,
                            status = a.status ?? "",
                            sent_date = a.sent_date,
                            category = a.category ?? "",
                            cc_add = a.cc_add ?? ""
                        };
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    a.to_add.Contains(searchValue) ||
                    a.subject.Contains(searchValue) ||
                    a.e_message.Contains(searchValue) ||
                    a.status.Contains(searchValue) ||
                    a.category.Contains(searchValue) ||
                    a.cc_add.Contains(searchValue)
                );
            }
            var data = query.ToList();

            int totalRecord = data.Count;
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };

            return new JsonResult(jsonData);
        }
        public IActionResult EmailLogAddEdit(string id, string mode)
        {
            //This is for Modal View Page
            #region FOR PERMISSION
            string PageId = "7";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.mode = mode;
            EmailLogViewModel model;

            //this is to load blank form while doing add process
            model = new EmailLogViewModel();
            if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    //return error
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var query = (
                    from a in _context.tbl_email_list
                    orderby a.id descending
                    select new EmailLogViewModel
                    {
                        id = a.id,
                        from_add = a.from_add ?? "",
                        to_add = a.to_add ?? "",
                        subject = a.subject ?? "",
                        e_message = a.e_message ?? "",
                        submit_date = a.submit_date,
                        status = a.status ?? "",
                        sent_date = a.sent_date,
                        category = a.category ?? "",
                        cc_add = a.cc_add ?? ""
                    })
                    .FirstOrDefault();

                    if (query == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        return PartialView("Admin/_EmailLogAddEdit", query);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailLogUpdateStatus([FromBody] bulkStatusUpdateRequest request)
        {
            if (!_accountServices.HasPermission("7", "edit")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            // Validate input
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }

            string mode = request.mode;
            string hStatus = request.hStatus;

            if (mode == "updateStatus" && !string.IsNullOrWhiteSpace(hStatus))
            {
                /** Bulk update all selected IDs */
                int updatedCount = _context.tbl_email_list
                    .Where(r => request.SelectedIds.Contains(r.id))
                    .ExecuteUpdate(setters => setters
                        .SetProperty(r => r.status, "Y")
                    );

                if (updatedCount == 0)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }

                return Ok(new
                {
                    status = "success",
                    updatedCount,
                    message = Lang.msg_update_success
                });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailLogDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("7", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            var recordsToDeleteAtt = await _context.tbl_email_list_attachment
                .Where(r => request.SelectedIds.Contains(r.eid.ToString()))
                .ToListAsync().ConfigureAwait(false);
            if (recordsToDeleteAtt.Count > 0)
            {
                _context.tbl_email_list_attachment.RemoveRange(recordsToDeleteAtt);
                _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                _context.ChangeTracker.Clear();
            }
            /** Delete Error Log */
            var recordsToDeleteErr = await _context.tbl_email_list_sub
                .Where(r => request.SelectedIds.Contains(r.eid.ToString()))
                .ToListAsync().ConfigureAwait(false);
            if (recordsToDeleteErr.Count > 0)
            {
                _context.tbl_email_list_sub.RemoveRange(recordsToDeleteErr);
                _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                _context.ChangeTracker.Clear();
            }

            /** Delete Email**/
            var recordsToDelete = await _context.tbl_email_list
                .Where(r => request.SelectedIds.Contains(r.id.ToString()))
                .ToListAsync().ConfigureAwait(false);
            if (recordsToDelete.Count > 0)
            {
                _context.tbl_email_list.RemoveRange(recordsToDelete);
                _ = await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            return Ok(new
            {
                status = "success",
                deletedCount = recordsToDelete.Count,
                message = Lang.msg_delete_success
            });
        }
        #endregion


        #region EMAIL TESTING
        [HttpGet]
        public async Task<IActionResult> EmailTesting()
        {
            #region FOR PERMISSION
            string PageId = "8";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var model = new EmailTestingViewModel();
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Admin/ApplicationSettings", "SEND-TEST-MAIL|", PageId, 0);
            return PartialView("Views/Shared/Admin/_EmailTesting.cshtml", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmailTestingSendEmail(EmailTestingViewModel model)
        {
            if (!_accountServices.HasPermission("8", "add")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string ToEmail = "";
            string CcEmail = "";
            string Subject = "";
            string Messages = "";

            if (!string.IsNullOrWhiteSpace(model.to_email)) { ToEmail = model.to_email; }
            if (!string.IsNullOrWhiteSpace(model.cc_email)) { CcEmail = model.cc_email; }
            if (!string.IsNullOrWhiteSpace(model.subject)) { Subject = model.subject; }
            if (!string.IsNullOrWhiteSpace(model.messages)) { Messages = model.messages; }

            if (string.IsNullOrWhiteSpace(ToEmail) || string.IsNullOrWhiteSpace(Subject) || string.IsNullOrWhiteSpace(Messages))
            {
                return Json(new { status = "invalid", message = Lang.msg_insufficient_info });
            }
            else
            {
                string emst = _emailService.SendEmail("EmailTest", ToEmail, Subject, Messages, "", CcEmail);
                if (emst == "true")
                {
                    return Json(new { status = "success", message = Lang.msg_email_sent_success });
                }
                else
                {
                    return Json(new { status = "error", message = emst });
                }
            }
        }
        #endregion


        #region USER LOGIN HISTORY
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult UserLoginHistory()
        {
            #region FOR PERMISSION
            string PageId = "9";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.que_user_log
                orderby a.ID descending
                select new UserLoginHistoryViewModel
                {
                    ID = a.ID,
                    fullname = a.fullname,
                    username = a.username,
                    level_name = a.level_name,
                    in_date = a.in_date,
                    out_date = a.out_date,
                    ip = a.ip,
                }).AsNoTracking().ToList();
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Admin/UserLoginHistory", "|SN-DEL", PageId, Records.Count);
            return PartialView("Admin/_UserLoginHistory", Records);
        }
        //List search | order |        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLoginHistoryList()
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var query = _context.que_user_log
                .OrderByDescending(a => a.ID)
                .Select(a => new UserLoginHistoryViewModel
                {
                    ID = a.ID,
                    fullname = a.fullname,
                    username = a.username,
                    level_name = a.level_name,
                    in_date = a.in_date,
                    out_date = a.out_date,
                    ip = a.ip
                });

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    (a.fullname != null && a.fullname.Contains(searchValue)) ||
                    (a.username != null && a.username.Contains(searchValue)) ||
                    (a.level_name != null && a.level_name.Contains(searchValue))
                );
            }
            var data = query.ToList();
            int totalRecord = data.Count;
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };

            return new JsonResult(jsonData);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLoginHistoryDelete([FromBody] modeRequest request)
        {
            if (!string.Equals(_accountServices.GetSingleMenuPermission("9", "D"), "true", StringComparison.OrdinalIgnoreCase)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
            /** if delete clicked it is to delete data before 6 months */
            if (string.Equals(request.mode, "updateDataNoChk", StringComparison.OrdinalIgnoreCase))
            {
                /** Calculate cutoff date (1 year ago from now)*/
                var cutoffDate = DateTime.Now.AddYears(-1);

                /** records older than 6 months */
                var recordsToDelete = await _context.tbl_user_login_log
                    .Where(r => r.in_date < cutoffDate).ToListAsync().ConfigureAwait(false);

                if (recordsToDelete.Count > 0)
                {
                    return Json(new { status = "error", message = Lang.msg_no_record_found });
                }
                _context.tbl_user_login_log.RemoveRange(recordsToDelete);
                _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                return Ok(new
                {
                    status = "success",
                    deletedCount = recordsToDelete.Count,
                    message = string.Concat("[", recordsToDelete.Count, "]", Lang.msg_delete_success)
                });
            }
            else
            {
                return Json(new { status = "error", message = Lang.msg_delete_fail });
            }
        }
        public IActionResult UserLoginHistoryAddEdit(string? id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "9";
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            ViewBag.dpern = perm.dpern;
            #endregion FOR END PERMISSION

            ViewBag.mode = mode;
            UserLoginHistoryViewModel model;
            /** this is to load blank form while doing add process */
            model = new UserLoginHistoryViewModel();
            if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = _context.que_user_log.FirstOrDefault(h => h.ID == id);
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        model = new UserLoginHistoryViewModel
                        {
                            ID = smt.ID,
                            fullname = smt.fullname,
                            username = smt.username,
                            level_name = smt.level_name,
                            in_date = smt.in_date,
                            out_date = smt.out_date,
                            ip = smt.ip,
                            user_agent = smt.user_agent,
                        };
                        return PartialView("Admin/_UserLoginHistoryAddEdit", model);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        #endregion


        #region USER LOGIN FAIL
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult UserLoginFail()
        {
            #region FOR PERMISSION
            string PageId = "10";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_user_login_fail
                orderby a.Id descending
                select new UserLoginFailViewModel
                {
                    Id = a.Id,
                    username = a.username,
                    on_date = a.on_date,
                    ip = a.ip,
                    user_agent = a.user_agent,
                }).AsNoTracking().ToList();
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Admin/UserLoginFail", "|SN-DEL", PageId, Records.Count);
            return PartialView("Admin/_UserLoginFail", Records);
        }

        //List search | order |        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLoginFailList()
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var query = _context.tbl_user_login_fail.OrderByDescending(a => a.Id)
                .Select(a => new UserLoginFailViewModel
                {
                    Id = a.Id,
                    username = a.username,
                    on_date = a.on_date,
                    ip = a.ip,
                    user_agent = a.user_agent,
                });

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a => a.username.Contains(searchValue));
            }

            var data = query.ToList();
            int totalRecord = data.Count;
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = data.Skip(skip).Take(pageSize).ToList();

            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };

            return new JsonResult(jsonData);
        }

        public IActionResult UserLoginFailAddEdit(string? id, string mode)
        {
            #region FOR PERMISSION
            string PageId = "10";
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.mode = mode;
            UserLoginFailViewModel model;
            //this is to load blank form while doing add process
            model = new UserLoginFailViewModel();
            if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = _context.tbl_user_login_fail.FirstOrDefault(h => h.Id == id);
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        model = new UserLoginFailViewModel
                        {
                            Id = smt.Id,
                            username = smt.username,
                            on_date = smt.on_date,
                            ip = smt.ip,
                            user_agent = smt.user_agent,
                        };
                        return PartialView("Admin/_UserLoginFailAddEdit", model);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        #endregion

    }
}
