IF OBJECT_ID('UpdateAutoApproval') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateAutoApproval SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateAutoApproval
@CustomerId INT,
@AutoApproveAmount INT,
@AutoDecisionID INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE CashRequests SET
		SystemCalculatedSum = @AutoApproveAmount,
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
