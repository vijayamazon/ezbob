IF OBJECT_ID('CustomerAnalyticsUpdateNonLimitedCompany') IS NULL
	EXECUTE('CREATE PROCEDURE CustomerAnalyticsUpdateNonLimitedCompany AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE CustomerAnalyticsUpdateNonLimitedCompany
@CustomerId INT,		
@RefNumber NVARCHAR(50),
@MaxScore INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE 
		@SuggestedAmount DECIMAL(18,6),
		@Score INT,
		@IncorporationDate DATETIME,
		@AgeOfMostRecentCcj INT,
		@NumOfCcjsInLast24Months INT,
		@SumOfCcjsInLast24Months INT

	SELECT
		@IncorporationDate = IncorporationDate,		
		@Score = RiskScore,
		@SuggestedAmount = CreditLimit,
		@AgeOfMostRecentCcj = AgeOfMostRecentJudgmentDuringOwnershipMonths,
		@NumOfCcjsInLast24Months = TotalJudgmentCountLast24Months + TotalAssociatedJudgmentCountLast24Months,
		@SumOfCcjsInLast24Months = TotalJudgmentValueLast24Months + TotalAssociatedJudgmentValueLast24Months
	FROM
		ExperianNonLimitedResults
	WHERE
		RefNumber = @RefNumber AND
		IsActive = 1

	IF (@Score IS NOT NULL AND @Score > @MaxScore)
		SET @MaxScore = @Score

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	BEGIN TRANSACTION

	EXECUTE ClearCustomerAnalyticsCompany @CustomerId

	INSERT INTO CustomerAnalyticsCompany (
		CustomerID, AnalyticsDate, IsActive, Score, SuggestedAmount, IncorporationDate,
		CurrentBalanceSum, TangibleEquity, AdjustedProfit,
		Sic1980Code1, Sic1980Desc1, Sic1992Code1, Sic1992Desc1,
		AgeOfMostRecentCcj, NumOfCcjsInLast24Months, SumOfCcjsInLast24Months, MaxScore
	) VALUES (
		@CustomerId, GETUTCDATE(), 1, @Score, @SuggestedAmount, @IncorporationDate,
		0, 0, 0,
		'', '', '', '',
		@AgeOfMostRecentCcj, @NumOfCcjsInLast24Months, @SumOfCcjsInLast24Months, @MaxScore
	)

	COMMIT TRANSACTION
END
GO
