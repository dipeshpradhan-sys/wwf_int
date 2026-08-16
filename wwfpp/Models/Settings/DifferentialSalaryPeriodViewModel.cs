namespace wwfpp.Models.Settings
{
    public class DifferentialSalaryPeriodViewModel
    {
        public required string fiscal_year { get; set; } //varchar not null pk
        public string? fiscal_year_abb { get; set; } //varchar 
        public short? sal_year { get; set; }         //small int null
        public byte? sal_month { get; set; }         // tiny int null    
    }
}