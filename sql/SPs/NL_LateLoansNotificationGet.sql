IF OBJECT_ID('NL_LateLoansNotificationGet') IS  NULL
	EXECUTE('CREATE PROCEDURE NL_LateLoansNotificationGet AS SELECT 1')
GO

ALTER PROCEDURE [dbo].[NL_LateLoansNotificationGet]
	@Now DATETIME
AS
BEGIN
	;WITH NL_LateSchedules AS (
		SELECT min(ls.PlannedDate) FirstLate, l.LoanID FROM NL_LoanSchedules ls
		inner join NL_LoanScheduleStatuses lss on lss.LoanScheduleStatusID = ls.LoanScheduleStatusID
		inner join NL_LoanHistory lh on lh.LoanHistoryID = ls.LoanHistoryID
		inner join nl_loans l on l.LoanID = lh.LoanID
		WHERE lss.LoanScheduleStatus IN ('Late', 'StillToPay') AND ls.PlannedDate < '11-12-2015'
		GROUP BY l.LoanID
	)
	SELECT 
		c.Id CustomerID,
		convert(DATE, ls.PlannedDate) AS ScheduleDate, 
		ls.Principal AmountDue, 
		l.LoanID LoanID,
		ls.LoanHistoryID, 
		ls.InterestRate Interest,  
		c.Id CustomerID,
		c.OriginID OriginID,
		c.Name AS email, 
		c.FirstName FirstName, 
		c.Fullname FullName,
		ls.LoanScheduleID AS ScheduleID, 
		l.RefNum LoanRefNum, 
		c.DaytimePhone DaytimePhone,
		c.MobilePhone MobilePhone,
		lf.Amount Fees,
		CAST(CASE WHEN o.EmailSendingAllowed IS NULL THEN 1 ELSE o.EmailSendingAllowed END AS BIT) EmailSendingAllowed,
		CAST(CASE WHEN o.MailSendingAllowed IS NULL THEN 1 ELSE o.MailSendingAllowed END AS BIT) MailSendingAllowed,
		CAST(CASE WHEN o.SmsSendingAllowed IS NULL THEN 1 ELSE o.SmsSendingAllowed END AS BIT) SmsSendingAllowed
		
	FROM 
		NL_LoanSchedules ls 			
		INNER JOIN NL_LoanHistory lh on ls.LoanHistoryID = lh.LoanHistoryID	
		INNER JOIN NL_Loans l ON l.LoanID = lh.LoanId
		INNER JOIN NL_LateSchedules lls ON lls.LoanID = l.loanid AND ls.PlannedDate=lls.FirstLate	
		INNER JOIN vw_NL_LoansCustomer v on v.loanID = l.LoanID
		INNER JOIN NL_LoanFees lf on lf.LoanID = l.LoanID
		INNER JOIN Customer c ON v.CustomerId = c.Id 
		INNER JOIN CustomerStatuses cs ON cs.Id = c.CollectionStatus AND cs.IsEnabled = 1
		LEFT JOIN NL_LoanOptions o ON o.LoanId = l.LoanID
END
