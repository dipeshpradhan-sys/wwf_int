namespace wwfpp.Models.Settings
{
    public class DifferentialSalaryViewModel
    {
        /// <summary>
        /// IDENTITY(1,1) NOT NULL PRIMARY KEY,
        /// </summary>
        public int? id { get; set; }
        public string fiscal_year { get; set; }
        public string fiscal_year_abb { get; set; }
        public int emp_id { get; set; }
        public string? employee { get; set; }
        public short emp_year { get; set; }         // smallint not null,
        public  byte emp_month { get; set; }        // tinyint not null,
        public decimal basic_salary { get; set; }  // money NOT NULL DEFAULT 0,
        public decimal pf_a { get; set; }          //money NOT NULL DEFAULT 0,
        public decimal gratuity_a { get; set; }    //money NOT NULL DEFAULT 0,
        public decimal ssf_a { get; set; }         //money NOT NULL DEFAULT 0,
        public decimal pf_d  { get; set; }         //money NOT NULL DEFAULT 0,
        public decimal gratuity_d { get; set; }    //money NOT NULL DEFAULT 0,
        public decimal ssf_d  { get; set; }        //money NOT NULL DEFAULT 0,
        public string emp_code { get; set; }       //nvarchar(6) NOT NULL
    }
    public class DifferentialSalaryExportViewModel
    {
        public string? employee { get; set; }
        public string emp_code { get; set; }       //nvarchar(6) NOT NULL
    }
    /*may be remove later
    public class DifferentialSalaryImportViewModel
    {
        public int emp_id { get; set; }
        public short emp_year { get; set; }        
        public byte emp_month { get; set; }        
        public decimal basic_salary { get; set; }  
        public decimal pf_a { get; set; }          
        public decimal gratuity_a { get; set; }    
        public decimal ssf_a { get; set; }       
        public decimal pf_d { get; set; }         
        public decimal gratuity_d { get; set; }   
        public decimal ssf_d { get; set; }       
        public string emp_code { get; set; }      
        public string fiscal_year { get; set; }
    }
    */
}