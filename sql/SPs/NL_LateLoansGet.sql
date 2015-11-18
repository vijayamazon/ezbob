IF OBJECT_ID('NL_GetLoansToCollect') IS  NULL
	EXECUTE('CREATE PROCEDURE NL_GetLoansToCollect AS SELECT 1')
GO


ALTER PROCEDURE [dbo].[NL_GetLoansToCollect]
@Now DATETIME
AS
BEGIN
	DECLARE @UtcNow DATE
	SELECT 
		ls.LoanScheduleID, 
		l.LoanId,
		v.CustomerId, 
		lst.LoanStatus LoanStatus,
		lss.LoanScheduleStatus ScheduleStatus, 
		ls.PlannedDate ScheduleDate,
		ls.InterestRate
	FROM 
		NL_LoanSchedules ls
		INNER JOIN NL_LoanHistory lh ON 
			lh.LoanHistoryID = ls.LoanHistoryID
		INNER JOIN nl_loans l
			on lh.LoanID = l.LoanID
		INNER JOIN vw_NL_LoansCustomer v
			on v.loanID = l.LoanID
		INNER JOIN NL_LoanScheduleStatuses lss
			on lss.LoanScheduleStatusID = ls.LoanScheduleStatusID
		INNER JOIN NL_LoanStatuses lst
			on lst.LoanStatusID = l.LoanStatusID
	WHERE 
		(lss.LoanScheduleStatus = 'StillToPay' OR lss.LoanScheduleStatus = 'Late')
		AND
		ls.PlannedDate < @Now
END