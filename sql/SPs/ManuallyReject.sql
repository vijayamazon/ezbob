IF OBJECT_ID('ManuallyReject') IS NULL
	EXECUTE('CREATE PROCEDURE ManuallyReject AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE ManuallyReject
@CustomerID INT,
@CreditResult NVARCHAR(MAX),
@Status NVARCHAR(250),
@UnderwriterName VARCHAR(200),
@DateRejected DATETIME,
@RejectedReason NVARCHAR(1000),
@NumRejects INT,
@CashRequestID BIGINT,
@UnderwriterID INT,
@UnderwriterDecisionDate DATETIME,
@UnderwriterDecision NVARCHAR(50),
@UnderwriterComment NVARCHAR(400),
@RejectionReasons IntList READONLY
AS
BEGIN
	BEGIN TRANSACTION

	UPDATE Customer SET
		CreditResult = @CreditResult,
		Status = @Status,
		UnderwriterName = @UnderwriterName,
		DateRejected = @DateRejected,
		RejectedReason = @RejectedReason,
		NumRejects = @NumRejects
	WHERE
		Id = @CustomerID

	UPDATE CashRequests SET
		IdUnderwriter = @UnderwriterID,
		UnderwriterDecisionDate = @UnderwriterDecisionDate,
		UnderwriterDecision = @UnderwriterDecision,
		UnderwriterComment = @UnderwriterComment,
		ManagerApprovedSum = NULL
	WHERE
		Id = @CashRequestID

	INSERT INTO DecisionHistory (
		[Date],
		[Action],
		UnderwriterId,
		CustomerId,
		Comment,
		CashRequestId,
		LoanTypeId
	) SELECT
		@UnderwriterDecisionDate,
		'Reject',
		@UnderwriterID,
		@CustomerID,
		@RejectedReason,
		@CashRequestID,
		LoanTypeID
	FROM
		CashRequests
	WHERE
		Id = @CashRequestID

	DECLARE @dhid INT = SCOPE_IDENTITY()

	INSERT INTO DecisionHistoryRejectReason (RejectReasonId, DecisionHistoryId)
	SELECT
		Value,
		@dhid
	FROM
		@RejectionReasons		

	COMMIT TRANSACTION
END
GO
