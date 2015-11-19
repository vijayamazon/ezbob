IF OBJECT_ID('ManuallyUnsuspend') IS NULL
	EXECUTE('CREATE PROCEDURE ManuallyUnsuspend AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE ManuallyUnsuspend
@CustomerID INT,
@CreditResult NVARCHAR(MAX),
@Status NVARCHAR(250),
@UnderwriterName VARCHAR(200),
@IsWaitingForSignature BIT,
@CashRequestID BIGINT,
@CashRequestRowVersion VARBINARY(8),
@UnderwriterID INT,
@UnderwriterDecisionDate DATETIME,
@UnderwriterDecision NVARCHAR(50),
@UnderwriterComment NVARCHAR(400)
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
		IsWaitingForSignature = CASE WHEN @IsWaitingForSignature IS NULL THEN IsWaitingForSignature ELSE @IsWaitingForSignature END
	WHERE
		Id = @CustomerID

	UPDATE CashRequests SET
		IdUnderwriter = @UnderwriterID,
		UnderwriterDecisionDate = @UnderwriterDecisionDate,
		UnderwriterDecision = @UnderwriterDecision,
		UnderwriterComment = @UnderwriterComment
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
		'Waiting',
		@UnderwriterID,
		@CustomerID,
		'',
		@CashRequestID,
		LoanTypeID
	FROM
		CashRequests
	WHERE
		Id = @CashRequestID

	COMMIT TRANSACTION

	SELECT Result = 'OK'
END
GO
