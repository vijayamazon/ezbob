SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanHistorySave') IS NOT NULL
	DROP PROCEDURE NL_LoanHistorySave
GO

IF TYPE_ID('NL_LoanHistoryList') IS NOT NULL
	DROP TYPE NL_LoanHistoryList
GO

CREATE TYPE NL_LoanHistoryList AS TABLE (
	[EventTime] DATETIME NOT NULL,
	[Description] NVARCHAR(MAX) NULL,
	[LoanID] INT NULL,
	[UserID] INT NULL,
	[LoanLegalID] INT NULL
)
GO

CREATE PROCEDURE NL_LoanHistorySave
@Tbl NL_LoanHistoryList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanHistory (
		[EventTime],
		[Description],
		[LoanID],
		[UserID],
		[LoanLegalID]
	) SELECT
		[EventTime],
		[Description],
		[LoanID],
		[UserID],
		[LoanLegalID]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


