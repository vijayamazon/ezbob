IF OBJECT_ID('GetLateForCollection') IS NULL
	EXECUTE('CREATE PROCEDURE GetLateForCollection AS SELECT 1')
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[GetLateForCollection]
	@Now DATETIME
AS
BEGIN
	;WITH LateSchedules AS (
		SELECT min(ls.[Date]) FirstLate, ls.LoanId FROM LoanSchedule ls
		WHERE ls.Status IN ('Late', 'StillToPay') AND CAST(ls.[Date] AS DATE)<@Now
		GROUP BY ls.LoanId
	)
	SELECT 
		c.Id CustomerID,
		convert(DATE, ls.Date) AS ScheduleDate, 
		ls.AmountDue AmountDue, 
		ls.LoanId LoanID, 
		ls.Interest Interest,  
		c.Id CustomerID,
		c.Name AS email, 
		c.FirstName FirstName, 
		c.Fullname FullName,
		ls.Id AS ScheduleID, 
		l.RefNum LoanRefNum, 
		c.DaytimePhone DaytimePhone,
		c.MobilePhone MobilePhone,
		ls.Fees Fees,
		CAST(CASE WHEN o.EmailSendingAllowed IS NULL THEN 1 ELSE o.EmailSendingAllowed END AS BIT) EmailSendingAllowed,
		CAST(CASE WHEN o.MailSendingAllowed IS NULL THEN 1 ELSE o.EmailSendingAllowed END AS BIT) MailSendingAllowed,
		CAST(CASE WHEN o.SmsSendingAllowed IS NULL THEN 1 ELSE o.EmailSendingAllowed END AS BIT) SmsSendingAllowed
		
	FROM 
		LoanSchedule ls 
		INNER JOIN LateSchedules lls ON ls.LoanId = lls.LoanId AND ls.[Date]=lls.FirstLate
		INNER JOIN Loan l ON l.Id = ls.LoanId
		INNER JOIN Customer c ON l.CustomerId = c.Id 
		INNER JOIN CustomerStatuses cs ON cs.Id = c.CollectionStatus AND cs.IsEnabled = 1
		LEFT JOIN LoanOptions o ON o.LoanId = l.Id
END

GO