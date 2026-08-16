using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq.Dynamic.Core;
using System.Text;
using wwfpp.Data;
using wwfpp.Helpers;
using wwfpp.Models;
using wwfpp.Models.General;
using wwfpp.Services;
using static GblUtilities;
namespace wwfpp.Controllers
{
    public class GeneralController(
        AppDbContext context,
        IOptions<AppSettings> appSettings,
        SettingsServices settingsServices,
        AccountServices accountServices,
        GlobalOptionServices globalOptionServices,
        IWebHostEnvironment webHostEnvironment
        ) : Controller
    {
        private readonly AppDbContext _context = context;
        private readonly AppSettings _appSettings = (appSettings ?? throw new ArgumentNullException(nameof(appSettings))).Value;
        private readonly SettingsServices _settingsServices = settingsServices;
        private readonly AccountServices _accountServices = accountServices;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly GlobalOptionServices _globalOptionServices = globalOptionServices;

        #region DOCUMENT TEMPLATES
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult DocumentTemplates()
        {
            string PageId = "10004";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_document_templates
                where a.status == "A"
                orderby a.id descending
                select new DocumentTemplatesViewModel
                {
                    id = a.id,
                    document_title = a.document_title,
                    document_version = a.document_version,
                    document_desc = a.document_desc,
                    upload_file = a.upload_file,
                    upload_date = Convert.ToDateTime(a.upload_date),
                    status = a.status
                }).ToList();
            ViewBag.StatusFilter = StatusActivePassive("AD", "A");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("General/DocumentTemplates", "ADD|ACI-DACT|DEL", PageId, Records.Count);
            return PartialView("General/_DocumentTemplates", Records);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DocumentTemplatesList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string StatusFilter = request.FilterValue;/*Dropdwon Filter*/
            var query = _context.tbl_document_templates.OrderByDescending(a => a.id)
                .Select(a => new DocumentTemplatesViewModel
                {
                    id = a.id,
                    document_title = a.document_title,
                    document_version = a.document_version,
                    document_desc = a.document_desc,
                    upload_file = a.upload_file,
                    upload_date = a.upload_date,
                    status = a.status
                });

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(d => d.status == StatusFilter);/*filter*/
            }

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    (a.document_title != null && a.document_title.Contains(searchValue)) ||
                    (a.document_version != null && a.document_version.Contains(searchValue))
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
        [HttpGet]
        [Route("[controller]/[action]/{id}")]
        public async Task<IActionResult> DocumentTemplatesDownload(string id)
        {
            string PageId = "10004";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return NotFound(); }
            #endregion FOR END PERMISSION

