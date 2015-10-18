SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.udfGetCustomerCompanyAnalytics') IS NOT NULL
	DROP FUNCTION dbo.udfGetCustomerCompanyAnalytics
GO

-------------------------------------------------------------------------------
--
-- Output table MUST be in the same format in all these functions:
-- udfGetLimitedAnalytics
-- udfGetNonLimitedAnalytics
-- udfGetCustomerCompanyAnalytics
--
-------------------------------------------------------------------------------

CREATE FUNCTION dbo.udfGetCustomerCompanyAnalytics(
	@CustomerID INT,
	@Now DATETIME,
	@LoadCurrentBalanceSum BIT,
	@LoadTangibleEquityAndAdjustedProfit BIT,
	@LoadMaxScore BIT
)
RETURNS @output TABLE (
	ServiceLogID BIGINT NULL,
	ParsedRootEntryID BIGINT NULL,
	Score INT NULL,
	MaxScore INT NULL,
	SuggestedAmount DECIMAL(18, 6) NULL,
	IncorporationDate DATETIME NULL,
	CurrentBalanceSum INT NULL,
	TangibleEquity DECIMAL(18, 6) NULL,
	AdjustedProfit DECIMAL(18, 6) NULL,
	Sic1980Code1 NVARCHAR(4) NULL,
	Sic1980Desc1 NVARCHAR(75) NULL,
	Sic1992Code1 NVARCHAR(4) NULL,
	Sic1992Desc1 NVARCHAR(75) NULL,
	AgeOfMostRecentCcj INT NULL,
	NumOfCcjsInLast24Months INT NULL,
	SumOfCcjsInLast24Months INT NULL
)
AS
BEGIN
	DECLARE @CompanyRefNum NVARCHAR(50)
	DECLARE @ServiceLogID BIGINT = NULL
	DECLARE @TypeOfBusiness NVARCHAR(50) = NULL

	------------------------------------------------------------------------------

	SELECT
		@ServiceLogID = ServiceLogID,
		@TypeOfBusiness = TypeOfBusiness
	FROM
		dbo.udfGetCustomerHistoricalCompanyLogID(@CustomerID, @Now)

	------------------------------------------------------------------------------

	IF @ServiceLogID IS NOT NULL
	BEGIN
		IF @TypeOfBusiness IN ('Limited', 'LLP')
		BEGIN
			INSERT INTO @output (
				ServiceLogID, ParsedRootEntryID,
				Score, MaxScore, SuggestedAmount, IncorporationDate,
				Sic1980Code1, Sic1980Desc1, Sic1992Code1, Sic1992Desc1,
				AgeOfMostRecentCcj, NumOfCcjsInLast24Months, SumOfCcjsInLast24Months,
				CurrentBalanceSum, TangibleEquity, AdjustedProfit
			)
			SELECT
				ServiceLogID, ParsedRootEntryID,
				Score, MaxScore, SuggestedAmount, IncorporationDate,
				Sic1980Code1, Sic1980Desc1, Sic1992Code1, Sic1992Desc1,
				AgeOfMostRecentCcj, NumOfCcjsInLast24Months, SumOfCcjsInLast24Months,
				CurrentBalanceSum, TangibleEquity, AdjustedProfit
			FROM
				dbo.udfGetLimitedAnalytics(@ServiceLogID, @Now, @LoadCurrentBalanceSum, @LoadTangibleEquityAndAdjustedProfit, @LoadMaxScore)
		END
		ELSE BEGIN
			INSERT INTO @output (
				ServiceLogID, ParsedRootEntryID,
				Score, MaxScore, SuggestedAmount, IncorporationDate,
				Sic1980Code1, Sic1980Desc1, Sic1992Code1, Sic1992Desc1,
				AgeOfMostRecentCcj, NumOfCcjsInLast24Months, SumOfCcjsInLast24Months,
				CurrentBalanceSum, TangibleEquity, AdjustedProfit
			)
			SELECT
				ServiceLogID, ParsedRootEntryID,
				Score, MaxScore, SuggestedAmount, IncorporationDate,
				Sic1980Code1, Sic1980Desc1, Sic1992Code1, Sic1992Desc1,
				AgeOfMostRecentCcj, NumOfCcjsInLast24Months, SumOfCcjsInLast24Months,
				CurrentBalanceSum, TangibleEquity, AdjustedProfit
			FROM
				dbo.udfGetNonLimitedAnalytics(@ServiceLogID)
		END
	END

	------------------------------------------------------------------------------

	RETURN
END
GO
