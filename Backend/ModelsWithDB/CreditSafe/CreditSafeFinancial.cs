using System;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeFinancial
    {

        [PK(true)]
        [NonTraversable]
        public long CreditSafeFinancialID { get; set; }
        [FK("CreditSafeBaseData", "CreditSafeBaseDataID")]
        public long? CreditSafeBaseDataID { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? PeriodMonths { get; set; }
        [Length(10)]
        public string Currency { get; set; }
        public bool? ConsolidatedAccounts { get; set; }
        public int? Turnover { get; set; }
        public int? Export { get; set; }
        public int? CostOfSales { get; set; }
        public int? GrossProfit { get; set; }
        public int? WagesSalaries { get; set; }
        public int? DirectorEmoluments { get; set; }
        public int? OperatingProfits { get; set; }
        public int? Depreciation { get; set; }
        public int? AuditFees { get; set; }
        public int? InterestPayments { get; set; }
        public int? Pretax { get; set; }
        public int? Taxation { get; set; }
        public int? PostTax { get; set; }
        public int? DividendsPayable { get; set; }
        public int? RetainedProfits { get; set; }
        public int? TangibleAssets { get; set; }
        public int? IntangibleAssets { get; set; }
        public int? FixedAssets { get; set; }
        public int? CurrentAssets { get; set; }
        public int? TradeDebtors { get; set; }
        public int? Stock { get; set; }
        public int? Cash { get; set; }
        public int? OtherCurrentAssets { get; set; }
        public int? IncreaseInCash { get; set; }
        public int? MiscellaneousCurrentAssets { get; set; }
        public int? TotalAssets { get; set; }
        public int? TotalCurrentLiabilities { get; set; }
        public int? TradeCreditors { get; set; }
        public int? OverDraft { get; set; }
        public int? OtherShortTermFinance { get; set; }
        public int? MiscellaneousCurrentLiabilities { get; set; }
        public int? OtherLongTermFinance { get; set; }
        public int? LongTermLiabilities { get; set; }
        public int? OverDrafeLongTermLiabilities { get; set; }
        public int? Liabilities { get; set; }
        public int? NetAssets { get; set; }
        public int? WorkingCapital { get; set; }
        public int? PaidUpEquity { get; set; }
        public int? ProfitLossReserve { get; set; }
        public int? SundryReserves { get; set; }
        public int? RevalutationReserve { get; set; }
        public int? Reserves { get; set; }
        public int? ShareholderFunds { get; set; }
        public int? Networth { get; set; }
        public int? NetCashFlowFromOperations { get; set; }
        public int? NetCashFlowBeforeFinancing { get; set; }
        public int? NetCashFlowFromFinancing { get; set; }
        public bool? ContingentLiability { get; set; }
        public int? CapitalEmployed { get; set; }
        public int? Employees { get; set; }
        [Length(100)]
        public string Auditors { get; set; }
        [Length(100)]
        public string AuditQualification { get; set; }
        [Length(100)]
        public string Bankers { get; set; }
        [Length(100)]
        public string BankBranchCode { get; set; }
        [Length(10)]
        public string PreTaxMargin { get; set; }
        [Length(10)]
        public string CurrentRatio { get; set; }
        [Length(10)]
        public string NetworkingCapital { get; set; }
        [Length(10)]
        public string GearingRatio { get; set; }
        [Length(10)]
        public string Equity { get; set; }
        [Length(10)]
        public string CreditorDays { get; set; }
        [Length(10)]
        public string DebtorDays { get; set; }
        [Length(10)]
        public string Liquidity { get; set; }
        [Length(10)]
        public string ReturnOnCapitalEmployed { get; set; }
        [Length(10)]
        public string ReturnOnAssetsEmployed { get; set; }
        [Length(10)]
        public string CurrentDebtRatio { get; set; }
        [Length(10)]
        public string TotalDebtRatio { get; set; }
        [Length(10)]
        public string StockTurnoverRatio { get; set; }
        [Length(10)]
        public string ReturnOnNetAssetsEmployed { get; set; }


    }
}
