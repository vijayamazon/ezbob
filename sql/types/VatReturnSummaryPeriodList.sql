IF TYPE_ID('VatReturnSummaryPeriodList') IS NULL
BEGIN
	CREATE TYPE VatReturnSummaryPeriodList AS TABLE (
		DateFrom DATETIME NULL,
		DateTo DATETIME NULL,
		PctOfAnnualRevenues DECIMAL(18, 6) NULL,
		Revenues DECIMAL(18, 6) NULL,
		Opex DECIMAL(18, 6) NULL,
		TotalValueAdded DECIMAL(18, 6) NULL,
		PctOfRevenues DECIMAL(18, 6) NULL,
		Salaries DECIMAL(18, 6) NULL,
		Tax DECIMAL(18, 6) NULL,
		Ebida DECIMAL(18, 6) NULL,
		PctOfAnnual DECIMAL(18, 6) NULL,
		ActualLoanRepayment DECIMAL(18, 6) NULL,
		FreeCashFlow DECIMAL(18, 6) NULL,
		SalariesMultiplier DECIMAL(18, 6) NULL
	)
END
GO
