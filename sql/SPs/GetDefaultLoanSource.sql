IF OBJECT_ID('GetDefaultLoanSource') IS NULL 
	EXECUTE('CREATE PROCEDURE GetDefaultLoanSource AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetDefaultLoanSource
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE
		@DefaultLoanTypeID INT,
		@LoanSourceID INT,
		@RepaymentPeriod INT,
		@IsCustomerRepaymentPeriodSelectionAllowed BIT

	SELECT TOP 1
		@LoanSourceID = LoanSourceID,
		@RepaymentPeriod = DefaultRepaymentPeriod,
		@IsCustomerRepaymentPeriodSelectionAllowed = IsCustomerRepaymentPeriodSelectionAllowed
	FROM
		LoanSource
	WHERE
		IsDefault = 1

	IF @LoanSourceID IS NULL
	BEGIN
		SELECT
			@DefaultLoanTypeID = DefaultLoanTypeID
		FROM
			dbo.udfGetLoanTypeAndDefault(NULL)

		SELECT TOP 1
			@LoanSourceID = LoanSourceID,
			@RepaymentPeriod = DefaultRepaymentPeriod,
			@IsCustomerRepaymentPeriodSelectionAllowed = IsCustomerRepaymentPeriodSelectionAllowed
		FROM
			LoanSource
		WHERE
			LoanSourceID = 1
	END

	IF @RepaymentPeriod IS NULL
		SELECT @RepaymentPeriod = RepaymentPeriod FROM LoanType WHERE Id = @DefaultLoanTypeID

	SELECT
		LoanSourceID = @LoanSourceID,
		RepaymentPeriod = @RepaymentPeriod,
		IsCustomerRepaymentPeriodSelectionAllowed = @IsCustomerRepaymentPeriodSelectionAllowed
END
GO
