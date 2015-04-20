SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('SaveCallCreditDataTpdDecisionCreditScores') IS NOT NULL
	DROP PROCEDURE SaveCallCreditDataTpdDecisionCreditScores
GO

IF TYPE_ID('CallCreditDataTpdDecisionCreditScoresList') IS NOT NULL
	DROP TYPE CallCreditDataTpdDecisionCreditScoresList
GO

CREATE TYPE CallCreditDataTpdDecisionCreditScoresList AS TABLE (
	[CallCreditDataTpdID] BIGINT NULL,
	[score] INT NULL,
	[ScoreClass] INT NULL,
	[Reason1] INT NULL,
	[Reason2] INT NULL,
	[Reason3] INT NULL,
	[Reason4] INT NULL
)
GO

CREATE PROCEDURE SaveCallCreditDataTpdDecisionCreditScores
@Tbl CallCreditDataTpdDecisionCreditScoresList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @c INT

	SELECT @c = COUNT(*) FROM @Tbl

	IF @c = 0
		RAISERROR('Invalid argument: no/too much data to insert into SaveCallCreditDataTpdDecisionCreditScores table.', 11, 1)

	INSERT INTO CallCreditDataTpdDecisionCreditScores (
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


