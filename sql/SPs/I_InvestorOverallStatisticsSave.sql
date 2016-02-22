SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_InvestorOverallStatisticsSave') IS NOT NULL
	DROP PROCEDURE I_InvestorOverallStatisticsSave
GO

IF TYPE_ID('I_InvestorOverallStatisticsList') IS NOT NULL
	DROP TYPE I_InvestorOverallStatisticsList
GO

CREATE TYPE I_InvestorOverallStatisticsList AS TABLE (
	[InvestorID] INT NOT NULL,
	[InvestorAccountTypeID] INT NOT NULL,
	[TotalYield] DECIMAL(18, 6) NULL,
	[TotalAccumulatedRepayments] DECIMAL(18, 6) NULL,
	[Defaults] DECIMAL(18, 6) NULL,
	[Recoveries] DECIMAL(18, 6) NULL,
	[Timestamp] DATETIME NOT NULL
)
GO

CREATE PROCEDURE I_InvestorOverallStatisticsSave
@Tbl I_InvestorOverallStatisticsList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO I_InvestorOverallStatistics (
		[InvestorID],
		[InvestorAccountTypeID],
		[TotalYield],
		[TotalAccumulatedRepayments],
		[Defaults],
		[Recoveries],
		[Timestamp]
	) SELECT
		[InvestorID],
		[InvestorAccountTypeID],
		[TotalYield],
		[TotalAccumulatedRepayments],
		[Defaults],
		[Recoveries],
		[Timestamp]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


