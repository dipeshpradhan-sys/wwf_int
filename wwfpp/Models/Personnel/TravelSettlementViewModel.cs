namespace wwfpp.Models.Personnel
{
    public class TravelSettlementListViewModel
    {
        public int EmpTravelId { get; set; }
        public string? TravelType { get; set; }
        public string? Destinations { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? SubmitDate { get; set; }
        public string? AppStatus { get; set; }

        public int? EmpId { get; set; }
        public string? Firstname { get; set; }
        public string? Middlename { get; set; }
        public string? Lastname { get; set; }
        public string? Employee { get; set; }
        public string? EmpStatus { get; set; }
        public string? TravSetId { get; set; }
        public bool HasDocuments { get; set; }
        public string SettlementType { get; set; }
        public string SettlementTypeDetail { get; set; }
        public string? ShowPrint { get; set; }
    }

    public class TravelSettlementViewModel
    {
        //Travel All Detail
        public TravelViewModel Travel { get; set; }

        // Settlement
        public TravelSettlementMainViewModel TravelSettlement { get; set; }
    }
    public class TravelSettlementMainViewModel
    {
        public string? TravSetId { get; set; }    //nvarchar
        public int? EmpTravelId { get; set; }   //int
        public int? EmpId { get; set; }  //int
        public DateTime? SubmitDate { get; set; } //datetime
        public DateTime? TravelDate { get; set; } //datetime
        public DateTime? ReturnDate { get; set; } //datetime
        public double? USDRate { get; set; }    //float
        public decimal? AdvCashLess { get; set; }   //money
        public string? ChargePerOrAmt { get; set; }   //nvarchar
        public int? ChargeFundId1 { get; set; }    //int
        public string? ChargeFundName1 { get; set; }
        public int? ChargeFundId2 { get; set; }    //int
        public string? ChargeFundName2 { get; set; }
        public int? ChargeFundId3 { get; set; }    //int
        public string? ChargeFundName3 { get; set; }
        public int? ChargeFundId4 { get; set; }    //int
        public string? ChargeFundName4 { get; set; }
        public double? ChargeFundPer1 { get; set; }   //float
        public double? ChargeFundPer2 { get; set; }   //float
        public double? ChargeFundPer3 { get; set; }   //float
        public double? ChargeFundPer4 { get; set; }   //float
        public decimal? ChargeFundAmt1 { get; set; }   //money
        public decimal? ChargeFundAmt2 { get; set; }   //money
        public decimal? ChargeFundAmt3 { get; set; }   //money
        public decimal? ChargeFundAmt4 { get; set; }   //money
        public string? Remarks { get; set; } //nvarchar
        public string? AppStatus { get; set; }  //nvarchar
        public int? AppBy { get; set; }  //int
        public string? AppByName { get; set; }
        public DateTime? AppDate { get; set; }    //datetime
        public string? IsForSet { get; set; }  //nvarchar   | is settlement required
        public string SettlementType { get; set; }
        public string SettlementTypeDetail { get; set; }

        public double? TChargeFundPer { get; set; }   //float
        public decimal? TChargeFundAmt { get; set; }   //money

        // Expenses
        public List<TravelSettlementSubViewModel> SetExpenses { get; set; }

        public decimal? TotalBill { get; set; }
        public decimal? TotalVAT { get; set; }
        public decimal? TotalTDS { get; set; }
        public decimal? TotalCash { get; set; } // Sum(IntUSDAmount) or sum(NatAmount) depend on travel type
        // Documents
        public List<TravelSettlementSubDocViewModel> SetDocs { get; set; }

        //To use in print section
        public string? EmployeeSignature { get; set; }
        public string? ApprovedSignature { get; set; }
        public decimal? NetCash { get; set; }   //AdvCashLess - TotalCash
    }
    public class TravelSettlementSubViewModel
    {
        public string? TravSetId { get; set; } //[nvarchar](50) NULL,
        public short? Sn { get; set; }  //[smallint] NULL,
        public DateTime? BillDate { get; set; }  //[datetime] NULL,
        public string? Location { get; set; }       //[nvarchar](255) NULL,
        public string? Description { get; set; }    //[nvarchar](255) NULL,
        public string? RefField { get; set; }   //[nvarchar](50) NULL, //ref is a reserved keyword in C#, so you can t use it directly as a property name.                        
        public string? IntCurName { get; set; }  //[nvarchar](50) NULL,   Int = international    
        public double? IntRate { get; set; }  //[float] NULL,
        public decimal? IntAmount { get; set; }  //[money] NULL,
        public decimal? IntUSDAmount { get; set; }  //[money] NULL,
        public decimal? NatBillAmount { get; set; }  //[money] NULL, = Nat = National
        public decimal? NatVAT { get; set; }  //[money] NULL,
        public decimal? NatTDS { get; set; }  //[money] NULL,
        public decimal? NatAmount { get; set; }  //[money] NULL
    }

    public class TravelSettlementSubDocViewModel
    {
        public string? TravSetDocId { get; set; }  //[nvarchar](50) NOT NULL,
        public string? DocName { get; set; }  //[nvarchar](250) NULL,
        public DateTime? SubmitDate { get; set; }  //[datetime] NULL,
        public string? TravSetId { get; set; } //[nvarchar](50) NULL,
    }
}
