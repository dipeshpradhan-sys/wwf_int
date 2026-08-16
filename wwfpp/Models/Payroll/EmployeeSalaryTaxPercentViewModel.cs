namespace wwfpp.Models.Payroll
{
    public class EmployeeSalaryTaxPercentViewModel
    {
        //One to One    
        public int emp_id { get; set; } //[int] NULL,
        public string? percent_for_tax_add { get; set; }  //[nvarchar](1) NULL
    }

}
