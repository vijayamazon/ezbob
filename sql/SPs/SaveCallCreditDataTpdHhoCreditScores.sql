SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataTpdHhoCreditScores') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataTpdHhoCreditScores
GO

IF TYPE_ID('CallCreditDataTpdHhoCreditScoresList') IS NOT NULL
	DROP TYPE CallCreditDataTpdHhoCreditScoresList
GO

CREATE TYPE CallCreditDataTpdHhoCreditScoresList AS TABLE (
	[CallCreditDataTpdID] BIGINT NULL,
	[score] INT NULL,
	[ScoreClass] INT NULL,
	[Reason1] INT NULL,
	[Reason2] INT NULL,
	[Reason3] INT NULL,
	[Reason4] INT NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataTpdHhoCreditScores
@Tbl CallCreditDataTpdHhoCreditScoresList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO CallCreditDataTpdHhoCreditScores (
		[CallCreditDataTpdID],
		[score],
		[ScoreClass],
		[Reason1],
		[Reason2],
		[Reason3],
		[Reason4]
	) SELECT
		[CallCreditDataTpdID],
		[score],
		[ScoreClass],
		[Reason1],
		[Reason2],
		[Reason3],
		[Reason4]
	FROM @Tbl
END
GO


