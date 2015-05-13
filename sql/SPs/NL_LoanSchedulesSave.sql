SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanSchedulesSave') IS NOT NULL
	DROP PROCEDURE NL_LoanSchedulesSave
GO

IF TYPE_ID('NL_LoanSchedulesList') IS NOT NULL
	DROP TYPE NL_LoanSchedulesList
GO

CREATE TYPE NL_LoanSchedulesList AS TABLE (
	[LoanHistoryID] INT NOT NULL,
	[Position] INT NOT NULL,
	[PlannedDate] DATETIME NOT NULL,
	[ClosedTime] DATETIME NULL,
	[Principal] DECIMAL(18, 6) NOT NULL,
	[InterestRate] DECIMAL(18, 6) NOT NULL
)
GO

CREATE PROCEDURE NL_LoanSchedulesSave
@Tbl NL_LoanSchedulesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanSchedules (
		[LoanHistoryID],
		[Position],
		[PlannedDate],
		[ClosedTime],
		[Principal],
		[InterestRate]
	) SELECT
		[LoanHistoryID],
		[Position],
		[PlannedDate],
		[ClosedTime],
		[Principal],
		[InterestRate]
	FROM @Tbl

	DECLARE @ScopeID INT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


