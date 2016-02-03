IF OBJECT_ID('GetDefaultLoanSource') IS NULL 
	EXECUTE('CREATE PROCEDURE GetDefaultLoanSource AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetDefaultLoanSource
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @OriginID INT = (SELECT OriginID FROM Customer WHERE Id = @CustomerID)

	DECLARE
		@DefaultLoanTypeID INT,
		@LoanSourceID INT,
		@RepaymentPeriod INT,
		@IsCustomerRepaymentPeriodSelectionAllowed BIT

	SELECT
		@LoanSourceID = LoanSourceID,
		@RepaymentPeriod = DefaultRepaymentPeriod,
		@IsCustomerRepaymentPeriodSelectionAllowed = IsCustomerRepaymentPeriodSelectionAllowed
	FROM
		dbo.udfGetLoanSource(NULL, @OriginID)

	IF @RepaymentPeriod IS NULL
	BEGIN
		SELECT
			@DefaultLoanTypeID = DefaultLoanTypeID
		FROM
			dbo.udfGetLoanTypeAndDefault(NULL)

		SELECT @RepaymentPeriod = RepaymentPeriod FROM LoanType WHERE Id = @DefaultLoanTypeID
	END

	SELECT
		LoanSourceID = @LoanSourceID,
		RepaymentPeriod = @RepaymentPeriod,
		IsCustomerRepaymentPeriodSelectionAllowed = @IsCustomerRepaymentPeriodSelectionAllowed
END
GO
