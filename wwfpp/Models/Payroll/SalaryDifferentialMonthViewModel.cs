namespace wwfpp.Models.Payroll
{
    public class SalaryDifferentialMonthViewModel
    {
        public string fiscal_year { get; set; }  //[nvarchar](10) NOT NULL,
        public short? sal_year { get; set; }  //[smallint] NULL,
        public byte? sal_month { get; set; }  //[tinyint] NULL,
    }

}
