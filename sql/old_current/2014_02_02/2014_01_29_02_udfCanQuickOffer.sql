IF OBJECT_ID('dbo.udfCanQuickOffer') IS NOT NULL
	DROP FUNCTION dbo.udfCanQuickOffer
GO

CREATE FUNCTION dbo.udfCanQuickOffer()
RETURNS @out TABLE (
	FundsAvailable DECIMAL(18, 4) NULL,
	LoanCount INT NULL,
	IssuedAmount DECIMAL(18, 4) NULL,
	OpenCashRequests DECIMAL(18, 4) NULL,
	ErrorMsg NVARCHAR(255) NOT NULL
)
AS
BEGIN
	DECLARE @PacnetBalance DECIMAL(18, 4)
	DECLARE @ManualBalance DECIMAL(18, 4)
	DECLARE @FundsAvailable DECIMAL(18, 4)
	DECLARE @LoanCount INT
	DECLARE @IssuedAmount DECIMAL(18, 4)
	DECLARE @OpenCashRequests DECIMAL(18, 4)

	DECLARE @bContinue BIT = 1

	INSERT INTO @out (ErrorMsg) VALUES ('')

	IF @bContinue = 1
	BEGIN
		SELECT
			@LoanCount = COUNT(*),
			@IssuedAmount = SUM(l.LoanAmount)
		FROM
			Loan l
			INNER JOIN CashRequests r
				ON l.RequestCashId = r.Id
				AND r.QuickOfferID IS NOT NULL
				AND CONVERT(DATE, l.Date) = CONVERT(DATE, GETUTCDATE())

		SET @LoanCount = ISNULL(@LoanCount, 0)
		SET @IssuedAmount = ISNULL(@IssuedAmount, 0)

		UPDATE @out SET
			LoanCount = @LoanCount,
			IssuedAmount = @IssuedAmount

		IF EXISTS (SELECT * FROM QuickOfferConfiguration WHERE MaxLoanCountPerDay <= @LoanCount OR MaxIssuedValuePerDay <= @IssuedAmount)
		BEGIN
			UPDATE @out SET ErrorMsg = 'Max loan count per day/max issued value per day exceeded.'
			SET @bContinue = 0
		END
		ELSE
			SET @bContinue = 1
	END

	IF @bContinue = 1
	BEGIN
		SELECT TOP 1
			@PacnetBalance = Adjusted
		FROM
			vPacnetBalance
		ORDER BY
			Date DESC

		SELECT
			@ManualBalance = SUM(Amount)
		FROM
			PacNetManualBalance
		WHERE
			Enabled = 1
			AND
			CONVERT(DATE, Date) = CONVERT(DATE, GETUTCDATE())

		SET @FundsAvailable = ISNULL(@PacnetBalance, 0) + ISNULL(@ManualBalance, 0)

		UPDATE @out SET FundsAvailable = @FundsAvailable

		IF @FundsAvailable <= 0
		BEGIN
			UPDATE @out SET ErrorMsg = 'No funds available.'
			SET @bContinue = 0
		END
		ELSE
			SET @bContinue = 1
	END

	IF @bContinue = 1
	BEGIN
		SELECT
			@OpenCashRequests = ISNULL(SUM(ISNULL(
				CASE
				WHEN r.ManagerApprovedSum IS NULL
					THEN r.SystemCalculatedSum
				ELSE
					r.ManagerApprovedSum
				END,
			0)), 0)
			-
			ISNULL(SUM(l.LoanAmount), 0)
		FROM
			CashRequests r
			LEFT JOIN Loan l ON r.Id = l.RequestCashId
		WHERE
			(
				r.UnderwriterDecision = 'Approved'
				OR
				(r.UnderwriterDecision IS NULL AND r.SystemDecision = 'Approve' AND r.UnderwriterComment = 'Automatic Re-Approve')
			)
			AND
			r.OfferStart <= GETUTCDATE() AND GETUTCDATE() <= r.OfferValidUntil

		SET @OpenCashRequests = ISNULL(@OpenCashRequests, 0)

		UPDATE @out SET OpenCashRequests = @OpenCashRequests

		IF @FundsAvailable <= @OpenCashRequests
		BEGIN
			UPDATE @out SET ErrorMsg = 'Too many open cash requests.'
			SET @bContinue = 0
		END
		ELSE
			SET @bContinue = 1
	END

	RETURN
END
GO
