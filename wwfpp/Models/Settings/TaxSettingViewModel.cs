namespace wwfpp.Models.Settings
{
    public class TaxSettingViewModel
    {
        public short Id { get; set; }      //smallint 
        public decimal? single_amt { get; set; }  //[money] NULL,
        public decimal? married_amt { get; set; }  //[money] NULL,
        public double? first_tax_percent { get; set; }  //[float] NULL,
        public double? second_tax_percent { get; set; }  //[float] NULL,
        public bool is_used_initial_tax_percent { get; set; }  //[bit] NOT NULL,
        public decimal? initial_tax_percent { get; set; }  //[money] NULL,
        public double? first_tax_amount { get; set; }  //[float] NULL,
        public decimal? second_tax_amount { get; set; }  //[money] NULL,
        public decimal? third_tax_amount_single { get; set; }  //[money] NULL,
        public decimal? third_tax_amount_married { get; set; }  //[money] NULL,
        public double? third_tax_percent { get; set; }  //[float] NULL,
        public double? fourth_tax_percent { get; set; }  //[float] NULL,
        public double? single_female_ded_per { get; set; }  //[float] NULL,
        public double? max_medical_expenses_reimbursed { get; set; }  //[float] NULL,
        public double? max_medical_tax_credit_amount { get; set; }  //[float] NULL,
        public double? max_medical_tax_credit_per { get; set; }  //[float] NULL,
        public decimal? ins_amt { get; set; }  //[money] NULL,
        public decimal? ins_amt_non_life { get; set; }  //[money] NULL,
        public decimal? fourth_tax_amount { get; set; }  //[money] NULL,
        public double? fifth_tax_percent { get; set; }  //[float] NULL
    }
}
