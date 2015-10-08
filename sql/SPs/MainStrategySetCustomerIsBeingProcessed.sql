IF OBJECT_ID('MainStrategySetCustomerIsBeingProcessed') IS NULL
	EXECUTE('CREATE PROCEDURE MainStrategySetCustomerIsBeingProcessed AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE MainStrategySetCustomerIsBeingProcessed
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Customer SET
		CreditResult               = NULL,
		CreditSum                  = NULL,
		Status                     = NULL,
		ApplyForLoan               = NULL,
		ValidFor                   = NULL,
		SystemDecision             = NULL,
		MedalType                  = NULL,
		SystemCalculatedSum        = 0,
		ManagerApprovedSum         = 0,
		DateRejected               = NULL,
		RejectedReason             = NULL,
		DateApproved               = NULL,
		ApprovedReason             = NULL,
		IsLoanTypeSelectionAllowed = NULL
	WHERE
		Id = @CustomerID
END
GO
