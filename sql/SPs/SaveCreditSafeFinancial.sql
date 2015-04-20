SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCreditSafeFinancial') IS NOT NULL
	DROP PROCEDURE SaveCreditSafeFinancial
GO

IF TYPE_ID('CreditSafeFinancialList') IS NOT NULL
	DROP TYPE CreditSafeFinancialList
GO

CREATE TYPE CreditSafeFinancialList AS TABLE (
	CreditSafeBaseDataID BIGINT NULL,
	DateFrom DATETIME NULL,
	DateTo DATETIME NULL,
	PeriodMonths INT NULL,
	Currency NVARCHAR(10) NULL,
	ConsolidatedAccounts BIT NULL,
	Turnover INT NULL,
	Export INT NULL,
	CostOfSales INT NULL,
	GrossProfit INT NULL,
	WagesSalaries INT NULL,
	DirectorEmoluments INT NULL,
	OperatingProfits INT NULL,
	Depreciation INT NULL,
	AuditFees INT NULL,
	InterestPayments INT NULL,
	Pretax INT NULL,
	Taxation INT NULL,
	PostTax INT NULL,
	DividendsPayable INT NULL,
	RetainedProfits INT NULL,
	TangibleAssets INT NULL,
	IntangibleAssets INT NULL,
	FixedAssets INT NULL,
	CurrentAssets INT NULL,
	TradeDebtors INT NULL,
	Stock INT NULL,
	Cash INT NULL,
	OtherCurrentAssets INT NULL,
	IncreaseInCash INT NULL,
	MiscellaneousCurrentAssets INT NULL,
	TotalAssets INT NULL,
	TotalCurrentLiabilities INT NULL,
	TradeCreditors INT NULL,
	OverDraft INT NULL,
	OtherShortTermFinance INT NULL,
	MiscellaneousCurrentLiabilities INT NULL,
	OtherLongTermFinance INT NULL,
	LongTermLiabilities INT NULL,
	OverDrafeLongTermLiabilities INT NULL,
	Liabilities INT NULL,
	NetAssets INT NULL,
	WorkingCapital INT NULL,
	PaidUpEquity INT NULL,
	ProfitLossReserve INT NULL,
	SundryReserves INT NULL,
	RevalutationReserve INT NULL,
	Reserves INT NULL,
	ShareholderFunds INT NULL,
	Networth INT NULL,
	NetCashFlowFromOperations INT NULL,
	NetCashFlowBeforeFinancing INT NULL,
	NetCashFlowFromFinancing INT NULL,
	ContingentLiability BIT NULL,
	CapitalEmployed INT NULL,
	Employees INT NULL,
	Auditors NVARCHAR(100) NULL,
	AuditQualification NVARCHAR(100) NULL,
	Bankers NVARCHAR(100) NULL,
	BankBranchCode NVARCHAR(100) NULL,
	PreTaxMargin NVARCHAR(10) NULL,
	CurrentRatio NVARCHAR(10) NULL,
	NetworkingCapital NVARCHAR(10) NULL,
	GearingRatio NVARCHAR(10) NULL,
	Equity NVARCHAR(10) NULL,
	CreditorDays NVARCHAR(10) NULL,
	DebtorDays NVARCHAR(10) NULL,
	Liquidity NVARCHAR(10) NULL,
	ReturnOnCapitalEmployed NVARCHAR(10) NULL,
	ReturnOnAssetsEmployed NVARCHAR(10) NULL,
	CurrentDebtRatio NVARCHAR(10) NULL,
	TotalDebtRatio NVARCHAR(10) NULL,
	StockTurnoverRatio NVARCHAR(10) NULL,
	ReturnOnNetAssetsEmployed NVARCHAR(10) NULL
)
GO

