SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataCreditScores') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataCreditScores
GO

IF TYPE_ID('CallCreditDataCreditScoresList') IS NOT NULL
	DROP TYPE CallCreditDataCreditScoresList
GO

CREATE TYPE CallCreditDataCreditScoresList AS TABLE (
	[CallCreditDataID] BIGINT NULL,
	[score] INT NULL,
	[ScoreClass] INT NULL,
	[Reason1] INT NULL,
	[Reason2] INT NULL,
	[Reason3] INT NULL,
	[Reason4] INT NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataCreditScores
@Tbl CallCreditDataCreditScoresList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataCreditScores (
		[CallCreditDataID],
		[score],
		[ScoreClass],
		[Reason1],
		[Reason2],
		[Reason3],
		[Reason4]
	) SELECT
		[CallCreditDataID],
		[score],
		[ScoreClass],
		[Reason1],
		[Reason2],
		[Reason3],
		[Reason4]
	FROM @Tbl
END
GO


