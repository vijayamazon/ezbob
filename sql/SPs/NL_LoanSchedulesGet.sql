SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_LoanSchedulesGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanSchedulesGet AS SELECT 1')
GO

ALTER PROCEDURE NL_LoanSchedulesGet
@LoanID BIGINT
--,@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		s.LoanScheduleID,
		s.LoanHistoryID,
		s.LoanScheduleStatusID,
		s.Position,
		s.PlannedDate,
		s.ClosedTime,
		s.Principal,
		s.InterestRate,
		s.[TwoDaysDueMailSent], s.[FiveDaysDueMailSent]
	FROM
		NL_LoanSchedules s INNER JOIN NL_LoanHistory h ON s.LoanHistoryID = h.LoanHistoryID
	WHERE
		h.LoanID = @LoanID --AND (@Now IS NULL OR h.EventTime < @Now)
END
GO
