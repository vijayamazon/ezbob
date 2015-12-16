SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadLoanRepaymentsForHmrc') IS NULL
	EXECUTE('CREATE PROCEDURE LoadLoanRepaymentsForHmrc AS SELECT 1')
GO

ALTER PROCEDURE LoadLoanRepaymentsForHmrc
@CustomerID INT,
@Now DATETIME
AS
BEGIN
	DECLARE @LoanRepayment DECIMAL(18, 6) = 0

	DECLARE @CompanyID INT = dbo.udfGetCustomerCompanyID(@CustomerID, @Now)

	IF @CompanyID IS NULL
	BEGIN
		SELECT LoanRepayment = @LoanRepayment
		RETURN
	END

	DECLARE @ExperianRefNum NVARCHAR(50) = (SELECT ExperianRefNum FROM Company WHERE Id = @CompanyID)

	DECLARE @ServiceLogID BIGINT = (
		SELECT TOP 1
			Id
		FROM
			MP_ServiceLog
		WHERE
			CompanyRefNum = @ExperianRefNum
			AND
			ServiceType = 'E-SeriesLimitedData'
		ORDER BY
			InsertDate DESC,
			Id DESC
	)

	IF @ServiceLogID IS NULL
	BEGIN
		SELECT LoanRepayment = @LoanRepayment
		RETURN
	END

	DECLARE @ExperianLtdID BIGINT = (
		SELECT ExperianLtdID FROM ExperianLtd WHERE ServiceLogID = @ServiceLogID
	)

	IF @ExperianLtdID IS NULL
	BEGIN
		SELECT LoanRepayment = @LoanRepayment
		RETURN
	END

	SET @LoanRepayment = (
		SELECT
			 SUM(ISNULL(CurrentBalance, 0))
		FROM
			ExperianLtdDL97
		WHERE
			ExperianLtdID = @ExperianLtdID
			AND
			AccountState = 'A'
	)

	SELECT LoanRepayment = ISNULL(@LoanRepayment, 0)
END
GO
