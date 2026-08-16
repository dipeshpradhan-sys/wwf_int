using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Dynamic.Core;
using wwfpp.Data;
using wwfpp.Models;
using wwfpp.Models.Users;
using wwfpp.Services;
using static GblUtilities;

namespace wwfpp.Controllers
{
    public class UsersController(
        AppDbContext context,
        EmployeeServices employeeServices,
        AccountServices accountServices,
        UserServices userServices,
        UserRightsServices userRightsServices
        ) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly EmployeeServices _employeeServices = employeeServices;
        private readonly AccountServices _accountServices = accountServices;
        private readonly UserServices _userServices = userServices;
        private readonly UserRightsServices _userRightsServices = userRightsServices;

        #region USERS
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult Users()
        {
            string PageId = "10403";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V");
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = _context.tbl_user
                .Where(u => u.user_id > 1)
                .Join(_context.tbl_user_level,
                      usr => usr.level_id,
                      lvl => lvl.level_id,
                      (usr, lvl) => new { usr, lvl })
                .Join(_context.tbl_employee,
                      ul => ul.usr.emp_id,
                      emp => emp.emp_id,
                      (ul, emp) => new UsersViewModel
                      {
                          user_id = ul.usr.user_id,
                          username = ul.usr.username,
                          level_id = ul.usr.level_id,
                          level_name = ul.lvl.level_name,
                          emp_id = ul.usr.emp_id ?? 0,
                          emp_code = emp.emp_code,
                          firstname = emp.firstname,
                          middlename = emp.middlename,
                          lastname = emp.lastname,
                          is_active = ul.usr.is_active,
                          sign_in_type = ul.usr.sign_in_type
                      })
                .OrderByDescending(x => x.user_id)
                .AsNoTracking().ToList();
            ViewBag.StatusFilter = StatusActivePassive("YNAD", "Y");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Users/Users", "ADD|ACT-DACT|DEL", PageId, Records.Count);
            return PartialView("Users/_Users", Records);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UsersList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            /*****
            * Correct Pattern
            1. Query only raw fields(simple joins, filters, ordering) ? EF Core can translate this.
            2. Materialize with ToListAsync().
            3. Build your UsersViewModel and do string concatenation in memory.
            4. Apply search filtering on the in-memory projection.
            ********/
            string StatusFilter = request.FilterValue;/*Dropdwon Filter*/
            var query = from usr in _context.tbl_user
                        join lvl in _context.tbl_user_level on usr.level_id equals lvl.level_id
                        join emp in _context.tbl_employee on usr.emp_id equals emp.emp_id
                        where usr.user_id > 1
                        orderby usr.user_id descending
                        select new
                        {
                            usr.user_id,
                            usr.username,
                            usr.level_id,
                            lvl.level_name,
                            usr.emp_id,
                            emp.emp_code,
                            emp.firstname,
                            emp.middlename,
                            emp.lastname,
                            usr.is_active,
                            usr.sign_in_type,
                        };
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(d => d.is_active == StatusFilter);/*filter*/
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(u =>
                u.username.Contains(searchValue) ||
                u.level_name.Contains(searchValue) ||
                u.firstname.Contains(searchValue) ||
                u.middlename.Contains(searchValue) ||
                u.lastname.Contains(searchValue) ||
                u.emp_code.Contains(searchValue));
            }
            var rawData = await query.ToListAsync().ConfigureAwait(false);/** Materialize first (EF translates this fine)*/
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                /** Requires System.Linq.Dynamic.Core **/
                query = sortColumn switch
                {
                    "emp_name_code" => sortColumnDir == "asc"
                    ? query.OrderBy(u => u.firstname).ThenBy(u => u.middlename).ThenBy(u => u.lastname)
                    : query.OrderByDescending(u => u.firstname).ThenByDescending(u => u.middlename).ThenByDescending(u => u.lastname),
                    _ => query.OrderBy($"{sortColumn} {sortColumnDir}"),
                };
            }
            else
            {
                query = query.OrderByDescending(u => u.user_id);
            }
            int totalRecord = query.Count();
            if (pageSize == -1) { pageSize = totalRecord; }
            var cData = query.Skip(skip).Take(pageSize).ToList();

            var data = rawData.Select(u => new UsersViewModel
            {
                user_id = u.user_id,
                username = u.username,
                level_id = u.level_id,
                level_name = u.level_name,
                emp_id = u.emp_id,
                emp_code = u.emp_code,
                firstname = u.firstname,
                middlename = u.middlename,
                lastname = u.lastname,
                is_active = u.is_active,
                sign_in_type = u.sign_in_type
            }).ToList();
            var jsonData = new
            {
                draw,
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                data = cData
            };
            return new JsonResult(jsonData);
        }
        public IActionResult UsersAddEdit(int id, string mode)
        {
            string PageId = "10403";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V");
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            /** Load employee dropdown */
            ViewBag.EmployeeList = _userServices.GetEmployeeNotHavingUsername();
            ViewBag.Status = StatusActivePassive("YN");
            var UserLevel = _context.tbl_user_level.Where(c => c.level_type > 2).OrderBy(c => c.level_name).ToList();
            ViewBag.UserLevel = new SelectList(UserLevel, "level_id", "level_name");
            ViewBag.EmployeeName = "";
            ViewBag.mode = mode;
            UsersViewModel model;
            /** this is to load blank form while doing add process **/
            model = new UsersViewModel();

            if (mode == "add")
            {
                ViewBag.UserRightsHtml = _userRightsServices.GetUserRights("0", "user_id");
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A");
                return PartialView("Users/_UsersAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id == 0)
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = _context.tbl_user.FirstOrDefault(h => h.user_id == Convert.ToInt32(id));
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        model = new UsersViewModel
                        {
                            user_id = smt.user_id,
                            username = smt.username,
                            level_id = smt.level_id,
                            emp_id = smt.emp_id,
                            is_active = smt.is_active,
                            sign_in_type = smt.sign_in_type
                        };
                        ViewBag.EmployeeName = _employeeServices.GetEmployeeName(Convert.ToInt32(smt.emp_id));
                        ViewBag.UserRightsHtml = _userRightsServices.GetUserRights(smt.user_id.ToString(), "user_id");
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E");
                        return PartialView("Users/_UsersAddEdit", model);
                    }
                }
            }
            else
            {
                return BadRequest(new { success = false, message = Lang.msg_error });
            }
        }
        [HttpGet]
        public IActionResult UsersRights(string level_id)
        {
            string outputRights = _userRightsServices.GetUserRights(level_id, "level_id");
            return Json(outputRights);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult UsersSave(UsersViewModel model)
        {
            /*
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("user_id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10403", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string? level_id = model.level_id;
            string? status = model.is_active;
            int user_id;
            string hArrModule = Request.Form["h_arr_module"].ToString();
            string[] arrModule = hArrModule.Split(',');

            if (mode == "add")
            {
                string? username = model.username;
                int emp_id = Convert.ToInt32(model.emp_id);
                //check if the data is exits on another record
                var isData = _context.tbl_user.FirstOrDefault(u => u.username == username);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                //get maximum id
                user_id = (_context.tbl_user.Any() ? _context.tbl_user.Max(o => o.user_id) : 0) + 1;
                string npwd = GetRndUniqueText(10, "");
                string hashpwd = AccountServices.MakeHash(npwd);
                int sign_in_type = 0;
                string activation_key = GetRndUniqueText(16, "");

                var DataSave = new tbl_user
                {
                    user_id = user_id,
                    username = username,
                    pwd = hashpwd,
                    level_id = level_id,
                    emp_id = emp_id,
                    is_active = status,
                    sign_in_type = sign_in_type,
                    activation_key = activation_key,
                    submit_date = DateTime.Now
                };
                _ = _context.tbl_user.Add(DataSave);
                _ = _context.SaveChanges();
                _context.ChangeTracker.Clear();

                var DataSavePH = new tbl_user_pwd_history
                {
                    Id = UniqueID(),
                    user_id = user_id,
                    pwd = hashpwd,
                    updated_date = DateTime.Now,
                    is_current_one = "Y"
                };
                _ = _context.tbl_user_pwd_history.Add(DataSavePH);
                _ = _context.SaveChanges();
                _context.ChangeTracker.Clear();

            }
            else if (mode == "edit")
            {
                user_id = Convert.ToInt32(model.user_id);
                var modulesToDelete = _context.tbl_user_user_module.Where(m => m.user_id == user_id); /** Delete all modules for this level */
                _context.tbl_user_user_module.RemoveRange(modulesToDelete);
                var menusToDelete = _context.tbl_user_user_menu.Where(m => m.user_id == user_id);/** Delete all menus for this level */
                _context.tbl_user_user_menu.RemoveRange(menusToDelete);
                _ = _context.SaveChanges();
                _context.ChangeTracker.Clear();

                var DataUpdate = _context.tbl_user.FirstOrDefault(h => h.user_id == user_id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }

                DataUpdate.is_active = status;
                DataUpdate.level_id = level_id;
                _ = _context.tbl_user.Update(DataUpdate);
                _ = _context.SaveChanges();
                _context.ChangeTracker.Clear();
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            /** Save modules + menus
            * Convert Request.Form into a dictionary
            */
            var formValues = Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString());
            _userRightsServices.SaveModulesAndMenus(formValues, user_id.ToString(), arrModule, "User");

            return Json(new
            {
                status = "success",
                message = mode == "add" ? Lang.msg_added_success : Lang.msg_update_success,
                id = user_id
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UsersDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10403", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            // Validate input
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            bool recordsExist = await _context.tbl_user_pwd_history.AnyAsync(r => request.SelectedIds.Contains(r.user_id.ToString())).ConfigureAwait(false);
            if (recordsExist)
            {
                return BadRequest(new { status = "false", message = Lang.msg_delete_fail });/** FK record exists | Canot delete **/
            }
            /** matching records
             * delete from tbl_user_level_menu
             */
            var delLevelMenu = await _context.tbl_user_user_menu.Where(r => request.SelectedIds.Contains(r.user_id.ToString())).ToListAsync().ConfigureAwait(false);
            if (delLevelMenu.Count > 0)
            {
                _context.tbl_user_user_menu.RemoveRange(delLevelMenu);
                _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                _context.ChangeTracker.Clear();
            }
            /** delete from tbl_user_level_module */
            var delLevelModule = await _context.tbl_user_user_module.Where(r => request.SelectedIds.Contains(r.user_id.ToString())).ToListAsync().ConfigureAwait(false);
            if (delLevelModule.Count > 0)
            {
                _context.tbl_user_user_module.RemoveRange(delLevelModule);
                _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                _context.ChangeTracker.Clear();
            }
            /** delete from tbl_user_level */
            var recordsToDelete = await _context.tbl_user.Where(r => request.SelectedIds.Contains(r.user_id.ToString())).ToListAsync().ConfigureAwait(false);
            if (recordsToDelete.Count < 1)
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }
            _context.tbl_user.RemoveRange(recordsToDelete);
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);

            return Ok(new
            {
                status = "success",
                deletedCount = recordsToDelete.Count,
                message = Lang.msg_delete_success
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UsersUpdateStatus([FromBody] bulkStatusUpdateRequest request)
        {
            // Validate input
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            string mode = request.mode;
            string hStatus = request.hStatus;
            hStatus = hStatus == "Activate" ? "Y" : "N";

            if (string.Equals(mode, "updateStatus", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(hStatus))
            {
                /** Bulk update all selected IDs*/
                int updatedCount = _context.tbl_user
                    .Where(r => request.SelectedIds.Contains(r.user_id.ToString()))
                    .ExecuteUpdate(setters => setters
                        .SetProperty(r => r.is_active, hStatus)
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
        #endregion

        #region USER LEVEL
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult UserLevel()
        {
            string PageId = "10402";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V");
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_user_level
                where a.level_type > 2
                orderby a.level_id descending
                select new UserLevelViewModel
                {
                    level_id = a.level_id,
                    level_name = a.level_name,
                    level_type = Convert.ToInt32(a.level_type),
                    level_sort = Convert.ToInt32(a.level_sort)
                }).AsNoTracking().ToList();
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Users/UserLevel", "ADD|DEL", PageId, Records.Count);
            return PartialView("Users/_UserLevel", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLevelList()
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var query = _context.tbl_user_level.Where(a => a.level_type > 2)
                .OrderByDescending(a => a.level_id)
                .Select(a => new UserLevelViewModel
                {
                    level_id = a.level_id,
                    level_name = a.level_name
                });
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(string.Concat(sortColumn, " ", sortColumnDir));
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a => a.level_name != null && a.level_name.Contains(searchValue));
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
        public IActionResult UserLevelAddEdit(string id, string mode)
        {
            string PageId = "10402";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V");
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.mode = mode;
            UserLevelViewModel model;
            //this is to load blank form while doing add process
            model = new UserLevelViewModel();
            if (mode == "add")
            {
                ViewBag.UserRightsHtml = _userRightsServices.GetUserRights("0", "level_id");
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A");
                return PartialView("Users/_UserLevelAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = _context.tbl_user_level.FirstOrDefault(h => h.level_id == id);
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        model = new UserLevelViewModel
                        {
                            level_id = smt.level_id,
                            level_name = smt.level_name,
                            level_type = smt.level_type,
                            level_sort = smt.level_sort
                        };
                        ViewBag.UserRightsHtml = _userRightsServices.GetUserRights(smt.level_id.ToString(), "level_id");
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E");
                        return PartialView("Users/_UserLevelAddEdit", model);
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
        public JsonResult UserLevelSave(UserLevelViewModel model)
        {
            /**
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("level_id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10402", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string? level_name = model.level_name;
            int level_type = 3;
            int level_sort = 0;

            string hArrModule = Request.Form["h_arr_module"].ToString();
            string[] arrModule = hArrModule.Split(',');
            string level_id;

            if (mode == "add")
            {
                /**check if the data is exits on another record**/
                var isData = _context.tbl_user_level.FirstOrDefault(u => u.level_name == level_name);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                level_id = UniqueID();
                var DataSave = new tbl_user_level
                {
                    level_id = level_id,
                    level_name = level_name,
                    level_type = level_type,
                    level_sort = level_sort
                };
                _ = _context.tbl_user_level.Add(DataSave);
            }
            else if (mode == "edit")
            {
                level_id = model.level_id;
                /** check if the data is exits on another record */
                var isData = _context.tbl_user_level.FirstOrDefault(u => u.level_name == level_name && u.level_id != level_id);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                /** Delete all modules for this level */
                var modulesToDelete = _context.tbl_user_level_module.Where(m => m.level_id == level_id);
                _context.tbl_user_level_module.RemoveRange(modulesToDelete);
                /** Delete all menus for this level*/
                var menusToDelete = _context.tbl_user_level_menu.Where(m => m.level_id == level_id);
                _context.tbl_user_level_menu.RemoveRange(menusToDelete);
                /** Commit changes */
                _ = _context.SaveChanges();
                _context.ChangeTracker.Clear();
                var DataUpdate = _context.tbl_user_level.FirstOrDefault(h => h.level_id == level_id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }
                DataUpdate.level_name = level_name;
                DataUpdate.level_type = level_type;
                DataUpdate.level_sort = level_sort;
                _ = _context.tbl_user_level.Update(DataUpdate);
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            /** Save level first */
            _ = _context.SaveChanges();
            _context.ChangeTracker.Clear();
            /** Save modules + menus
             * Convert Request.Form into a dictionary
             */
            var formValues = Request.Form.ToDictionary(k => k.Key, v => v.Value.ToString());
            _userRightsServices.SaveModulesAndMenus(formValues, level_id, arrModule, "Level");

            return Json(new
            {
                status = "success",
                message = mode == "add" ? Lang.msg_added_success : Lang.msg_update_success,
                id = level_id
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserLevelDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10402", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            /** Validate input **/
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            bool recordsExist = await _context.tbl_user.AnyAsync(r => request.SelectedIds.Contains(r.level_id ?? "")).ConfigureAwait(false);
            if (recordsExist)
            {
                return BadRequest(new { success = false, message = Lang.msg_delete_fail });/**FK record exists | Canot delete*/
            }
            /** matching records
             * delete from tbl_user_level_menu
             */
            var delLevelMenu = await _context.tbl_user_level_menu.Where(r => request.SelectedIds.Contains(r.level_id)).ToListAsync().ConfigureAwait(false);
            if (delLevelMenu.Count > 0)
            {
                _context.tbl_user_level_menu.RemoveRange(delLevelMenu);
                _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                _context.ChangeTracker.Clear();
            }
            /** delete from tbl_user_level_module */
            var delLevelModule = await _context.tbl_user_level_module.Where(r => request.SelectedIds.Contains(r.level_id ?? "")).ToListAsync().ConfigureAwait(false);
            if (delLevelModule.Count > 0)
            {
                _context.tbl_user_level_module.RemoveRange(delLevelModule);
                _ = await _context.SaveChangesAsync().ConfigureAwait(false);
                _context.ChangeTracker.Clear();
            }
            /**delete from tbl_user_level*/
            var recordsToDelete = await _context.tbl_user_level.Where(r => request.SelectedIds.Contains(r.level_id)).ToListAsync().ConfigureAwait(false);
            if (recordsToDelete.Count < 1)
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }
            _context.tbl_user_level.RemoveRange(recordsToDelete);
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);

            return Ok(new
            {
                status = "success",
                deletedCount = recordsToDelete.Count,
                message = Lang.msg_delete_success
            });
        }
        #endregion

        #region USER GUARD
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult Guard()
        {
            string PageId = "10451";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V");
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_user_guard
                orderby a.pk_user_id ascending
                select new GuardViewModel
                {
                    pk_user_id = a.pk_user_id,
                    user_name = a.user_name,
                    user_pass = a.user_pass,
                    full_name = a.full_name,
                    is_active = a.is_active,
                    user_type = a.user_type
                }).AsNoTracking().ToList();
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("Users/Guard", "ADD|DEL", PageId, Records.Count);
            return PartialView("Users/_Guard", Records);

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardList()
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var query = _context.tbl_user_guard
                .OrderByDescending(a => a.pk_user_id)
                .Select(a => new GuardViewModel
                {
                    pk_user_id = a.pk_user_id,
                    user_name = a.user_name,
                    user_pass = a.user_pass,
                    full_name = a.full_name,
                    is_active = a.is_active,
                    user_type = a.user_type
                });
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(string.Concat(sortColumn, " ", sortColumnDir));
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    (a.user_name != null && a.user_name.Contains(searchValue)) ||
                    (a.full_name != null && a.full_name.Contains(searchValue)) ||
                    (a.is_active != null && a.is_active.Contains(searchValue)) ||
                    (a.user_type != null && a.user_type.Contains(searchValue))
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
        public IActionResult GuardAddEdit(int? id, string mode)
        {
            string PageId = "10451";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V");
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.is_active = StatusActivePassive("YN");
            ViewBag.user_type = _accountServices.GuardUserType();
            ViewBag.mode = mode;
            GuardViewModel model;
            /**this is to load blank form while doing add process**/
            model = new GuardViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A");
                return PartialView("Users/_GuardAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id < 1 || string.IsNullOrWhiteSpace(id.ToString()))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = _context.tbl_user_guard.FirstOrDefault(h => h.pk_user_id == Convert.ToInt32(id));
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        model = new GuardViewModel
                        {
                            pk_user_id = smt.pk_user_id,
                            user_name = smt.user_name,
                            user_pass = Decode(smt.user_pass ?? ""),
                            full_name = smt.full_name,
                            is_active = smt.is_active,
                            user_type = smt.user_type,
                        };
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E");
                        return PartialView("Users/_GuardAddEdit", model);
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
        public JsonResult GuardSave(GuardViewModel model)
        {
            /**
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("pk_user_id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10451", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string user_name = model.user_name ?? "";
            string user_pass = Encode(model.user_pass ?? "");
            string full_name = model.full_name ?? "";
            string is_active = model.is_active ?? "";
            string user_type = model.user_type ?? "";

            if (mode == "add")
            {
                /**check if the data is exits on another record*/
                var isData = _context.tbl_user_guard.FirstOrDefault(u => u.user_name == user_name);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }

                //get maximum id
                int pk_user_id = (_context.tbl_user_guard.Any() ? _context.tbl_user_guard.Max(o => o.pk_user_id) : 0) + 1;
                var DataSave = new tbl_user_guard
                {
                    pk_user_id = pk_user_id,
                    user_name = user_name,
                    user_pass = user_pass,
                    full_name = full_name,
                    is_active = is_active,
                    user_type = user_type,
                };
                _ = _context.tbl_user_guard.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id = pk_user_id });
            }
            else if (mode == "edit")
            {
                int pk_user_id = model.pk_user_id;
                /** check if the data is exits on another record */
                var isData = _context.tbl_user_guard.FirstOrDefault(u => u.user_name == user_name && u.pk_user_id != pk_user_id);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                var DataUpdate = _context.tbl_user_guard.FirstOrDefault(h => h.pk_user_id == pk_user_id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }
                DataUpdate.user_name = user_name;
                DataUpdate.user_pass = user_pass;
                DataUpdate.full_name = full_name;
                DataUpdate.is_active = is_active;
                DataUpdate.user_type = user_type;
                _ = _context.tbl_user_guard.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.pk_user_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10451", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            // Validate input
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            bool recordsExist =
               await _context.tbl_employee_check_in_out_sub.AnyAsync(r => request.SelectedIds.Contains(r.in_guard_user_id.ToString() ?? "")).ConfigureAwait(false)
            || await _context.tbl_employee_check_in_out_sub.AnyAsync(r => request.SelectedIds.Contains(r.out_guard_user_id.ToString() ?? "")).ConfigureAwait(false)
            || await _context.tbl_employee_check_in_out_sub_outside.AnyAsync(r => request.SelectedIds.Contains(r.in_guard_user_id.ToString() ?? "")).ConfigureAwait(false)
            || await _context.tbl_employee_check_in_out_sub_outside.AnyAsync(r => request.SelectedIds.Contains(r.out_guard_user_id.ToString() ?? "")).ConfigureAwait(false);
            if (recordsExist)
            {
                return BadRequest(new { success = false, message = Lang.msg_delete_fail });
            }
            var recordsToDelete = await _context.tbl_user_guard.Where(r => request.SelectedIds.Contains(r.pk_user_id.ToString())).ToListAsync().ConfigureAwait(false);
            if (recordsToDelete.Count < 1)
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }
            _context.tbl_user_guard.RemoveRange(recordsToDelete);
            _ = await _context.SaveChangesAsync().ConfigureAwait(false);

            return Ok(new
            {
                status = "success",
                deletedCount = recordsToDelete.Count,
                message = Lang.msg_delete_success
            });

        }
        #endregion

    }
}