CREATE PROCEDURE SaveCreditSafeFinancial
@Tbl CreditSafeFinancialList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CreditSafeFinancial (
		CreditSafeBaseDataID,
		DateFrom,
		DateTo,
		PeriodMonths,
		Currency,
		ConsolidatedAccounts,
		Turnover,
		Export,
		CostOfSales,
		GrossProfit,
		WagesSalaries,
		DirectorEmoluments,
		OperatingProfits,
		Depreciation,
		AuditFees,
		InterestPayments,
		Pretax,
		Taxation,
		PostTax,
		DividendsPayable,
		RetainedProfits,
		TangibleAssets,
		IntangibleAssets,
		FixedAssets,
		CurrentAssets,
		TradeDebtors,
		Stock,
		Cash,
		OtherCurrentAssets,
		IncreaseInCash,
		MiscellaneousCurrentAssets,
		TotalAssets,
		TotalCurrentLiabilities,
		TradeCreditors,
		OverDraft,
		OtherShortTermFinance,
		MiscellaneousCurrentLiabilities,
		OtherLongTermFinance,
		LongTermLiabilities,
		OverDrafeLongTermLiabilities,
		Liabilities,
		NetAssets,
		WorkingCapital,
		PaidUpEquity,
		ProfitLossReserve,
		SundryReserves,
		RevalutationReserve,
		Reserves,
		ShareholderFunds,
		Networth,
		NetCashFlowFromOperations,
		NetCashFlowBeforeFinancing,
		NetCashFlowFromFinancing,
		ContingentLiability,
		CapitalEmployed,
		Employees,
		Auditors,
		AuditQualification,
		Bankers,
		BankBranchCode,
		PreTaxMargin,
		CurrentRatio,
		NetworkingCapital,
		GearingRatio,
		Equity,
		CreditorDays,
		DebtorDays,
		Liquidity,
		ReturnOnCapitalEmployed,
		ReturnOnAssetsEmployed,
		CurrentDebtRatio,
		TotalDebtRatio,
		StockTurnoverRatio,
		ReturnOnNetAssetsEmployed
	) SELECT
		CreditSafeBaseDataID,
		DateFrom,
		DateTo,
		PeriodMonths,
		Currency,
		ConsolidatedAccounts,
		Turnover,
		Export,
		CostOfSales,
		GrossProfit,
		WagesSalaries,
		DirectorEmoluments,
		OperatingProfits,
		Depreciation,
		AuditFees,
		InterestPayments,
		Pretax,
		Taxation,
		PostTax,
		DividendsPayable,
		RetainedProfits,
		TangibleAssets,
		IntangibleAssets,
		FixedAssets,
		CurrentAssets,
		TradeDebtors,
		Stock,
		Cash,
		OtherCurrentAssets,
		IncreaseInCash,
		MiscellaneousCurrentAssets,
		TotalAssets,
		TotalCurrentLiabilities,
		TradeCreditors,
		OverDraft,
		OtherShortTermFinance,
		MiscellaneousCurrentLiabilities,
		OtherLongTermFinance,
		LongTermLiabilities,
		OverDrafeLongTermLiabilities,
		Liabilities,
		NetAssets,
		WorkingCapital,
		PaidUpEquity,
		ProfitLossReserve,
		SundryReserves,
		RevalutationReserve,
		Reserves,
		ShareholderFunds,
		Networth,
		NetCashFlowFromOperations,
		NetCashFlowBeforeFinancing,
		NetCashFlowFromFinancing,
		ContingentLiability,
		CapitalEmployed,
		Employees,
		Auditors,
		AuditQualification,
		Bankers,
		BankBranchCode,
		PreTaxMargin,
		CurrentRatio,
		NetworkingCapital,
		GearingRatio,
		Equity,
		CreditorDays,
		DebtorDays,
		Liquidity,
		ReturnOnCapitalEmployed,
		ReturnOnAssetsEmployed,
		CurrentDebtRatio,
		TotalDebtRatio,
		StockTurnoverRatio,
		ReturnOnNetAssetsEmployed
	FROM @Tbl
END
GO


