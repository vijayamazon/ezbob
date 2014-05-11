IF OBJECT_ID('SaveVatReturnSummary') IS NULL
	EXECUTE('CREATE PROCEDURE SaveVatReturnSummary AS SELECT 1')
GO

ALTER PROCEDURE SaveVatReturnSummary
@CustomerID INT,
@CustomerMarketplaceID INT,
@CreationDate DATETIME,
@Totals VatReturnSummaryList READONLY,
@Quarters VatReturnSummaryPeriodList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @TotalCount INT
	DECLARE @SummaryID BIGINT
	
	SELECT
		@TotalCount = COUNT(*)
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
		ISActive = 1

	INSERT INTO MP_VatReturnSummary (
		CustomerID, BusinessID, CreationDate, IsActive, Currency,
		PctOfAnnualRevenues, Revenues, Opex, TotalValueAdded, PctOfRevenues,
		Salaries, Tax, Ebida, PctOfAnnual, ActualLoanRepayment, FreeCashFlow,
		SalariesMultiplier, CustomerMarketplaceID
	) SELECT
		@CustomerID, BusinessID, @CreationDate, 1, CurrencyCode,
		PctOfAnnualRevenues, Revenues, Opex, TotalValueAdded, PctOfRevenues,
		Salaries, Tax, Ebida, PctOfAnnual, ActualLoanRepayment, FreeCashFlow,
		SalariesMultiplier, @CustomerMarketplaceID
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
