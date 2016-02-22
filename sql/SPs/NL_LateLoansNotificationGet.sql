IF OBJECT_ID('NL_LateLoansNotificationGet') IS  NULL
	EXECUTE('CREATE PROCEDURE NL_LateLoansNotificationGet AS SELECT 1')
GO


ALTER PROCEDURE [dbo].[NL_LateLoansNotificationGet]
	@Now DATETIME
AS
BEGIN
	;WITH nlLateSchedules AS (
		SELECT min(s.PlannedDate) FirstLate, l.LoanID 
		FROM NL_LoanSchedules s inner join NL_LoanScheduleStatuses lss on lss.LoanScheduleStatusID = s.LoanScheduleStatusID
		inner join NL_LoanHistory h on h.LoanHistoryID = s.LoanHistoryID inner join NL_Loans l on l.LoanID = h.LoanID
		WHERE lss.LoanScheduleStatus IN ('Late', 'StillToPay') AND s.PlannedDate < @Now
		GROUP BY l.LoanID
	)
	SELECT 
		c.Id CustomerID,
		convert(DATE, s.PlannedDate) AS ScheduleDate, 		
		l.LoanID LoanID,
		l.OldLoanID OldLoanID,
		s.LoanHistoryID, 		 
		c.Id CustomerID,
		c.OriginID OriginID,
		c.Name AS email, 
		c.FirstName FirstName, 
		c.Fullname FullName,
		s.LoanScheduleID AS ScheduleID, 
		l.RefNum LoanRefNum, 
		c.DaytimePhone DaytimePhone,
		c.MobilePhone MobilePhone,		
		CAST(CASE WHEN o.EmailSendingAllowed IS NULL THEN 1 ELSE o.EmailSendingAllowed END AS BIT) EmailSendingAllowed,
		CAST(CASE WHEN o.MailSendingAllowed IS NULL THEN 1 ELSE o.MailSendingAllowed END AS BIT) MailSendingAllowed,
		CAST(CASE WHEN o.SmsSendingAllowed IS NULL THEN 1 ELSE o.SmsSendingAllowed END AS BIT) SmsSendingAllowed		
	FROM 
		NL_LoanSchedules s 			
		INNER JOIN NL_LoanHistory h on s.LoanHistoryID = h.LoanHistoryID	
		INNER JOIN NL_Loans l ON l.LoanID = h.LoanId
		INNER JOIN nlLateSchedules lls ON lls.LoanID = l.loanID AND s.PlannedDate=lls.FirstLate	
		INNER JOIN vw_NL_LoansCustomer v on v.loanID = l.LoanID	
		INNER JOIN Customer c ON v.CustomerId = c.Id 
		INNER JOIN CustomerStatuses cs ON cs.Id = c.CollectionStatus AND cs.IsEnabled = 1
		LEFT JOIN NL_LoanOptions o ON o.LoanId = l.LoanID
END

