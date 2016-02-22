IF OBJECT_ID('GetOfferConsumerBusinessDefaultRates') IS NULL
	EXECUTE('CREATE PROCEDURE GetOfferConsumerBusinessDefaultRates AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetOfferConsumerBusinessDefaultRates
@CustomerId INT
AS
BEGIN
	DECLARE @ConsumerScore INT = ISNULL((
		SELECT
			MinScore
		FROM
			dbo.udfGetCustomerScoreAnalytics(@CustomerId, NULL)
	), 0)

	------------------------------------------------------------------------------

	DECLARE @BusinessScore INT = ISNULL((
		SELECT
			Score
		FROM
			dbo.udfGetCustomerCompanyAnalytics(@CustomerId, NULL, 0, 0, 0)
	), 0)

	------------------------------------------------------------------------------

	DECLARE @ConsumerDefaultRate DECIMAL(18, 6) = (SELECT Value FROM DefaultRateCustomer d WHERE @ConsumerScore >= d.Start AND @ConsumerScore <= d.[End])

	------------------------------------------------------------------------------

	DECLARE @BusinessDefaultRate DECIMAL(18, 6) = (SELECT Value FROM DefaultRateCompany  d WHERE @BusinessScore >= d.Start AND @BusinessScore <= d.[End])

	------------------------------------------------------------------------------

	DECLARE @GradeID INT
	DECLARE @GradeScore DECIMAL(18, 3)
	DECLARE @ProbabilityOfDefault DECIMAL(18, 6)

	------------------------------------------------------------------------------

	SELECT
		@GradeID = h.GradeID,
		@GradeScore = CONVERT(DECIMAL(18, 3), h.Score)
	FROM
		CustomerLogicalGlueHistory h
		INNER JOIN Customer c
			ON h.CustomerID = c.Id
			AND h.CompanyID = c.CompanyId
			AND h.IsActive = 1
			AND c.Id = @CustomerID

	------------------------------------------------------------------------------

	SELECT
		@ProbabilityOfDefault = DefaultRate
	FROM
		I_SubGrade
	WHERE
		MinScore <= @GradeScore AND @GradeScore <= MaxScore

	------------------------------------------------------------------------------

	SELECT
		@ConsumerScore AS ConsumerScore,
		@BusinessScore AS BusinessScore,
		@ConsumerDefaultRate AS ConsumerDefaultRate,
		@BusinessDefaultRate AS BusinessDefaultRate,
		@GradeID AS GradeID,
		@GradeScore AS GradeScore,
		@ProbabilityOfDefault AS ProbabilityOfDefault
END
GO
