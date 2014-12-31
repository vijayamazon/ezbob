IF OBJECT_ID('CustomerAnalyticsUpdateCompany') IS NULL
	EXECUTE('CREATE PROCEDURE CustomerAnalyticsUpdateCompany AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE CustomerAnalyticsUpdateCompany
@CustomerID BIGINT,
@Score INT,
@MaxScore INT,
@SuggestedAmount DECIMAL(18, 6),
@IncorporationDate DATETIME,
@TangibleEquity DECIMAL(18, 6),
@AdjustedProfit DECIMAL(18, 6),
@Sic1980Code1 NVARCHAR(4),
@Sic1980Desc1 NVARCHAR(75),
@Sic1992Code1 NVARCHAR(4),
@Sic1992Desc1 NVARCHAR(75),
@AgeOfMostRecentCcj INT,
@NumOfCcjsInLast24Months INT,
@SumOfCcjsInLast24Months INT,
@AnalyticsDate DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @CurrentBalanceSum INT = 0
	DECLARE @ServiceLogID BIGINT

	------------------------------------------------------------------------------

	SET @ServiceLogID = dbo.udfGetCustomerCompanyLogID(@CustomerID)

	------------------------------------------------------------------------------

	SELECT
		@CurrentBalanceSum = SUM(dl97.CurrentBalance)
	FROM
		ExperianLtdDL97 dl97
		INNER JOIN ExperianLtd ltd
			ON dl97.ExperianLtdID = ltd.ExperianLtdID
			AND ltd.ServiceLogID = @ServiceLogID
	WHERE
		dl97.AccountState = 'A'

	------------------------------------------------------------------------------

	IF @CurrentBalanceSum IS NULL
		SET @CurrentBalanceSum = 0

	------------------------------------------------------------------------------
	------------------------------------------------------------------------------
	------------------------------------------------------------------------------

	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	BEGIN TRANSACTION

	------------------------------------------------------------------------------

	EXECUTE ClearCustomerAnalyticsCompany @CustomerID

	------------------------------------------------------------------------------

	INSERT INTO CustomerAnalyticsCompany (
		CustomerID, AnalyticsDate, IsActive, Score,MaxScore, SuggestedAmount,
		IncorporationDate, CurrentBalanceSum, TangibleEquity, AdjustedProfit,
		Sic1980Code1, Sic1980Desc1, Sic1992Code1, Sic1992Desc1,
		AgeOfMostRecentCcj, NumOfCcjsInLast24Months, SumOfCcjsInLast24Months
	) VALUES (
		@CustomerID, @AnalyticsDate, 1, @Score,@MaxScore, @SuggestedAmount,
		@IncorporationDate, @CurrentBalanceSum, @TangibleEquity, @AdjustedProfit,
		@Sic1980Code1, @Sic1980Desc1, @Sic1992Code1, @Sic1992Desc1,
		@AgeOfMostRecentCcj, @NumOfCcjsInLast24Months, @SumOfCcjsInLast24Months
	)

	------------------------------------------------------------------------------

	COMMIT TRANSACTION
END
GO
