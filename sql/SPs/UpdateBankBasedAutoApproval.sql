IF OBJECT_ID('UpdateBankBasedAutoApproval') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateBankBasedAutoApproval AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateBankBasedAutoApproval
@CustomerId INT,
@AutoApproveAmount INT,
@RepaymentPeriod INT,
@AutoDecisionID INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE 
		@Score INT,
		@InterestRate NUMERIC(18,4)

	SELECT @Score = ISNULL(ExperianConsumerScore,0) FROM Customer WHERE Id=@CustomerId

	SELECT @InterestRate = Value FROM BasicInterestRate WHERE Start <= @Score AND [End] >= @Score

	IF @InterestRate IS NULL
		SET @InterestRate=0

	UPDATE CashRequests SET
		SystemCalculatedSum = @AutoApproveAmount,
		InterestRate = @InterestRate,
		RepaymentPeriod = @RepaymentPeriod,
		AutoDecisionID = @AutoDecisionID
	WHERE
		IdCustomer = @CustomerId

	UPDATE Customer SET
		SystemCalculatedSum = @AutoApproveAmount,
		CreditSum = @AutoApproveAmount,
		IsLoanTypeSelectionAllowed = 1,
		LastStatus = 'Approve'
	WHERE
		Id = @CustomerId
END
GO
