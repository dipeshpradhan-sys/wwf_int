namespace wwfpp.Models.Payroll
{
    public class SalaryPreviousViewModel
    {
        public int id { get; set; }   // maps sal_id
        public int empid { get; set; }
        public int? salyear { get; set; }
        public int? salmonth { get; set; }
        public decimal? basicsalary { get; set; }       // t_basic_salary
        public decimal? betalabideduction { get; set; } // t_betalabi
        public decimal? pfaddition { get; set; }        // t_pf
        public decimal? pfdeduction { get; set; }       // t_pf_d
        public decimal? allowance { get; set; }         // t_allow
        public decimal? citdeduction { get; set; }      // t_cit_d
        public decimal? remoteareaallowance { get; set; } // t_raa
        public decimal? lipreimbursement { get; set; }  // t_lip_rem
        public decimal? prevyearexcesslesstax { get; set; } // t_tax_pre
        public decimal? dashainbonus { get; set; }      // t_dashain
        public decimal? taxdeduction { get; set; }      // t_tax
        public string? remarks { get; set; }
        public string? fiscalyear { get; set; }
        public byte? empweek { get; set; }
        public string? emp_status { get; set; }
        public string? firstname { get; set; }
        public string? middlename { get; set; }
        public string? lastname { get; set; }
        public string? employee { get; set; }
        public string? empcode { get; set; }
    }

}
