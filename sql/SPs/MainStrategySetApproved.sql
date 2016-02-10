SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

IF OBJECT_ID('MainStrategySetApproved') IS NULL
	EXECUTE('CREATE PROCEDURE MainStrategySetApproved AS SELECT 1')
GO

ALTER PROCEDURE MainStrategySetApproved
@CustomerID INT,
@CashRequestID BIGINT,
@OverrideApprovedRejected BIT,
@CustomerStatus NVARCHAR(250),
@SystemDecision NVARCHAR(50),
@CreditResult NVARCHAR(100)
AS
BEGIN
	UPDATE Customer SET
		CreditResult = CASE WHEN @OverrideApprovedRejected = 1 THEN @CreditResult ELSE CreditResult END,
		Status = CASE WHEN @OverrideApprovedRejected = 1 THEN @CustomerStatus ELSE Status END,
		SystemDecision = @SystemDecision,
		LastStatus = ISNULL(@CreditResult, 'N/A')
	WHERE
		Id = @CustomerID

	UPDATE CashRequests SET
		SystemDecision = @SystemDecision,
		UnderwriterDecision = @CreditResult
	WHERE
		Id = @CashRequestID
END
GO
