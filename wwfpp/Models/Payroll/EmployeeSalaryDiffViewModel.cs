namespace wwfpp.Models.Payroll
{
    public class EmployeeSalaryDiffViewModel
    {
        public int id { get; set; }  //[int] IDENTITY(1,1) NOT NULL,
        public int emp_id { get; set; }
        public short emp_year { get; set; }  //[smallint] NOT NULL,
        public byte emp_month { get; set; }  //[tinyint] NOT NULL,
        public decimal basic_salary { get; set; }  //[money] NOT NULL,
        public decimal pf_a { get; set; }  //[money] NOT NULL,
        public decimal gratuity_a { get; set; }  //[money] NOT NULL,
        public decimal ssf_a { get; set; }  //[money] NOT NULL,
        public decimal pf_d { get; set; }  //[money] NOT NULL,
        public decimal gratuity_d { get; set; }  //[money] NOT NULL,
        public decimal ssf_d { get; set; }  //[money] NOT NULL,
        public string emp_code { get; set; }  //[nvarchar](6) NOT NULL,
    }

}
