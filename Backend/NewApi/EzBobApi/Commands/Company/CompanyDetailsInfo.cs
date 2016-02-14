namespace EzBobApi.Commands.Company
{
    using EzBobModels.Enums;

    /// <summary>
    /// Contains company related data
    /// </summary>
    public class CompanyDetailsInfo {
        public IndustryType? IndustryType { get; set; }
        public TypeOfBusiness TypeOfBusiness { get; set; }
        public string BusinessName { get; set; }
        public string MainPhoneNumber { get; set; }
        public string RegistrationNumber { get; set; }
        public decimal TotalAnnualRevenue { get; set; }
        public double TotalMonthlySalaryExpenditure { get; set; }
        public int NumberOfEmployees { get; set; }
        public bool IsVatRegistered { get; set; }
        public AuthorityInfo[] Authorities { get; set; }
    }
}
