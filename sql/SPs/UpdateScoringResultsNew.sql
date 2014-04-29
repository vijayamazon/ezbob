IF OBJECT_ID('UpdateScoringResultsNew') IS NULL
	EXECUTE('CREATE PROCEDURE UpdateScoringResultsNew AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE UpdateScoringResultsNew
@CustomerId INT,
@CreditResult VARCHAR(100),
@SystemDecision VARCHAR(50),
@Status VARCHAR(250),
@Medal NVARCHAR(50),
@ValidFor DATETIME,
@Now DATETIME,
@OverrideApprovedRejected BIT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @ValidForHours INT

	------------------------------------------------------------------------------

	SELECT
		@ValidForHours = CONVERT(INT, Value)
	FROM
		ConfigurationVariables
	WHERE
		Name = 'OfferValidForHours'

	------------------------------------------------------------------------------

	IF @ValidFor IS NULL OR @ValidFor < DATEADD(hh, @ValidForHours, @Now)
		SET @ValidFor = DATEADD(hh, @ValidForHours, @Now)

	------------------------------------------------------------------------------

	UPDATE Customer SET
		CreditResult = CASE
			WHEN CreditResult IN ('Approved', 'Rejected') AND @OverrideApprovedRejected = 0
			THEN CreditResult
			ELSE @CreditResult
		END,
		Status = CASE
			WHEN Status IN ('Approved', 'Rejected') AND @OverrideApprovedRejected = 0
			THEN Status
			ELSE @Status
		END,
		SystemDecision = @SystemDecision,
		ApplyForLoan = @Now,
		ValidFor = @ValidFor,
		MedalType = @Medal
	WHERE
		Id = @CustomerId
END
GO
