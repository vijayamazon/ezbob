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
@IsWaitingForSignature BIT,
@DateRejected DATETIME,
@RejectedReason NVARCHAR(1000),
@NumRejects INT,
@CashRequestID BIGINT,
@CashRequestRowVersion VARBINARY(8),
@UnderwriterID INT,
@UnderwriterDecisionDate DATETIME,
@UnderwriterDecision NVARCHAR(50),
@UnderwriterComment NVARCHAR(400),
@RejectionReasons IntList READONLY
AS
BEGIN
	IF NOT EXISTS (SELECT * FROM CashRequests WHERE Id = @CashRequestID AND TimestampCounter = @CashRequestRowVersion)
	BEGIN
		SELECT Result = 'Please refresh your browser page, cash request was changed by someone else.'
		RETURN
	END

	BEGIN TRANSACTION

	UPDATE Customer SET
		CreditResult = @CreditResult,
		Status = @Status,
		UnderwriterName = @UnderwriterName,
		IsWaitingForSignature = CASE WHEN @IsWaitingForSignature IS NULL THEN IsWaitingForSignature ELSE @IsWaitingForSignature END,
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

	SELECT Result = 'OK'
END
GO
