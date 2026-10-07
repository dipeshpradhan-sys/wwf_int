using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;
using wwfpp.Data;
using wwfpp.Models;
namespace wwfpp.Services
{
    public class GeneralServices
    {
        private readonly AppDbContext _context;
        public GeneralServices(AppDbContext context)
        {
            _context = context;
        }
        /***************************************************************************************************
        * Since : 2026-Jun-16
        ****************************************************************************************************/
        public SelectList GetFundSourceActiveOnly(string selvalue = "")
        {
            var fund_source = _context.tbl_fund_source
                .Where(e => e.fund_status == "A")
                .Select(e => new
                {
                    Value = e.fund_id,   // force nullable int
                    Text = (e.fund_source ?? "")
                })
                .OrderByDescending(e => e.Value)
                .ToList();

            return new SelectList(fund_source, "Value", "Text", selvalue);
        }
        /***************************************************************************************************
        * Since : 2026-Jul-11
        * Return active and then passive employees list
        ****************************************************************************************************/
        public SelectList GetFundSourceBoth()
        {
            var query = _context.tbl_fund_source
                .OrderByDescending(fnd => fnd.default_for_holiday)
                .OrderBy(fnd => fnd.fund_status)
                .ThenBy(fnd => fnd.fund_source)
                .AsQueryable();

            var FundSource = query
                .Select(fnd => new FundSourceDropDownViewModel
                {
                    fund_id = fnd.fund_id,
                    fund_source = $"{GblUtilities.GetSplitPart(fnd.fund_source, 0, " ")} [{GblUtilities.GetStringPart(fnd.fund_desc, 30)}]" +
                                  (fnd.default_for_holiday == "1"
                                      ? " [Default]"
                                      : (fnd.fund_status != "A" ? " [INACTIVE]" : "")),
                })
                .ToList();

            // Add separator if both lists requested
            var InDefaultCount = FundSource.Count(e => !e.fund_source.Contains("[Default]"));
            if (InDefaultCount > 0 && FundSource.Any(e => e.fund_source.Contains("[Default]")))
            {
                FundSource.Insert(InDefaultCount, new FundSourceDropDownViewModel
                {
                    fund_id = 0,
                    fund_source = "-- [Default] Fund Source(s) --"
                });
            }

            // Add separator if both lists requested
            var InActiveCount = FundSource.Count(e => !e.fund_source.Contains("[INACTIVE]"));
            if (InActiveCount > 0 && FundSource.Any(e => e.fund_source.Contains("[INACTIVE]")))
            {
                FundSource.Insert(InActiveCount, new FundSourceDropDownViewModel
                {
                    fund_id = 0,
                    fund_source = "-- [INACTIVE] Fund Source(s) --"
                });
            }

            return new SelectList(FundSource, "fund_id", "fund_source");
        }
        /***************************************************************************************************
        * Since : 2026-Oct-01
        ****************************************************************************************************/
        public string GetFundSourceName(int FundId)
        {
            if (FundId == 0) { return ""; }

; return _context.tbl_fund_source
                .Where(f => f.fund_id == FundId)
                .Select(f => f.fund_source)
                .FirstOrDefault() ?? "";
        }
        /***************************************************************************************************
        * Since : 2026-Oct-01
        ****************************************************************************************************/
    }
}

