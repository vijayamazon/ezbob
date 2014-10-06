IF OBJECT_ID ('dbo.udfCanQuickOffer') IS NOT NULL
	DROP FUNCTION dbo.udfCanQuickOffer
GO

CREATE FUNCTION dbo.udfCanQuickOffer(@Now DATETIME)
RETURNS @out TABLE (
	Enabled INT NOT NULL,
	FundsAvailable DECIMAL(18, 4) NULL,
	LoanCount INT NULL,
	IssuedAmount DECIMAL(18, 4) NULL,
	OpenCashRequests DECIMAL(18, 4) NULL,
	ErrorMsg NVARCHAR(255) NOT NULL,
	AmlMin INT,
	ConsumerScoreMin INT,
	ApplicantMinAgeYears INT
)
AS
BEGIN
	DECLARE @PacnetBalance DECIMAL(18, 4)
	DECLARE @ManualBalance DECIMAL(18, 4)
	DECLARE @FundsAvailable DECIMAL(18, 4)
	DECLARE @LoanCount INT
	DECLARE @IssuedAmount DECIMAL(18, 4)
	DECLARE @OpenCashRequests DECIMAL(18, 4)
	DECLARE @Enabled INT
	DECLARE @AmlMin INT
	DECLARE @ConsumerScoreMin INT
	DECLARE @ApplicantMinAgeYears INT

	DECLARE @bContinue BIT = 1

	IF @bContinue = 1
	BEGIN
		SELECT
			@Enabled = Enabled,
			@AmlMin = AmlMin,
			@ConsumerScoreMin = PersonalScoreMin,
			@ApplicantMinAgeYears = ApplicantMinAgeYears
		FROM
			QuickOfferConfiguration
		WHERE
			ID = 1
	
		IF @Enabled IS NULL OR @Enabled = 0
		BEGIN
			INSERT INTO @out (Enabled, ErrorMsg) VALUES (0, CASE WHEN @Enabled IS NULL THEN 'Quick offer configuration not found.' ELSE 'Quick offer is disabled.' END)
			SET @bContinue = 0
		END
		ELSE BEGIN
			INSERT INTO @out (Enabled, ErrorMsg, AmlMin, ConsumerScoreMin, ApplicantMinAgeYears) VALUES (@Enabled, '', @AmlMin, @ConsumerScoreMin, @ApplicantMinAgeYears)
			SET @bContinue = 1
		END
	END

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
				AND CONVERT(DATE, l.Date) = CONVERT(DATE, @Now)

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
			CONVERT(DATE, Date) = CONVERT(DATE, @Now)

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
			r.OfferStart <= @Now AND @Now <= r.OfferValidUntil

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
