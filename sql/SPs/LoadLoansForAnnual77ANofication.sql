IF OBJECT_ID('LoadLoansForAnnual77ANofication') IS NULL
	EXECUTE('CREATE PROCEDURE LoadLoansForAnnual77ANofication AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON;
GO

ALTER PROCEDURE LoadLoansForAnnual77ANofication
	@Now DATETIME
AS
BEGIN
	
	;WITH last_notification_sent AS 
	(
		SELECT 
			max(cl.TimeStamp) LastSent, 
			cl.LoanID 
		FROM 
			CollectionLog cl 
		WHERE 
			Type='Annual77ANotification' 
		GROUP BY 
			cl.LoanID
	)
	SELECT c.OriginID, c.TypeOfBusiness, c.Id CustomerID, l.Id LoanID, ls.LastSent
	FROM 
		Loan l INNER JOIN Customer c ON l.CustomerId=c.Id 
		LEFT JOIN last_notification_sent ls ON ls.LoanID = l.Id
	WHERE
		c.IsTest=0 
	AND 	
		l.Status<>'PaidOff' 
	AND 
		l.[Date]<dateadd(year,-1,@Now)
	AND 
		l.[Date] > '2015-01-01'	--skip all old loans

END
GO

