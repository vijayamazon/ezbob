IF OBJECT_ID('GetOfferConsumerBusinessDefaultRates') IS NULL
	EXECUTE('CREATE PROCEDURE GetOfferConsumerBusinessDefaultRates AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetOfferConsumerBusinessDefaultRates
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ConsumerScore INT = ISNULL((
		SELECT
			MinScore
		FROM
			dbo.udfGetCustomerScoreAnalytics(@CustomerId, NULL)
	), 0)

	DECLARE @BusinessScore INT = ISNULL((
		SELECT
			Score
		FROM
			dbo.udfGetCustomerCompanyAnalytics(@CustomerId, NULL, 0, 0, 0)
	), 0)

	DECLARE @ConsumerDefaultRate DECIMAL(18, 6) = (SELECT Value FROM DefaultRateCustomer d WHERE @ConsumerScore >= d.Start AND @ConsumerScore <= d.[End])

	DECLARE @BusinessDefaultRate DECIMAL(18, 6) = (SELECT Value FROM DefaultRateCompany  d WHERE @BusinessScore >= d.Start AND @BusinessScore <= d.[End])

	SELECT
		@ConsumerScore AS ConsumerScore,
		@BusinessScore AS BusinessScore,
		@ConsumerDefaultRate AS ConsumerDefaultRate,
		@BusinessDefaultRate AS BusinessDefaultRate
END
GO
