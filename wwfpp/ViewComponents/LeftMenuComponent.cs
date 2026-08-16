using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using wwfpp.Data;
using wwfpp.Models.Account;

namespace wwfpp.ViewComponents
{
    public class LeftMenuViewComponent(AppDbContext context) : ViewComponent
    {
        private readonly AppDbContext _context = context;

        public async Task<IViewComponentResult> InvokeAsync(int UserId)
        {
            /** Get current logged-in user id*/
            if (UserId < 1)
            {
                // Return empty menu if not logged in
            }
            if (UserId == 1)
            {
                /**super adminstrator | have all permissions**/
                var data = await (
                    from mdl in _context.tbl_user_module
                    join mnu in _context.tbl_user_menu
                    on mdl.module_id equals mnu.module_id
                    orderby mdl.module_sort ascending, mnu.menu_sort ascending
                    select new
                    {
                        mdl.module_id,
                        mdl.module_code,
                        mdl.module_name,
                        mdl.module_label,
                        mdl.module_folder,
                        mnu.menu_id,
                        mnu.menu_code,
                        mnu.menu_name,
                        mnu.menu_label,
                        mnu.menu_page
                    }
                ).ToListAsync().ConfigureAwait(false);
                /** Group by module and build DTOs **/
                var modules = data
                    .GroupBy(x => new
                    {
                        x.module_id,
                        x.module_code,
                        x.module_name,
                        x.module_label,
                        x.module_folder
                    })
                    .Select(g => new ModuleDto
                    {
                        module_id = g.Key.module_id,
                        module_code = g.Key.module_code,
                        module_name = g.Key.module_name,
                        module_label = g.Key.module_label,
                        module_folder = g.Key.module_folder,
                        Menus = [.. g.Select(m => new MenuDto
                        {
                            menu_id = m.menu_id,
                            menu_code = m.menu_code,
                            menu_name = m.menu_name,
                            menu_label = m.menu_label,
                            menu_page = m.menu_page
                        })]
                    })
                    .ToList();
                var viewModel = new UserModuleMenuLeftViewModel
                {
                    UserLeftModuleMenu = modules
                };
                return View(viewModel);
            }
            else
            {
                /** Query modules and menus for this user**/
                var data = await (
                    from mdl in _context.tbl_user_module
                    join umd in _context.tbl_user_user_module
                        on mdl.module_id equals umd.module_id
                    join mnu in _context.tbl_user_menu
                        on mdl.module_id equals mnu.module_id
                    join umn in _context.tbl_user_user_menu
                        on new { mnu.menu_id, umd.user_id }
                        equals new { umn.menu_id, umn.user_id }
                    where mdl.module_status == "A" && mnu.menu_status == "A" &&
                        umd.user_id == UserId && umn.is_vw == "Y"
                    orderby mdl.module_sort ascending, mnu.menu_sort ascending
                    select new
                    {
                        mdl.module_id,
                        mdl.module_code,
                        mdl.module_name,
                        mdl.module_label,
                        mdl.module_folder,
                        mnu.menu_id,
                        mnu.menu_code,
                        mnu.menu_name,
                        mnu.menu_label,
                        mnu.menu_page,
                    }
                ).ToListAsync().ConfigureAwait(false);
                /** Group by module and build DTOs*/
                var modules = data
                    .GroupBy(x => new
                    {
                        x.module_id,
                        x.module_code,
                        x.module_name,
                        x.module_label,
                        x.module_folder
                    })
                    .Select(g => new ModuleDto
                    {
                        module_id = g.Key.module_id,
                        module_code = g.Key.module_code,
                        module_name = g.Key.module_name,
                        module_label = g.Key.module_label,
                        module_folder = g.Key.module_folder,
                        Menus = [.. g.Select(m => new MenuDto
                        {
                            menu_id = m.menu_id,
                            menu_code = m.menu_code,
                            menu_name = m.menu_name,
                            menu_label = m.menu_label,
                            menu_page = m.menu_page
                        })]
                    })
                    .ToList();
                var viewModel = new UserModuleMenuLeftViewModel
                {
                    UserLeftModuleMenu = modules
                };
                return View(viewModel);
            }
        }
    }
}
