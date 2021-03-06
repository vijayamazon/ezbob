IF OBJECT_ID('LoadRtiMonthForVatReturnSummary') IS NULL
	EXECUTE('CREATE PROCEDURE LoadRtiMonthForVatReturnSummary AS SELECT 1')
GO

ALTER PROCEDURE LoadRtiMonthForVatReturnSummary
@CustomerMarketplaceID INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @OneMonthSalary DECIMAL(18, 0)
	DECLARE @CustomerID INT

	------------------------------------------------------------------------------

	SELECT
		@CustomerID = CustomerID
	FROM
		MP_CustomerMarketPlace
	WHERE
		Id = @CustomerMarketplaceID

	------------------------------------------------------------------------------

	SELECT TOP 1
		@OneMonthSalary = cec.TotalMonthlySalary
	FROM
		CompanyEmployeeCount cec
	WHERE
		cec.CustomerID = @CustomerID
	ORDER BY
		cec.Created DESC

	------------------------------------------------------------------------------

	IF ISNULL(@OneMonthSalary, 0) != 0
	BEGIN
		SELECT
			@CustomerID AS CustomerID

		SELECT
			CONVERT(INT, -1) AS RecordID,
			CONVERT(DATETIME, NULL) AS DateStart,
			@OneMonthSalary AS AmountPaid,
			'GBP' AS CurrencyCode

		RETURN
	END

	------------------------------------------------------------------------------

	SELECT
		@CustomerID AS CustomerID

	SELECT
		e.RecordID,
		e.DateStart,
		e.AmountPaid,
		e.CurrencyCode
	FROM
		dbo.udfLoadRtiMonthRawData(@CustomerMarketplaceID) e
END
GO
