IF OBJECT_ID('SaveVatReturnSummary') IS NOT NULL
	DROP PROCEDURE SaveVatReturnSummary
GO

IF TYPE_ID('VatReturnSummaryList') IS NOT NULL
	DROP TYPE VatReturnSummaryList
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TYPE VatReturnSummaryList AS TABLE (
	BusinessID INT NULL,
	CurrencyCode NCHAR(3) NULL,
	AnnualizedTurnover DECIMAL(18, 6) NULL,
	AnnualizedValueAdded DECIMAL(18, 6) NULL,
	AnnualizedFreeCashFlow DECIMAL(18, 6) NULL,
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
GO

CREATE PROCEDURE SaveVatReturnSummary
@CustomerID INT,
@CustomerMarketplaceID INT,
@CreationDate DATETIME,
@CalculationID UNIQUEIDENTIFIER,
@Totals VatReturnSummaryList READONLY,
@Quarters VatReturnSummaryPeriodList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @TotalCount INT
	DECLARE @BusinessID INT
	DECLARE @SummaryID BIGINT
	
	SELECT
		@TotalCount = COUNT(*),
		@BusinessID = MAX(BusinessID)
	FROM
		@Totals

	IF @TotalCount != 1
		RAISERROR('Number of totals should be exactly 1.', 11, 1)

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	BEGIN TRANSACTION

	UPDATE MP_VatReturnSummary SET
		IsActive = 0
	WHERE
		CustomerID = @CustomerID
		AND
		CustomerMarketplaceID = @CustomerMarketplaceID
		AND
		CalculationID != @CalculationID
		AND
		ISActive = 1

	INSERT INTO MP_VatReturnSummary (
		CustomerID, BusinessID, CreationDate, IsActive, Currency,
		PctOfAnnualRevenues, Revenues, Opex, TotalValueAdded, PctOfRevenues,
		Salaries, Tax, Ebida, PctOfAnnual, ActualLoanRepayment, FreeCashFlow,
		SalariesMultiplier, CustomerMarketplaceID, CalculationID,
		AnnualizedTurnover, AnnualizedValueAdded, AnnualizedFreeCashFlow
	) SELECT
		@CustomerID, BusinessID, @CreationDate, 1, CurrencyCode,
		PctOfAnnualRevenues, Revenues, Opex, TotalValueAdded, PctOfRevenues,
		Salaries, Tax, Ebida, PctOfAnnual, ActualLoanRepayment, FreeCashFlow,
		SalariesMultiplier, @CustomerMarketplaceID, @CalculationID,
		AnnualizedTurnover, AnnualizedValueAdded, AnnualizedFreeCashFlow
	FROM
		@Totals

	SET @SummaryID = SCOPE_IDENTITY()

	COMMIT TRANSACTION

	INSERT INTO MP_VatReturnSummaryPeriods (
		SummaryID, DateFrom, DateTo,
		PctOfAnnualRevenues, Revenues, Opex, TotalValueAdded, PctOfRevenues,
		Salaries, Tax, Ebida, PctOfAnnual, ActualLoanRepayment, FreeCashFlow
	) SELECT
		@SummaryID, DateFrom, DateTo,
		PctOfAnnualRevenues, Revenues, Opex, TotalValueAdded, PctOfRevenues,
		Salaries, Tax, Ebida, PctOfAnnual, ActualLoanRepayment, FreeCashFlow
	FROM
		@Quarters
END
GO
