SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanSchedulesSave') IS NOT NULL
	DROP PROCEDURE NL_LoanSchedulesSave
GO

IF TYPE_ID('NL_LoanSchedulesList') IS NOT NULL
	DROP TYPE NL_LoanSchedulesList
GO

CREATE TYPE NL_LoanSchedulesList AS TABLE (
	[LoanHistoryID] BIGINT NOT NULL,
	[LoanScheduleStatusID] INT NOT NULL, 
	[Position] INT NOT NULL,
	[PlannedDate] DATETIME NOT NULL,
	[ClosedTime] DATETIME NULL,
	[Principal] DECIMAL(18, 6) NOT NULL,
	[InterestRate] DECIMAL(18, 6) NOT NULL,
	[TwoDaysDueMailSent] BIT NOT NULL,
	[FiveDaysDueMailSent]	BIT NOT NULL
)
GO

CREATE PROCEDURE NL_LoanSchedulesSave
@Tbl NL_LoanSchedulesList READONLY
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO NL_LoanSchedules (
		[LoanHistoryID],
		[LoanScheduleStatusID],
		[Position],
		[PlannedDate],
		[ClosedTime],
		[Principal],
		[InterestRate],
		[TwoDaysDueMailSent],
		[FiveDaysDueMailSent]			
	) SELECT
		[LoanHistoryID],
		[LoanScheduleStatusID],
		[Position],
		[PlannedDate],
		[ClosedTime],
		[Principal],
		[InterestRate],
		[TwoDaysDueMailSent],
		[FiveDaysDueMailSent]				
	FROM @Tbl

	DECLARE @ScopeID BIGINT = SCOPE_IDENTITY()
	SELECT @ScopeID AS ScopeID
END
GO


