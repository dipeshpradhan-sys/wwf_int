namespace wwfpp.Models.Payroll
{
    public class FieldStaffSalaryViewModel
    {
        public string? salary_id { get; set; }  //[nvarchar](50) NOT NULL,
        public int? emp_id { get; set; }//[int] NULL,
        public short? sal_year { get; set; }  //[smallint] NULL,
        public short? sal_month { get; set; }  //[smallint] NULL,
        public decimal? act_basic_salary { get; set; }  //[money] NULL,
        public decimal? act_pf_a { get; set; }  //[money] NULL,
        public decimal? act_pf_d { get; set; }  //[money] NULL,
        public decimal? a_cit_d { get; set; }  //[money] NULL,
        public decimal? act_remote_area_all { get; set; }  //[money] NULL,
        public decimal? basic_salary { get; set; }  //[money] NULL,
        public decimal? grade { get; set; }  //[money] NULL,
        public decimal? pf_a { get; set; }  //[money] NULL,
        public decimal? children_edu_all { get; set; }  //[money] NULL,
        public decimal? performance_all { get; set; }  //[money] NULL,
        public decimal? remote_area_all { get; set; }  //[money] NULL,
        public decimal? overtime { get; set; }  //[money] NULL,
        public decimal? dashain_a { get; set; }  //[money] NULL,
        public decimal? gratudi { get; set; }  //[money] NULL,
        public decimal? insurance { get; set; }  //[money] NULL,
        public decimal? others { get; set; }  //[money] NULL,
        public decimal? pf_d { get; set; }  //[money] NULL,
        public decimal? cit_d { get; set; }  //[money] NULL,
        public decimal? pre_access_tax { get; set; }  //[money] NULL,
        public decimal? incometax_d { get; set; }  //[money] NULL,
        public decimal? betalibi_d { get; set; }  //[money] NULL,
        public decimal? tel_per_adv { get; set; }  //[money] NULL,
        public decimal? travel_prog_adv { get; set; }  //[money] NULL,
        public decimal? pr_adv { get; set; }  //[money] NULL,
        public decimal? fd_adv { get; set; }  //[money] NULL,
        public decimal? welfare_fund { get; set; }  //[money] NULL,
        public decimal? adv_pf_loan { get; set; }  //adv_PF_loan [money] NULL,
        public decimal? adv_cit_loan { get; set; }  //adv_CIT_loan [money] NULL,
        public decimal? wl_adv { get; set; }  //[money] NULL,
        public decimal? net_in_hand { get; set; }  //[money] NULL,
        public string? remarks { get; set; }  //[nvarchar](100) NULL,
        public DateTime? submit_date { get; set; }  //[datetime] NULL,
        public int? submit_by { get; set; }  //[int] NULL,
        public string? fiscal_year { get; set; }  //[nvarchar](10) NULL,
        public byte? emp_week { get; set; }  //[tinyint] NULL,
        public decimal? gratuity { get; set; }  //[money] NULL,
        public decimal? gratuity_ded { get; set; }  //[money] NULL,
        public decimal? medical_expense_reimburse_total { get; set; }  //[money] NULL,
        public decimal? leave_encash { get; set; }  //[money] NULL,
        public decimal? ssf { get; set; }  //[money] NULL,
        public decimal? ssf_ded { get; set; }  //[money] NULL,
        public decimal? annual_health_checkup_add { get; set; }  //[money] NULL,
        public decimal? annual_health_checkup_ded { get; set; }  //[money] NULL,
    }
    public class FieldStaffSalaryListViewModel
    {
        public string? mode { get; set; }
        public List<string> selectedIds { get; set; }
        public List<FieldStaffSalaryViewModel> Fields { get; set; }
    }

}
