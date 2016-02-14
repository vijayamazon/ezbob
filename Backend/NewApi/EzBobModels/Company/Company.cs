namespace EzBobModels.Company {
    using EzBobModels.Enums;

    public class Company {
        public int? Id { get; set; }
        public TypeOfBusiness TypeOfBusiness { get; set; }
        public string VatReporting { get; set; }
        public bool VatRegistered { get; set; }
        public string CompanyNumber { get; set; }
        public string CompanyName { get; set; }
        public int? TimeAtAddress { get; set; }
        public string TimeInBusiness { get; set; }
        public string BusinessPhone { get; set; }
        public bool? PropertyOwnedByCompany { get; set; }
        public string YearsInCompany { get; set; }
        public string RentMonthLeft { get; set; }
        public double? CapitalExpenditure { get; set; }
        public string ExperianRefNum { get; set; }
        public string ExperianCompanyName { get; set; }
    }
}