            var smt = _context.tbl_document_templates.FirstOrDefault(h => h.id == id);
            if (smt == null)
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(smt.upload_file))
            {
                return NotFound();
            }
            else
            {
                string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
                string uploadsFolder = Path.Combine(GblDocumentPath, "downloads");

                string filePath = Path.Combine(uploadsFolder, smt.upload_file);
                string fullPathResolved = Path.GetFullPath(filePath);
                string baseDirectoryResolved = Path.GetFullPath(uploadsFolder + Path.DirectorySeparatorChar);
                if (fullPathResolved.StartsWith(baseDirectoryResolved, StringComparison.OrdinalIgnoreCase))
                {
                    if (System.IO.File.Exists(fullPathResolved))
                    {
                        // Use provider to get MIME type from extension
                        var provider = new FileExtensionContentTypeProvider();
                        if (!provider.TryGetContentType(fullPathResolved, out var contentType))
                        {
                            contentType = "application/octet-stream"; // fallback
                        }
                        var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPathResolved);
                        return File(fileBytes, contentType, smt.upload_file);
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }
            }
        }

        // Renders the standard HTML page layout
        public IActionResult DocumentTemplatesAddEdit(string id, string mode)
        {
            string PageId = "10004";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.Status = StatusActivePassive("AD");
            ViewBag.mode = mode;
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            DocumentTemplatesViewModel model;
            //this is to load blank form while doing add process
            model = new DocumentTemplatesViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("General/_DocumentTemplatesAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = _context.tbl_document_templates.FirstOrDefault(h => h.id == id);
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        model = new DocumentTemplatesViewModel
                        {
                            id = smt.id,
                            document_title = smt.document_title,
                            document_version = smt.document_version,
                            document_desc = smt.document_desc,
                            upload_file = smt.upload_file,
                            upload_date = smt.upload_date,
                            status = smt.status
                        };
                        string extension = Path.GetExtension(smt.upload_file ?? "").TrimStart('.').ToUpperInvariant();

                        ViewBag.download = $@"
                            <a href='{Url.Content($"~/General/DocumentTemplatesDownload?id={id}")}'>
                                <img src='{Url.Content($"~/images/{extension}.png")}' title='Download' width='30' height='40' border='0'>
                            </a>";
                        string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
                        string uploadsFolder = Path.Combine(GblDocumentPath, "downloads", smt.upload_file);
                        ViewBag.fileSize = GetFileSize(uploadsFolder);
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        return PartialView("General/_DocumentTemplatesAddEdit", model);
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
        public JsonResult DocumentTemplatesSave(DocumentTemplatesViewModel model, IFormFile file)
        {
            /**
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("id");
            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10004", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            if (!FileValidator.ForMSOfficeWithPdf(file)) { return Json(new { status = "error", message = "There is problem with File." });}
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            string uploadsFolder = Path.Combine(GblDocumentPath, "downloads");

            string? document_title = model.document_title;
            string? document_version = model.document_version;
            string? document_desc = model.document_desc;
            string status = model.status ?? "";

            if (mode == "add")
            {
                /** check if the data is exits on another record**/
                var isData = _context.tbl_document_templates.FirstOrDefault(u => u.document_title == document_title);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }

                string id = UniqueID();
                var DataSave = new tbl_document_templates
                {
                    id = id,
                    document_title = document_title,
                    document_version = document_version,
                    document_desc = document_desc,
                    status = status
                };
                _ = _context.tbl_document_templates.Add(DataSave);
                _ = _context.SaveChanges();
                _context.ChangeTracker.Clear();

                var isDataSaved = _context.tbl_document_templates.FirstOrDefault(u => u.id == id);
                if (isDataSaved == null)
                {
                    return Json(new { status = "false", message = "Fail to save document template." });
                }

                if (file != null)
                {
                    UploadFile(uploadsFolder, file, out string uStatus, out string? filename);
                    if (string.Equals(uStatus, "true", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(filename))
                    {
                        isDataSaved.upload_file = filename;
                        isDataSaved.upload_date = DateTime.Now;

                        _ = _context.tbl_document_templates.Update(isDataSaved);
                        _ = _context.SaveChanges();

                    }
                    else
                    {
                        return Json(new { status = "false", message = "Document template information saved successfully, however the associated file could not be uploaded." });
                    }
                }
                return Json(new { status = "success", message = Lang.msg_added_success, id });
            }
            else if (mode == "edit")
            {
                string? id = Request.Form["id"];
                string? hid_doc_also = Request.Form["hid_doc_also"];
                string? h_upload_file = Request.Form["h_file_name"];
                if (string.IsNullOrWhiteSpace(id))
                {
                    return Json(new { status = "false", message = "Insufficient information." });
                }
                /** check if the data is exits on another record **/
                var isData = _context.tbl_document_templates.FirstOrDefault(u => u.document_title == document_title && u.id != id);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                var DataUpdate = _context.tbl_document_templates.FirstOrDefault(h => h.id == id);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }

                DataUpdate.document_title = document_title;
                DataUpdate.document_version = document_version;
                DataUpdate.document_desc = document_desc;

                if (file != null && string.Equals(hid_doc_also, "Y", StringComparison.OrdinalIgnoreCase))
                {
                    /** DELETE EXISTING FILE | instead of taking from post, better to get from db | security reason **/
                    string hUploadFile = DataUpdate.upload_file ?? "";
                    if (!string.IsNullOrWhiteSpace(hUploadFile))
                    {
                        string st = DeleteFile(uploadsFolder, hUploadFile);
                        if (!string.Equals(st, "true", StringComparison.OrdinalIgnoreCase))
                        {
                            return Json(new { status = "false", message = "Failed to overwite existing document template file. Please contact your system administrator." });
                        }
                    }
                    UploadFile(uploadsFolder, file, out string uStatus, out string? filename);
                    if (string.Equals(uStatus, "true", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(filename))
                    {
                        DataUpdate.upload_file = filename;
                    }
                    else
                    {
                        return Json(new { status = "false", message = "Failed to update document template file. Please contact your system administrator." });
                    }
                }
                DataUpdate.upload_date = DateTime.Now;
                DataUpdate.status = status;

                _ = _context.tbl_document_templates.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DocumentTemplatesDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10004", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            // Validate input
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            var recordsToDelete = _context.tbl_document_templates
                .Where(r => request.SelectedIds.Contains(r.id.ToString())).ToList();
            if (recordsToDelete.Count < 1)
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }
            _context.ChangeTracker.Clear();

            int DelCnt = 0;
            var deletedIds = new List<string>();
            string GblDocumentPath = _globalOptionServices.OptionServices["op_document_file_path_out"];
            string uploadsFolder = Path.Combine(GblDocumentPath, "downloads");
            foreach (string id in request.SelectedIds)
            {
                var smt = _context.tbl_document_templates.FirstOrDefault(h => h.id == id);
                if (smt != null)
                {
                    string hUploadFile = smt.upload_file ?? "";
                    if (!string.IsNullOrWhiteSpace(hUploadFile))
                    {
                        string st = DeleteFile(uploadsFolder, hUploadFile);
                        if (string.Equals(st, "true", StringComparison.OrdinalIgnoreCase))
                        {
                            DelCnt++;
                            deletedIds.Add(smt.id);
                        }
                    }
                }
            }
            if (deletedIds.Count > 0)
            {
                var entitiesToDelete = _context.tbl_document_templates.Where(t => deletedIds.Contains(t.id)).ToList();
                _context.tbl_document_templates.RemoveRange(entitiesToDelete);
                _ = await _context.SaveChangesAsync().ConfigureAwait(false);
            }
            return Ok(new
            {
                status = "success",
                deletedCount = request.SelectedIds.Count,
                message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", DelCnt.ToString(), StringComparison.Ordinal)
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DocumentTemplatesUpdateStatus([FromBody] bulkStatusUpdateRequest request)
        {
            if (!_accountServices.HasPermission("10004", "edit")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            // Validate input
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            string mode = request.mode;
            string hStatus = request.hStatus;
            hStatus = hStatus == "Activate" ? "A" : "D";
            if (string.Equals(mode, "updateStatus", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(hStatus))
            {
                int updatedCount = _context.tbl_document_templates
                    .Where(r => request.SelectedIds.Contains(r.id.ToString()))
                    .ExecuteUpdate(setters => setters
                        .SetProperty(r => r.status, hStatus)
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

        #region FUND SOURCE DEFAULT
        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult FundSourceDefault()
        {
            string PageId = "10002";
            #region FOR PERMISSION
            var perm = _accountServices.GetMenuPermission(PageId);
            if (perm.vpern == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            ViewBag.apern = perm.apern;
            ViewBag.epern = perm.epern;
            #endregion FOR END PERMISSION

            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            FundSourceViewModel model;
            var Records = _context.tbl_fund_source.FirstOrDefault(a => a.default_for_holiday == "1");
            if (Records == null)
            {
                ViewBag.mode = "add";
                model = new FundSourceViewModel
                {
                    fund_id = 0,
                    fund_source = "",
                    fund_desc = "",
                    fund_status = "",
                    expiry_date = null,
                    default_for_holiday = ""
                };
            }
            else
            {
                ViewBag.mode = "edit";
                model = new FundSourceViewModel
                {
                    fund_id = Records.fund_id,
                    fund_source = Records.fund_source,
                    fund_desc = Records.fund_desc,
                    fund_status = Records.fund_status,
                    expiry_date = Records.expiry_date,
                    default_for_holiday = Records.default_for_holiday
                };
            }
            return PartialView("General/_FundSourceDefault", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult FundSourceDefaultSave(FundSourceViewModel model)
        {
            /**
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("fund_id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }

            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10002", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string fund_source = model.fund_source ?? "";
            string fund_desc = model.fund_desc ?? "";
            var expiry_date = Convert.ToDateTime(model.expiry_date);

            if (mode == "add")
            {
                //check if the data is exits on another record
                var isData = _context.tbl_fund_source.FirstOrDefault(u => u.fund_source == fund_source);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                int fund_id = (_context.tbl_fund_source.Any()
                                ? _context.tbl_fund_source.Max(o => o.fund_id)
                                : 0) + 1;
                var DataSave = new tbl_fund_source
                {
                    fund_id = fund_id,
                    fund_source = fund_source,
                    fund_desc = fund_desc,
                    fund_status = "A",
                    expiry_date = expiry_date,
                    default_for_holiday = "1"
                };
                _ = _context.tbl_fund_source.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id = fund_id });
            }
            else if (mode == "edit")
            {
                int fund_id = model.fund_id;
                //check if the data is exits on another record
                var isData = _context.tbl_fund_source
                        .FirstOrDefault(u => u.fund_source == fund_source &&
                        u.fund_id != fund_id
                        );
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                var DataUpdate = _context.tbl_fund_source.FirstOrDefault(h => h.fund_id == fund_id);
                if (DataUpdate == null) { return Json(new { status = "notfound", message = Lang.msg_no_record_found }); }

                DataUpdate.fund_source = fund_source;
                DataUpdate.fund_desc = fund_desc;
                DataUpdate.expiry_date = expiry_date;

                _ = _context.tbl_fund_source.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.fund_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }

        #endregion

        #region FUND SOURCE

        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult FundSource()
        {
            string PageId = "10003";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_fund_source
                where a.fund_status == "A"
                orderby a.fund_id descending
                select new FundSourceViewModel
                {
                    fund_id = a.fund_id,
                    fund_source = a.fund_source,
                    fund_desc = a.fund_desc,
                    fund_status = a.fund_status,
                    expiry_date = a.expiry_date,
                    default_for_holiday = a.default_for_holiday
                }).ToList();
            ViewBag.StatusFilter = StatusActivePassive("AD", "A");
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("General/FundSource", "ADD|EXPORT|ACT-DACT", PageId, Records.Count);
            return PartialView("General/_FundSource", Records);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FundSourceList([FromForm] CostumFilterRequest request)
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            string StatusFilter = request.FilterValue;/*Dropdwon Filter*/
            var query = _context.tbl_fund_source
                .OrderByDescending(a => a.fund_id)
                .Select(a => new FundSourceViewModel
                {
                    fund_id = a.fund_id,
                    fund_source = a.fund_source,
                    fund_desc = a.fund_desc,
                    fund_status = a.fund_status,
                    expiry_date = a.expiry_date,
                    default_for_holiday = a.default_for_holiday
                });

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(d => d.fund_status == StatusFilter && d.default_for_holiday != "1");/*filter*/
            }
            else
            {
                query = query.Where(d => d.default_for_holiday != "1"); /*default*/
            }
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(a =>
                    (a.fund_source != null && a.fund_source.Contains(searchValue)) ||
                    (a.fund_desc != null && a.fund_desc.Contains(searchValue))
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
        public IActionResult FundSourceAddEdit(int? id, string mode)
        {
            string PageId = "10003";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.FundStatus = StatusActivePassive("AD");
            ViewBag.mode = mode;
            ViewBag.DATE_FORMAT = _appSettings.DATE_FORMAT;
            FundSourceViewModel model;
            /** this is to load blank form while doing add process **/
            model = new FundSourceViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("General/_FundSourceAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id < 1 || string.IsNullOrWhiteSpace(id.ToString()))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = _context.tbl_fund_source.FirstOrDefault(h => h.fund_id == Convert.ToInt32(id));
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        model = new FundSourceViewModel
                        {
                            fund_id = Convert.ToInt32(smt.fund_id),
                            fund_source = smt.fund_source,
                            fund_desc = smt.fund_desc,
                            fund_status = smt.fund_status,
                            expiry_date = smt.expiry_date,
                            default_for_holiday = smt.default_for_holiday
                        };
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        return PartialView("General/_FundSourceAddEdit", model);
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
        public JsonResult FundSourceSave(FundSourceViewModel model)
        {
            /**
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("fund_id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10003", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string? fund_source = model.fund_source;
            string? fund_desc = model.fund_desc;
            string? fund_status = model.fund_status;
            DateTime? expiry_date = Convert.ToDateTime(model.expiry_date);
            string? default_for_holiday = model.default_for_holiday;

            if (mode == "add")
            {
                var isData = _context.tbl_fund_source.FirstOrDefault(u => u.fund_source == fund_source);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                int fund_id = (_context.tbl_fund_source.Any()
                                ? _context.tbl_fund_source.Max(o => o.fund_id)
                                : 0) + 1;
                var DataSave = new tbl_fund_source
                {
                    fund_id = fund_id,
                    fund_source = fund_source,
                    fund_desc = fund_desc,
                    fund_status = fund_status,
                    expiry_date = expiry_date,
                    default_for_holiday = "0"
                };
                _ = _context.tbl_fund_source.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id = fund_id });
            }
            else if (mode == "edit")
            {
                int fund_id = model.fund_id;
                var isData = _context.tbl_fund_source.FirstOrDefault(u => u.fund_source == fund_source && u.fund_id != fund_id);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                var DataUpdate = _context.tbl_fund_source.FirstOrDefault(h => h.fund_id == fund_id);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }
                DataUpdate.fund_source = fund_source;
                DataUpdate.fund_desc = fund_desc;
                DataUpdate.fund_status = fund_status;
                DataUpdate.expiry_date = expiry_date;

                _ = _context.tbl_fund_source.Update(DataUpdate);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.fund_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> FundSourceDelete([FromBody] DeleteRequest request)
        //{
        //    /* This feature was never issued to client
        //    if (!_accountServices.HasPermission("10003", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }
        //    // Validate input
        //    if (request?.SelectedIds == null || !request.SelectedIds.Any())
        //    {
        //        return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
        //    }
        //    // matching records
        //    var recordsToDelete = _context.tbl_fund_source.Where(r => request.SelectedIds.Contains(r.fund_id.ToString())).ToList();
        //    if (recordsToDelete < 1 )
        //    {
        //        return NotFound(new { status = "false", message = Lang.msg_no_record_found });
        //    }
        //    bool recordsExist =
        //        _context.tbl_employee_fund_source.Any(r => request.SelectedIds.Contains(r.fund_id.ToString()))
        //        || _context.tbl_employee_travel_codes.Any(r => request.SelectedIds.Contains(r.fund_id.ToString()))
        //        || _context.tbl_employee_travel_settlement_main.Any(r =>
        //            new[] { r.charge_fund_id_1, r.charge_fund_id_2, r.charge_fund_id_3, r.charge_fund_id_4 }
        //                .Select(x => x.ToString())
        //                .Any(id => request.SelectedIds.Contains(id)));
        //    if (recordsExist)
        //    {
        //        return BadRequest(new { success = false, message = Lang.msg_delete_fail });
        //    }
        //    _context.tbl_fund_source.RemoveRange(recordsToDelete);
        //    await _context.SaveChangesAsync();
        //    var deletedCount = recordsToDelete.Count;
        //    return Ok(new
        //    {
        //        status = "success",
        //        deletedCount = deletedCount,
        //        message = Lang.msg_delete_success.Replace("[<DELETED-ROWS>]", deletedCount.ToString())
        //    });
        //    */
        //    return StatusCode(500, new { success = false, message = "Never try this" });
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FundSourceUpdateStatus([FromBody] bulkStatusUpdateRequest request)
        {
            if (!_accountServices.HasPermission("10003", "edit")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            // Validate input
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            string mode = request.mode;
            string hStatus = request.hStatus;
            hStatus = hStatus == "Activate" ? "A" : "D";

            if (mode == "updateStatus" && !string.IsNullOrWhiteSpace(hStatus))
            {
                /** Bulk update all selected IDs */
                int updatedCount = _context.tbl_fund_source
                    .Where(r => request.SelectedIds.Contains(r.fund_id.ToString()))
                    .ExecuteUpdate(setters => setters
                        .SetProperty(r => r.fund_status, hStatus)
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
        public IActionResult FundSourceExport()
        {
            var sb = new StringBuilder();
            _ = sb.AppendLine("SN, Fund Source, Project Description, Expiry Date, Status");

            int cnt = 0;
            var Records = _context.tbl_fund_source
                .OrderBy(a => a.fund_source)
                .Select(a => new FundSourceViewModel
                {
                    fund_id = a.fund_id,
                    fund_source = a.fund_source,
                    fund_desc = a.fund_desc,
                    expiry_date = a.expiry_date,
                    fund_status = a.fund_status
                }).ToList();
            string new_expiry_date = "";
            string is_active = "";
            if (Records.Count > 0)
            {
                foreach (var record in Records)
                {
                    cnt++;
                    new_expiry_date = _settingsServices.DateformatToDt(record.expiry_date.ToString() ?? "");
                    is_active = record.fund_status == "A" ? "Active" : "Inactive";
                    string fund_source = EscapeCSV(record.fund_source ?? "");
                    string fund_desc = EscapeCSV(record.fund_desc ?? "");
                    string expiry_date = EscapeCSV(new_expiry_date);
                    string fund_status = EscapeCSV(is_active);
                    _ = sb.AppendLine($"{cnt},\"{fund_source}\",\"{fund_desc}\",\"{expiry_date}\",{fund_status}");
                }
            }
            else
            {
                _ = sb.AppendLine($"No record(s) found");
            }
            byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "FundSourceExport.csv");
        }

        #endregion

        #region CONTRACT DOCUMENT TEMPLATES

        // Renders the standard HTML page layout
        [HttpGet]
        public IActionResult ContractDocumentTemplate()
        {
            string PageId = "10001";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            var Records = (
                from a in _context.tbl_contract_document_template
                orderby a.contract_document_id descending
                select new ContractDocumentTemplateViewModel
                {
                    contract_document_id = a.contract_document_id,
                    document_subject = a.document_subject,
                    document_desc = a.document_desc
                }).ToList();
            ViewBag.ViewButtons = _accountServices.getAddEditDeleteAccess("General/ContractDocumentTemplate", "ADD|DEL", PageId, Records.Count);
            return PartialView("General/_ContractDocumentTemplate", Records);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContractDocumentTemplateList()
        {
            var (pageSize, skip, draw, sortColumn, sortColumnDir, searchValue) = DataTableHelper.GetParameters(Request);
            var query = _context.tbl_contract_document_template
                .OrderByDescending(a => a.contract_document_id)
                .Select(a => new ContractDocumentTemplateViewModel
                {
                    contract_document_id = a.contract_document_id,
                    document_subject = a.document_subject,
                    document_desc = a.document_desc
                });

            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDir))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDir);
            }
            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(
                    a => a.document_subject.Contains(searchValue)
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
        public IActionResult ContractDocumentTemplateAddEdit(int? id, string mode)
        {
            string PageId = "10001";
            #region FOR PERMISSION
            string perm = _accountServices.GetSingleMenuPermission(PageId, "V") ?? "false";
            if (perm == "false") { return RedirectToAction("PermissionDenied", "Home"); }
            #endregion FOR END PERMISSION

            ViewBag.mode = mode;
            ContractDocumentTemplateViewModel model;
            /** this is to load blank form while doing add process*/
            model = new ContractDocumentTemplateViewModel();
            if (mode == "add")
            {
                ViewBag.apern = _accountServices.GetSingleMenuPermission(PageId, "A") ?? "false";
                return PartialView("General/_ContractDocumentTemplateAddEdit", model);
            }
            else if (mode == "edit")
            {
                if (id < 1 || string.IsNullOrWhiteSpace(id.ToString()))
                {
                    return BadRequest(new { success = false, message = Lang.msg_error });
                }
                else
                {
                    var smt = _context.tbl_contract_document_template.FirstOrDefault(h => h.contract_document_id == Convert.ToInt32(id));
                    if (smt == null)
                    {
                        return BadRequest(new { success = false, message = Lang.msg_error });
                    }
                    else
                    {
                        model = new ContractDocumentTemplateViewModel
                        {
                            contract_document_id = Convert.ToInt32(smt.contract_document_id),
                            document_subject = smt.document_subject,
                            document_desc = smt.document_desc
                        };
                        ViewBag.epern = _accountServices.GetSingleMenuPermission(PageId, "E") ?? "false";
                        return PartialView("General/_ContractDocumentTemplateAddEdit", model);
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
        public JsonResult ContractDocumentTemplateSave(ContractDocumentTemplateViewModel model)
        {
            /**
             * If not put ModelState.Remove for primary key
             * ModelState.IsValid return false, even though 
             * the rest of your data is fine
             * Ignore validation errors for this field, 
             * don’t block the save just because ID wasn’t posted
             * When mode is add the id will be certainly no value
            */
            _ = ModelState.Remove("contract_document_id");

            if (!ModelState.IsValid)
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
            string? mode = Request.Form["mode"];
            if (!_accountServices.HasPermission("10001", mode)) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            string document_subject = model.document_subject;
            string document_desc = model.document_desc;

            if (mode == "add")
            {
                /** check if the data is exits on another record */
                var isData = _context.tbl_contract_document_template.FirstOrDefault(u => u.document_subject == document_subject);
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                int contract_document_id = (_context.tbl_contract_document_template.Any()
                                ? _context.tbl_contract_document_template.Max(o => o.contract_document_id)
                                : 0) + 1;
                var DataSave = new tbl_contract_document_template
                {
                    contract_document_id = contract_document_id,
                    document_subject = document_subject,
                    document_desc = document_desc
                };
                _ = _context.tbl_contract_document_template.Add(DataSave);
                _ = _context.SaveChanges();

                return Json(new { status = "success", message = Lang.msg_added_success, id = contract_document_id });
            }
            else if (mode == "edit")
            {
                int contract_document_id = model.contract_document_id;

                //check if the data is exits on another record
                var isData = _context.tbl_contract_document_template
                        .FirstOrDefault(u => u.document_subject == document_subject &&
                        u.contract_document_id != contract_document_id
                        );
                if (isData != null)
                {
                    return Json(new { status = "false", message = Lang.msg_record_exist_other });
                }
                var DataUpdate = _context.tbl_contract_document_template.FirstOrDefault(h => h.contract_document_id == contract_document_id);
                if (DataUpdate == null)
                {
                    return Json(new { status = "notfound", message = Lang.msg_no_record_found });
                }
                DataUpdate.document_subject = document_subject;
                DataUpdate.document_desc = document_desc;
                _ = _context.tbl_contract_document_template.Update(DataUpdate);
                _ = _context.SaveChanges();
                return Json(new { status = "success", message = Lang.msg_update_success, id = DataUpdate.contract_document_id });
            }
            else
            {
                return Json(new { status = "invalid", message = Lang.msg_error_invalid });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContractDocumentTemplateDelete([FromBody] DeleteRequest request)
        {
            if (!_accountServices.HasPermission("10001", "delete")) { return Json(new { status = "error", message = Lang.msg_permission_denied }); }

            // Validate input
            if (request?.SelectedIds == null || request.SelectedIds.Count < 1)
            {
                return BadRequest(new { status = false, message = Lang.msg_no_record_selected });
            }
            var recordsToDelete = _context.tbl_contract_document_template
                .Where(r => request.SelectedIds.Contains(r.contract_document_id.ToString())).ToList();
            if (recordsToDelete.Count < 1)
            {
                return NotFound(new { status = "false", message = Lang.msg_no_record_found });
            }

            bool recordsExist = _context.tbl_employee_contract.Any(
                r => request.SelectedIds.Contains(r.contract_document_id.ToString() ?? ""));
            if (recordsExist)
            {
                //FK record exists | Canot delete
                return BadRequest(new { success = false, message = Lang.msg_delete_fail });
            }
            _context.tbl_contract_document_template.RemoveRange(recordsToDelete);
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
