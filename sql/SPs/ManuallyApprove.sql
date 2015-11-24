IF OBJECT_ID('ManuallyApprove') IS NULL
	EXECUTE('CREATE PROCEDURE ManuallyApprove AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE ManuallyApprove
@CustomerID INT,
@CreditResult NVARCHAR(MAX),
@Status NVARCHAR(250),
@UnderwriterName VARCHAR(200),
@IsWaitingForSignature BIT,
@DateApproved DATETIME,
@ApprovedReason NCHAR(200),
@CreditSum DECIMAL(18, 0),
@CustomerManagerApprovedSum DECIMAL(18, 4),
@NumApproves INT,
@IsLoanTypeSelectionAllowed INT,
@CashRequestID BIGINT,
@CashRequestRowVersion VARBINARY(8),
@UnderwriterID INT,
@UnderwriterDecisionDate DATETIME,
@UnderwriterDecision NVARCHAR(50),
@UnderwriterComment NVARCHAR(400),
@CashRequestManagerApprovedSum INT
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
		DateApproved = @DateApproved,
		ApprovedReason = @ApprovedReason,
		CreditSum = @CreditSum,
		ManagerApprovedSum = @CustomerManagerApprovedSum,
		NumApproves = @NumApproves,
		IsLoanTypeSelectionAllowed = @IsLoanTypeSelectionAllowed
	WHERE
		Id = @CustomerID

	UPDATE CashRequests SET
		IdUnderwriter = @UnderwriterID,
		UnderwriterDecisionDate = @UnderwriterDecisionDate,
		UnderwriterDecision = @UnderwriterDecision,
		UnderwriterComment = @UnderwriterComment,
		ManagerApprovedSum = @CashRequestManagerApprovedSum
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
		'Approve',
		@UnderwriterID,
		@CustomerID,
		@ApprovedReason,
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
