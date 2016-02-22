IF OBJECT_ID('GetLoanSource') IS NULL 
	EXECUTE('CREATE PROCEDURE GetLoanSource AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Returns loan source by id. If could not find loan source by id returns
-- default for the customer's origin. If none is marked as default returns loan source with
-- the minimal id existing in the DB.

ALTER PROCEDURE GetLoanSource
@LoanSourceID INT,
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @OriginID INT = (SELECT OriginID FROM Customer WHERE Id = @CustomerID)

	SELECT
		LoanSourceID,
		LoanSourceName,
		MaxInterest,
		DefaultRepaymentPeriod,
		IsCustomerRepaymentPeriodSelectionAllowed,
		MaxEmployeeCount,
		MaxAnnualTurnover,
		AlertOnCustomerReasonType --,
		--IsDisabled
	FROM
		dbo.udfGetLoanSource(@LoanSourceID, @OriginID)
END
GO
