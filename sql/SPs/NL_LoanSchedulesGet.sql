IF OBJECT_ID('NL_LoanSchedulesGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_LoanSchedulesGet AS SELECT 1')
GO

ALTER PROCEDURE NL_LoanSchedulesGet
@loanID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		ls.[LoanScheduleID]
      ,ls.[LoanHistoryID]
      ,ls.[LoanScheduleStatusID]
      ,ls.[Position]
      ,ls.[PlannedDate]
      ,ls.[ClosedTime]
      ,ls.[Principal]
      ,ls.[InterestRate]
	FROM
		[ezbob].[dbo].[NL_LoanHistory] lh
	LEFT JOIN
		[ezbob].[dbo].[NL_LoanShedule] ls
	ON
		ls.[LoanHistoryID]=lh.[LoanHistoryID]
	WHERE
		lh.[LoanID]=@loanID
END

GO


